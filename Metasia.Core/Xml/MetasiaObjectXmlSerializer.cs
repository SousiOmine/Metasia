using System;
using System.Xml.Serialization;
using Metasia.Core.Objects;
using System.Text;
using Metasia.Core.Coordinate.InterpolationLogic;
using System.Reflection;
using System.Xml;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.VisualEffects;

namespace Metasia.Core.Xml
{
    public class MetasiaObjectXmlSerializer
    {
        internal static TypeRegistry Registry { get; private set; } = new();

        public static void Initialize(TypeRegistry registry)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
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
            var xmlSerializer = new XmlSerializer(obj.GetType(), Registry.GetAllRegisteredTypes());
            using (var writer = doc.CreateWriter())
            {
                xmlSerializer.Serialize(writer, obj);
            }

            var root = doc.Root;
            var typeId = Registry.GetTypeId(obj.GetType());
            if (typeId is not null && root is not null)
            {
                root.SetAttributeValue("typeId", typeId);
            }
            var processedRoot = PostProcessSerialized(root!);
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

            XmlSerializer serializer = new XmlSerializer(typeof(T), Registry.GetAllRegisteredTypes());
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
            
            // UnknownObjectであれば元のxmlで保存
            if (element.Name.LocalName == nameof(UnknownClipObject) || 
                element.Name.LocalName == nameof(UnknownVisualEffect) ||
                element.Name.LocalName == nameof(UnknownAudioEffect))
            {
                Type targetType = element.Name.LocalName switch
                {
                    nameof(UnknownClipObject) => typeof(UnknownClipObject),
                    nameof(UnknownVisualEffect) => typeof(UnknownVisualEffect),
                    nameof(UnknownAudioEffect) => typeof(UnknownAudioEffect),
                    _ => throw new InvalidOperationException($"UnknownObjectの型が不正です。型名: {element.Name.LocalName}")
                };
                var serializer = new XmlSerializer(targetType);
                using var reader = element.CreateReader();
                var unknownObject = serializer.Deserialize(reader) as IUnknownMetasiaObject;
                if (unknownObject is not null)
                {
                    if (unknownObject is ClipObject) element.SetAttributeValue("typeKind", "Clip");
                    else if (unknownObject is VisualEffectBase) element.SetAttributeValue("typeKind", "VisualEffect");
                    else if (unknownObject is AudioEffectBase) element.SetAttributeValue("typeKind", "AudioEffect");
                    return XElement.Parse(unknownObject.RawXml);
                }
                
            }
            foreach (var node in element.Elements())
            {
                var childTypeId = Registry.GetTypeIdByTypeName(node.Name.LocalName);
                if (childTypeId is not null)
                {
                    node.SetAttributeValue("typeId", childTypeId);
                }
                
                PostProcessSerialized(node);
            }
            return element;
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
                    
                    return new XElement(unknownElementName, element);
                }
            }

            foreach (var node in element.Elements())
            {
                PreProcessDeserialized(node);
            }
            return element;
        }
    }
}
