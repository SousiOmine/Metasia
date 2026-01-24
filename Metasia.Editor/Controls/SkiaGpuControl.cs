using System;
using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Styling;
using Avalonia.Threading;
using SkiaSharp;

namespace Metasia.Editor.Controls;

/// <summary>
/// SkiaSharpを使用してGPU描画を行うカスタムコントロール。
/// Avaloniaの ICustomDrawOperation と ISkiaSharpApiLeaseFeature を使用して
/// 高速なGPU描画を実現します。
/// また、ビットマップのライフサイクル管理（遅延破棄）を行い、
/// レンダリングスレッドとの競合を防ぎます。
/// テーマ（ダーク/ライト）に応じた背景色を自動的に使用します。
/// </summary>
public class SkiaGpuControl : Control, IDisposable
{
    private readonly ConcurrentQueue<(SKBitmap Bitmap, DateTime ReleaseTime)> _releaseQueue = new();
    private SKColor _backgroundColor = SKColors.Black;
    private volatile bool _isDisposed;

    /// <summary>
    /// 描画するビットマップ
    /// </summary>
    public static readonly StyledProperty<SKBitmap?> BitmapProperty =
        AvaloniaProperty.Register<SkiaGpuControl, SKBitmap?>(nameof(Bitmap));

    /// <summary>
    /// 描画するビットマップ
    /// </summary>
    public SKBitmap? Bitmap
    {
        get => GetValue(BitmapProperty);
        set => SetValue(BitmapProperty, value);
    }

    public SkiaGpuControl()
    {
        ClipToBounds = true;
        UpdateBackgroundColor();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        UpdateBackgroundColor();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ActualThemeVariantChanged -= OnActualThemeVariantChanged;
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        UpdateBackgroundColor();
        InvalidateVisual();
    }

    private void UpdateBackgroundColor()
    {
        // テーマに応じた背景色を設定
        var isDark = ActualThemeVariant == ThemeVariant.Dark;
        _backgroundColor = isDark ? SKColors.Black : SKColors.White;
    }

    static SkiaGpuControl()
    {
        AffectsRender<SkiaGpuControl>(BitmapProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BitmapProperty)
        {
            var oldBitmap = change.GetOldValue<SKBitmap?>();
            // 破棄中でなく、有効な古いビットマップがある場合のみキューに入れる
            if (oldBitmap is not null && !_isDisposed)
            {
                // 古いビットマップは即座に破棄せず、描画完了待ちの猶予を持たせてキューに入れる
                _releaseQueue.Enqueue((oldBitmap, DateTime.Now.AddMilliseconds(200)));
            }
        }
    }

    /// <summary>
    /// Avaloniaの描画パイプラインをオーバーライドしてカスタム描画を行う
    /// </summary>
    public override void Render(DrawingContext context)
    {
        ProcessReleaseQueue();

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.Custom(new SkiaGpuDrawOperation(bounds, Bitmap, _backgroundColor));
    }

    /// <summary>
    /// サーフェスを無効化して再描画を要求する
    /// </summary>
    public void InvalidateSurface()
    {
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
    }

    private void ProcessReleaseQueue()
    {
        var now = DateTime.Now;
        while (_releaseQueue.TryPeek(out var item))
        {
            if (now >= item.ReleaseTime)
            {
                if (_releaseQueue.TryDequeue(out var dequeuedItem))
                {
                    dequeuedItem.Bitmap.Dispose();
                }
            }
            else
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        // 複数回のDispose呼び出しを防ぐ
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        var currentBitmap = Bitmap;
        SetCurrentValue(BitmapProperty, null);

        System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
        {
            currentBitmap?.Dispose();

            while (_releaseQueue.TryDequeue(out var item))
            {
                item.Bitmap.Dispose();
            }
        });
    }

    /// <summary>
    /// GPU描画を行うカスタム描画操作
    /// </summary>
    private class SkiaGpuDrawOperation : ICustomDrawOperation
    {
        private readonly SKBitmap? _bitmap;
        private readonly SKColor _backgroundColor;

        public Rect Bounds { get; }

        public SkiaGpuDrawOperation(Rect bounds, SKBitmap? bitmap, SKColor backgroundColor)
        {
            Bounds = bounds;
            _bitmap = bitmap;
            _backgroundColor = backgroundColor;
        }

        public void Dispose()
        {
            // ビットマップはControl側で管理されているため、ここでは破棄しない
        }

        public bool HitTest(Avalonia.Point p) => Bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other)
        {
            if (other is SkiaGpuDrawOperation op)
            {
                return Bounds == op.Bounds && ReferenceEquals(_bitmap, op._bitmap) && _backgroundColor == op._backgroundColor;
            }
            return false;
        }

        /// <summary>
        /// GPU描画を実行する
        /// ImmediateDrawingContext から ISkiaSharpApiLeaseFeature を取得して
        /// SkiaSharpのキャンバスに直接描画を行う
        /// </summary>
        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature is null)
            {
                // GPU描画が利用できない場合は何もしない（フォールバック処理が必要な場合はここに実装）
                return;
            }

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            if (canvas is null) return;

            canvas.Save();
            try
            {
                // テーマに応じた背景色でクリア（背景透過を防ぐ）
                canvas.Clear(_backgroundColor);

                if (_bitmap is not null)
                {
                    var canvasWidth = (float)Bounds.Width;
                    var canvasHeight = (float)Bounds.Height;
                    var bitmapWidth = _bitmap.Width;
                    var bitmapHeight = _bitmap.Height;

                    if (bitmapWidth <= 0 || bitmapHeight <= 0)
                    {
                        return;
                    }

                    // アスペクト比を維持したままキャンバスにフィットさせる
                    var scaleX = canvasWidth / bitmapWidth;
                    var scaleY = canvasHeight / bitmapHeight;
                    var scale = Math.Min(scaleX, scaleY);

                    var scaledWidth = bitmapWidth * scale;
                    var scaledHeight = bitmapHeight * scale;

                    var x = (canvasWidth - scaledWidth) / 2;
                    var y = (canvasHeight - scaledHeight) / 2;

                     var destRect = new SKRect(x, y, x + scaledWidth, y + scaledHeight);

                     // GPU上で高品質なビットマップ描画を行う
                     var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
                     using var paint = new SKPaint
                     {
                         IsAntialias = true
                     };

                    using var image = SKImage.FromBitmap(_bitmap);
                    canvas.DrawImage(image, destRect, sampling, paint);
                 }
             }
             finally
             {
                 canvas.Restore();
             }
         }
    }
}
