using System;
using System.Threading;
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
/// 画像のライフサイクルは参照カウントで管理し、
/// レンダリングスレッドとの競合を防ぎます。
/// テーマ（ダーク/ライト）に応じた背景色を自動的に使用します。
/// </summary>
public class SkiaGpuControl : Control, IDisposable
{
    private readonly Lock _imageLock = new();
    private RefCountedImage? _currentImage;
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
            RefCountedImage? oldRef = null;
            var newImage = change.GetNewValue<SKImage?>();

            lock (_imageLock)
            {
                oldRef = _currentImage;

                if (_isDisposed || newImage is null)
                {
                    _currentImage = null;
                }
                else if (oldRef is not null && ReferenceEquals(oldRef.Image, newImage))
                {
                    // 同一イメージ参照の再代入はライフサイクルを変更しない
                    oldRef = null;
                }
                else
                {
                    _currentImage = new RefCountedImage(newImage);
                }
            }

            oldRef?.Release();

            if (_isDisposed && newImage is not null)
            {
                newImage.Dispose();
            }
        }
    }

    /// <summary>
    /// Avaloniaの描画パイプラインをオーバーライドしてカスタム描画を行う
    /// </summary>
    public override void Render(DrawingContext context)
    {
        RefCountedImage? imageRef = null;
        lock (_imageLock)
        {
            imageRef = _currentImage?.TryAddRef();
        }

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.Custom(new SkiaGpuDrawOperation(bounds, imageRef, _backgroundColor));
    }

    /// <summary>
    /// サーフェスを無効化して再描画を要求する
    /// </summary>
    public void InvalidateSurface()
    {
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
    }

    public void Dispose()
    {
        // 複数回のDispose呼び出しを防ぐ
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        SetCurrentValue(ImageProperty, null);
    }

    /// <summary>
    /// GPU描画を行うカスタム描画操作
    /// </summary>
    private class SkiaGpuDrawOperation : ICustomDrawOperation
    {
        private readonly RefCountedImage? _imageRef;
        private readonly SKColor _backgroundColor;
        private bool _disposed;

        public Rect Bounds { get; }

        public SkiaGpuDrawOperation(Rect bounds, RefCountedImage? imageRef, SKColor backgroundColor)
        {
            Bounds = bounds;
            _imageRef = imageRef;
            _backgroundColor = backgroundColor;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _imageRef?.Release();
        }

        public bool HitTest(Avalonia.Point p) => Bounds.Contains(p);

        public bool Equals(ICustomDrawOperation? other)
        {
            if (other is SkiaGpuDrawOperation op)
            {
                return Bounds == op.Bounds
                    && ReferenceEquals(_imageRef?.Image, op._imageRef?.Image)
                    && _backgroundColor == op._backgroundColor;
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

                var image = _imageRef?.Image;
                if (image is not null)
                {
                    var canvasWidth = (float)Bounds.Width;
                    var canvasHeight = (float)Bounds.Height;
                    var imageWidth = image.Width;
                    var imageHeight = image.Height;

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

                    canvas.DrawImage(image, destRect, sampling, paint);
                }
            }
            finally
            {
                canvas.Restore();
            }
        }
    }

    private sealed class RefCountedImage
    {
        private int _refCount = 1;

        public RefCountedImage(SKImage image)
        {
            Image = image;
        }

        public SKImage Image { get; }

        public RefCountedImage? TryAddRef()
        {
            while (true)
            {
                var currentCount = Volatile.Read(ref _refCount);
                if (currentCount == 0)
                {
                    return null;
                }

                if (Interlocked.CompareExchange(ref _refCount, currentCount + 1, currentCount) == currentCount)
                {
                    return this;
                }
            }
        }

        public void Release()
        {
            var remaining = Interlocked.Decrement(ref _refCount);
            if (remaining == 0)
            {
                Image.Dispose();
                return;
            }

            if (remaining < 0)
            {
                throw new InvalidOperationException("RefCountedImage.Release was called too many times.");
            }
        }
    }
}
