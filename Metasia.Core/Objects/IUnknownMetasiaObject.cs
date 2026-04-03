using System.Xml.Serialization;

namespace Metasia.Core.Objects;

/// <summary>
/// 型不明のオブジェクトを表すクラス
/// </summary>
public interface IUnknownMetasiaObject : IMetasiaObject
{
    [XmlIgnore]
    /// <summary>
    /// オブジェクトの生のXMLデータ
    /// </summary>
    string RawXml { get; }
}
