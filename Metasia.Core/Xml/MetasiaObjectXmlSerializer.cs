using System.Xml.Serialization;
using Metasia.Core.Objects;
using System.Text;

namespace Metasia.Core.Xml
{
    public class MetasiaObjectXmlSerializer
    {
        private static readonly Type[] includedTypes;
        
        static MetasiaObjectXmlSerializer()
        {
            var baseInterface = typeof(IMetasiaObject);
            // IMetasiaObjectを実装する、インターフェースや抽象クラスではないすべての型を検索
            includedTypes = baseInterface.Assembly.GetTypes()
                .Where(t => baseInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToArray();
        }

        public static string Serialize(IMetasiaObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

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
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentException("XML cannot be null or empty", nameof(xml));

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