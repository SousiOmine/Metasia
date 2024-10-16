using Metasia.Core.Graphics;
using SkiaSharp;

namespace Metasia.Core.Render;

public class DrawExpresserArgs : IDisposable
{
	/// <summary>
	/// 描写結果が格納されるSKBitmap
	/// </summary>
	public SKBitmap? Bitmap;

	/// <summary>
	/// 実際にレンダリングする解像度
	/// </summary>
	public SKSize ActualResolution;
	
	/// <summary>
	/// レイアウトに使用する解像度
	/// </summary>
	public SKSize TargetResolution;
	
	/// <summary>
	/// targetSizeの値を1としたとき、実際にレンダリングする解像度の比率
	/// たとえば、targetSizeが1920*1080の場合、ResolutionLevelが0.5であればレンダリング解像度は960*540になる
	/// </summary>
	public double ResolutionLevel;

	/// <summary>
	/// 実際のSKBitmapのサイズ
	/// </summary>
	public SKSize? ActualSize;
	
	/// <summary>
	/// レイアウト時の配置に使うサイズ
	/// </summary>
	public SKSize? TargetSize;
    
	/// <summary>
	/// レンダリングの目標フレームレート
	/// </summary>
    public int FPS;
    
    public void Dispose()
    {
        if(Bitmap is not null) Bitmap.Dispose();
    }
}