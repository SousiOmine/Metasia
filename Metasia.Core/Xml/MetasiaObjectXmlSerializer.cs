using System;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate.InterpolationLogic;
using System.Xml.Linq;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.VisualEffects;

namespace Metasia.Core.Xml
{
    public class MetasiaObjectXmlSerializer
    {
        private static readonly XNamespace XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly object SerializerCacheLock = new();

        internal static TypeRegistry Registry { get; private set; } = new();
        private static readonly Dictionary<Type, XmlSerializer> SerializerCache = new();
        private static XmlAttributeOverrides? _attributeOverrides;

        public static void Initialize(TypeRegistry registry)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
            lock (SerializerCacheLock)
            {
                SerializerCache.Clear();
                _attributeOverrides = null;
            }
        }

        static MetasiaObjectXmlSerializer()
        {
            // デフォルトでMetasia.Coreの型を登録
            Registry.RegisterAssemblyTypes("metasia/core", typeof(IMetasiaObject).Assembly);
            Registry.RegisterAssemblyTypes("metasia/core", typeof(IInterpolationLogic).Assembly);
        }

        public static string Serialize(IMetasiaObject obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            var doc = new XDocument();
            var xmlSerializer = GetSerializer(obj.GetType());
            using (var writer = doc.CreateWriter())
            {
                xmlSerializer.Serialize(writer, obj);
            }

            var processedRoot = PostProcessSerialized(doc.Root!);
            ApplySerializedTypeMetadata(processedRoot, obj.GetType());
            return processedRoot.ToString();
        }

        public static T Deserialize<T>(string xml) where T : class, IMetasiaObject
        {
            ArgumentNullException.ThrowIfNull(xml);
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentException("XMLが空です。", nameof(xml));
            }
        
            var doc = XDocument.Parse(xml);
            doc = new XDocument(PreProcessDeserialized(doc.Root!));

