namespace Metasia.Core.Objects.Parameters;

/// <summary>
/// 対象レイヤーの指定を行うクラス
/// 無限（全ての下位レイヤー）または特定のレイヤー数を指定可能
/// </summary>
public class LayerTarget
{
    /// <summary>
    /// 無限に対象レイヤーを指定するかどうか
    /// trueの場合、全ての下位レイヤーが対象となる
    /// </summary>
    public bool IsInfinite { get; set; }

    /// <summary>
    /// IsInfiniteがfalseの場合の対象レイヤー数
    /// 1以上の値が有効
    /// </summary>
    public int LayerCount { get; set; }

    /// <summary>
    /// 無限レイヤーターゲットを作成
    /// </summary>
    public static LayerTarget Infinite { get; } = new() { IsInfinite = true, LayerCount = 0 };

    /// <summary>
    /// デフォルトコンストラクタ
    /// 無限レイヤー（IsInfinite=true）として初期化
    /// </summary>
    public LayerTarget()
    {
        IsInfinite = true;
        LayerCount = 0;
    }

    /// <summary>
    /// レイヤー数を指定して作成
    /// </summary>
    /// <param name="count">1以上のレイヤー数</param>
    public LayerTarget(int count)
    {
        if (count < 0)
        {
            IsInfinite = true;
            LayerCount = 0;
        }
        else if (count == 0)
        {
            IsInfinite = true;
            LayerCount = 0;
        }
        else
        {
            IsInfinite = false;
            LayerCount = count;
        }
    }

    /// <summary>
    /// レンダリングで使用するスコープレイヤー数を取得
    /// 無限の場合はint.MaxValue、そうでない場合はLayerCountを返す
    /// </summary>
    public int ToScopeCount()
    {
        return IsInfinite ? int.MaxValue : LayerCount;
    }

    /// <summary>
    /// 現在の設定値に基づいて新しいLayerTargetインスタンスを作成
    /// </summary>
    public LayerTarget Clone()
    {
        return new LayerTarget
        {
            IsInfinite = IsInfinite,
            LayerCount = LayerCount
        };
    }

    public override string ToString()
    {
        return IsInfinite ? "Infinite" : $"Count: {LayerCount}";
    }
}
