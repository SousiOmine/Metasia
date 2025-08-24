using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Metasia.Core.Objects;

namespace Metasia.Core.Xml
{
    public class TimelineXmlSerializer
    {
        private static readonly Type[] includedTypes;
        
        static TimelineXmlSerializer()
        {
            var baseInterface = typeof(IMetasiaObject);
            // IMetasiaObjectを実装する、インターフェースや抽象クラスではないすべての型を検索
            includedTypes = baseInterface.Assembly.GetTypes()
                .Where(t => baseInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToArray();
        }

        public static string SerializeTimeline(TimelineObject timeline)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TimelineObject), includedTypes);
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, timeline);
                return writer.ToString();
            }
        }

        public static TimelineObject DeserializeTimeline(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TimelineObject), includedTypes);
            using (var reader = new StringReader(xml))
            {
                var timeline = serializer.Deserialize(reader) as TimelineObject;
                if (timeline is null) throw new InvalidOperationException("タイムラインのデシリアライズに失敗しました");
                return timeline;
            }
        }
    }
}