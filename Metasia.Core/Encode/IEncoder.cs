namespace Metasia.Core.Encode;

/// <summary>
/// プロジェクトをエンコードするオブジェクトの抽象化インターフェース
/// </summary>
public interface IEncoder
{
    /// <summary>
    /// エンコード過程の進捗率を0~100で表す
    /// </summary>
    double ProgressRate { get; }
    
    /// <summary>
    /// エンコードの進捗に変化があった時に呼び出される
    /// </summary>
    event EventHandler<EventArgs> StatusChanged;

    /// <summary>
    /// エンコードが開始された時に呼び出される
    /// </summary>
    event EventHandler<EventArgs> EncodeStarted;

    /// <summary>
    /// エンコードが正常に終了した時に呼び出される
    /// </summary>
    event EventHandler<EventArgs> EncodeCompleted;

    /// <summary>
    /// エンコードが異常終了した時に呼び出される
    /// </summary>
    event EventHandler<EventArgs> EncodeFailed;

    /// <summary>
    /// エンコードを開始する
    /// </summary>
    void Start();
    
    /// <summary>
    /// エンコードを中止する
    /// </summary>
    void CancelRequest();
}