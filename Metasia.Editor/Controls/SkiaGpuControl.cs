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
    private readonly ConcurrentQueue<(SKImage Image, DateTime ReleaseTime)> _releaseQueue = new();
    private SKColor _backgroundColor = SKColors.Black;
    private volatile bool _isDisposed;

    /// <summary>
    /// 描画するイメージ
    /// </summary>
    public static readonly StyledProperty<SKImage?> ImageProperty =
        AvaloniaProperty.Register<SkiaGpuControl, SKImage?>(nameof(Image));

    /// <summary>
    /// 描画するイメージ
    /// </summary>
    public SKImage? Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
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
        AffectsRender<SkiaGpuControl>(ImageProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ImageProperty)
        {
            var oldImage = change.GetOldValue<SKImage?>();
            // 破棄中でなく、有効な古いイメージがある場合のみキューに入れる
            if (oldImage is not null && !_isDisposed)
            {
                // 古いイメージは即座に破棄せず、描画完了待ちの猶予を持たせてキューに入れる
                _releaseQueue.Enqueue((oldImage, DateTime.Now.AddMilliseconds(200)));
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
        context.Custom(new SkiaGpuDrawOperation(bounds, Image, _backgroundColor));
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
                    dequeuedItem.Image.Dispose();
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

        var currentImage = Image;
        SetCurrentValue(ImageProperty, null);

        System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
        {
            currentImage?.Dispose();

            while (_releaseQueue.TryDequeue(out var item))
            {
                item.Image.Dispose();
            }
        });
    }

    /// <summary>
    /// GPU描画を行うカスタム描画操作
    /// </summary>
    private class SkiaGpuDrawOperation : ICustomDrawOperation
    {
        private readonly SKImage? _image;
        private readonly SKColor _backgroundColor;

        public Rect Bounds { get; }

        public SkiaGpuDrawOperation(Rect bounds, SKImage? image, SKColor backgroundColor)
        {
            Bounds = bounds;
            _image = image;
            _backgroundColor = backgroundColor;
        }

        public void Dispose()
        {
            // イメージはControl側で管理されているため、ここでは破棄しない
        }

        public bool HitTest(Avalonia.Point p) => Bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other)
        {
            if (other is SkiaGpuDrawOperation op)
            {
                return Bounds == op.Bounds && ReferenceEquals(_image, op._image) && _backgroundColor == op._backgroundColor;
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

                if (_image is not null)
                {
                    var canvasWidth = (float)Bounds.Width;
                    var canvasHeight = (float)Bounds.Height;
                    var imageWidth = _image.Width;
                    var imageHeight = _image.Height;

                    if (imageWidth <= 0 || imageHeight <= 0)
                    {
                        return;
                    }

                    // アスペクト比を維持したままキャンバスにフィットさせる
                    var scaleX = canvasWidth / imageWidth;
                    var scaleY = canvasHeight / imageHeight;
                    var scale = Math.Min(scaleX, scaleY);

                    var scaledWidth = imageWidth * scale;
                    var scaledHeight = imageHeight * scale;

                    var x = (canvasWidth - scaledWidth) / 2;
                    var y = (canvasHeight - scaledHeight) / 2;

                    var destRect = new SKRect(x, y, x + scaledWidth, y + scaledHeight);

                    // GPU上で高品質なイメージ描画を行う
                    var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
                    using var paint = new SKPaint
                    {
                        IsAntialias = true
                    };

                    canvas.DrawImage(_image, destRect, sampling, paint);
                }
            }
            finally
            {
                canvas.Restore();
            }
        }
    }
}