            XmlSerializer serializer = GetSerializer(typeof(T));
            using (var reader = new StringReader(doc.ToString()))
            {
                var result = serializer.Deserialize(reader) as T;
                if (result is null)
                    throw new InvalidOperationException($"デシリアライズに失敗しました。型: {typeof(T).Name}");
                return result;
            }
        }

        /// <summary>
        /// 不明型を元のxmlで保存したり、要素に属性を追加したりする
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static XElement PostProcessSerialized(XElement? element)
        {
            if (element is null) return new XElement("null");

            if (TryRestoreUnknownElement(element, out var restoredUnknownElement))
            {
                return restoredUnknownElement;
            }

            var processed = new XElement(element.Name, element.Attributes());
            foreach (var node in element.Nodes())
            {
                if (node is XElement childElement)
                {
                    var processedChild = PostProcessSerialized(childElement);
                    ApplySerializedTypeMetadata(processedChild);
                    processed.Add(processedChild);
                }
                else
                {
                    processed.Add(CloneNode(node));
                }
            }

            return processed;
        }

        private static XElement PreProcessDeserialized(XElement element)
        {
            string? typeId = element.Attribute("typeId")?.Value;
            string? typeKind = element.Attribute("typeKind")?.Value;
            
            if (typeId is not null)
            {
                var type = Registry.GetType(typeId);
                if (type is null && typeKind is not null)
                {
                    string unknownElementName = typeKind switch
                    {
                        "Clip" => nameof(UnknownClipObject),
                        "VisualEffect" => nameof(UnknownVisualEffect),
                        "AudioEffect" => nameof(UnknownAudioEffect),
                        _ => throw new InvalidOperationException($"Unknown typeKind: {typeKind}")
                    };

                    return new XElement(unknownElementName, new XElement(element));
                }
            }

            var processed = new XElement(element.Name, element.Attributes());
            foreach (var node in element.Nodes())
            {
                if (node is XElement childElement)
                {
                    processed.Add(PreProcessDeserialized(childElement));
                }
                else
                {
                    processed.Add(CloneNode(node));
                }
            }

            return processed;
        }

        private static bool TryRestoreUnknownElement(XElement element, out XElement restoredElement)
        {
            restoredElement = null!;
            string? unknownWrapperTypeName = ResolveSerializedTypeName(element);
            string? typeKind = unknownWrapperTypeName is null ? null : GetTypeKindFromUnknownWrapperName(unknownWrapperTypeName);
            if (typeKind is null)
            {
                return false;
            }

            Type? wrapperType = unknownWrapperTypeName switch
            {
                nameof(UnknownClipObject) => typeof(UnknownClipObject),
                nameof(UnknownVisualEffect) => typeof(UnknownVisualEffect),
                nameof(UnknownAudioEffect) => typeof(UnknownAudioEffect),
                _ => null
            };

            if (wrapperType is null)
            {
                return false;
            }

            var serializer = GetSerializer(wrapperType);
            using var reader = element.CreateReader();
            if (serializer.Deserialize(reader) is not IUnknownMetasiaObject unknownObject || string.IsNullOrWhiteSpace(unknownObject.RawXml))
            {
                return false;
            }

            restoredElement = XElement.Parse(unknownObject.RawXml);
            restoredElement.SetAttributeValue("typeKind", typeKind);
            return true;
        }

        private static void ApplySerializedTypeMetadata(XElement element, Type? actualType = null)
        {
            actualType ??= ResolveSerializedType(element);

            if (actualType is null)
            {
                return;
            }

            var typeId = Registry.GetTypeId(actualType);
            if (typeId is not null)
            {
                element.SetAttributeValue("typeId", typeId);
            }

            var typeKind = GetTypeKind(actualType);
            if (typeKind is not null)
            {
                element.SetAttributeValue("typeKind", typeKind);
            }
        }

        private static Type? ResolveSerializedType(XElement element)
        {
            var typeName = ResolveSerializedTypeName(element);
            return typeName is null ? null : Registry.GetTypeByTypeName(typeName);
        }

        private static string? ResolveSerializedTypeName(XElement element)
        {
            var xsiTypeValue = element.Attribute(XsiNamespace + "type")?.Value;
            return xsiTypeValue?.Split(':').LastOrDefault() ?? element.Name.LocalName;
        }

        private static string? GetTypeKind(Type type)
        {
            if (typeof(ClipObject).IsAssignableFrom(type))
            {
                return "Clip";
            }

            if (typeof(VisualEffectBase).IsAssignableFrom(type))
            {
                return "VisualEffect";
            }

            if (typeof(AudioEffectBase).IsAssignableFrom(type))
            {
                return "AudioEffect";
            }

            return null;
        }

        private static string? GetTypeKindFromUnknownWrapperName(string elementName)
        {
            return elementName switch
            {
                nameof(UnknownClipObject) => "Clip",
                nameof(UnknownVisualEffect) => "VisualEffect",
                nameof(UnknownAudioEffect) => "AudioEffect",
                _ => null
            };
        }

        private static object CloneNode(XNode node)
        {
            return node switch
            {
                XCData cdata => new XCData(cdata.Value),
                XText text => new XText(text.Value),
                XComment comment => new XComment(comment.Value),
                XProcessingInstruction instruction => new XProcessingInstruction(instruction.Target, instruction.Data),
                XDocumentType documentType => new XDocumentType(documentType.Name, documentType.PublicId, documentType.SystemId, documentType.InternalSubset),
                _ => throw new NotSupportedException($"Unsupported XML node type: {node.GetType().Name}")
            };
        }

        private static XmlSerializer GetSerializer(Type rootType)
        {
            lock (SerializerCacheLock)
            {
                if (SerializerCache.TryGetValue(rootType, out var cached))
                {
                    return cached;
                }

                var serializer = new XmlSerializer(
                    rootType,
                    GetAttributeOverrides(),
                    Registry.GetAllRegisteredTypes(),
                    root: null,
                    defaultNamespace: null);

                SerializerCache[rootType] = serializer;
                return serializer;
            }
        }

        private static XmlAttributeOverrides GetAttributeOverrides()
        {
            if (_attributeOverrides is not null)
            {
                return _attributeOverrides;
            }

            var overrides = new XmlAttributeOverrides();
            foreach (var type in Registry.GetAllRegisteredTypes())
            {
                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (property.GetIndexParameters().Length > 0 || !property.CanRead)
                    {
                        continue;
                    }

                    if (!TryGetCollectionElementType(property.PropertyType, out var elementType))
                    {
                        continue;
                    }

                    var xmlAttributes = CreateCollectionItemOverrides(elementType);
                    if (xmlAttributes is null)
                    {
                        continue;
                    }

                    overrides.Add(type, property.Name, xmlAttributes);
                }
            }

            _attributeOverrides = overrides;
            return overrides;
        }

        private static bool TryGetCollectionElementType(Type propertyType, out Type elementType)
        {
            elementType = null!;

            if (propertyType.IsArray)
            {
                elementType = propertyType.GetElementType()!;
                return true;
            }

            if (propertyType.IsGenericType)
            {
                var genericArguments = propertyType.GetGenericArguments();
                if (genericArguments.Length == 1 && typeof(IEnumerable).IsAssignableFrom(propertyType))
                {
                    elementType = genericArguments[0];
                    return true;
                }
            }

            return false;
        }

        private static XmlAttributes? CreateCollectionItemOverrides(Type elementType)
        {
            if (elementType == typeof(ClipObject))
            {
                return CreateXmlArrayItemAttributes(typeof(ClipObject), typeof(UnknownClipObject));
            }

            if (elementType == typeof(AudioEffectBase))
            {
                return CreateXmlArrayItemAttributes(typeof(AudioEffectBase), typeof(UnknownAudioEffect));
            }

            if (elementType == typeof(VisualEffectBase))
            {
                return CreateXmlArrayItemAttributes(typeof(VisualEffectBase), typeof(UnknownVisualEffect));
            }

            return null;
        }

        private static XmlAttributes CreateXmlArrayItemAttributes(Type baseType, Type unknownType)
        {
            var attributes = new XmlAttributes();
            attributes.XmlArrayItems.Add(new XmlArrayItemAttribute(baseType.Name, baseType));
            attributes.XmlArrayItems.Add(new XmlArrayItemAttribute(unknownType.Name, unknownType));
            return attributes;
        }
    }
}
