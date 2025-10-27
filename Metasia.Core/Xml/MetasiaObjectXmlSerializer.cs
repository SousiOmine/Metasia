using System;
using System.Xml.Serialization;
using Metasia.Core.Objects;
using System.Text;
using Metasia.Core.Coordinate.InterpolationLogic;
using System.Reflection;

namespace Metasia.Core.Xml
{
    public class MetasiaObjectXmlSerializer
    {
        private static readonly Type[] includedTypes;

        static MetasiaObjectXmlSerializer()
        {
            var metasiaObjectType = typeof(IMetasiaObject);
            // ★ IInterpolationLogicの型も取得
            var interpolationLogicType = typeof(IInterpolationLogic);

            // 読み込まれている全てのアセンブリから型を検索
            // プラグイン読み込みに失敗したアセンブリがあっても継続できるようにエラーハンドリングを追加
            includedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // 部分的に読み込めた型のみを返す (nullを除外)
                        return ex.Types.OfType<Type>();
                    }
                    catch
                    {
                        // その他の例外の場合は空の配列を返す
                        return Array.Empty<Type>();
                    }
                })
                .Where(t => t != null && !t.IsInterface && !t.IsAbstract &&
                            // ★ IMetasiaObject または IInterpolationLogic を実装する型を全て検索
                            (metasiaObjectType.IsAssignableFrom(t) || interpolationLogicType.IsAssignableFrom(t)))
                .Distinct()
                .ToArray();
        }

        public static string Serialize(IMetasiaObject obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            Type objType = obj.GetType();
            XmlSerializer serializer = new XmlSerializer(objType, includedTypes);
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T Deserialize<T>(string xml) where T : class, IMetasiaObject
        {
            ArgumentNullException.ThrowIfNull(xml);
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentException("XMLが空です。", nameof(xml));
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T), includedTypes);
            using (var reader = new StringReader(xml))
            {
                var result = serializer.Deserialize(reader) as T;
                if (result is null)
                    throw new InvalidOperationException($"デシリアライズに失敗しました。型: {typeof(T).Name}");
                return result;
            }
        }
    }
}
