using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Globalization;

namespace Metasia.Editor.Controls
{
    /// <summary>
    /// タイムラインのルーラー（時間目盛り）を描画するコントロール。
    /// テーマ（ダーク/ライト）に応じて前景色を自動的に調整します。
    /// </summary>
    public class TimeRulerControl : Control
    {
        public static readonly StyledProperty<double> FramePerDIPProperty =
            AvaloniaProperty.Register<TimeRulerControl, double>(nameof(FramePerDIP), 10.0);

        public double FramePerDIP
        {
            get => GetValue(FramePerDIPProperty);
            set => SetValue(FramePerDIPProperty, value);
        }

        public static readonly StyledProperty<int> FrameRateProperty =
            AvaloniaProperty.Register<TimeRulerControl, int>(nameof(FrameRate), 60);

        public int FrameRate
        {
            get => GetValue(FrameRateProperty);
            set => SetValue(FrameRateProperty, value);
        }

        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            AvaloniaProperty.Register<TimeRulerControl, IBrush?>(nameof(Foreground), null);

        public IBrush? Foreground
        {
            get => GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        static TimeRulerControl()
        {
            AffectsRender<TimeRulerControl>(FramePerDIPProperty);
            AffectsRender<TimeRulerControl>(FrameRateProperty);
            AffectsRender<TimeRulerControl>(ForegroundProperty);
        }

        public TimeRulerControl()
        {
            // テーマ変更時に再描画
            ActualThemeVariantChanged += OnActualThemeVariantChanged;
        }

        private void OnActualThemeVariantChanged(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

        /// <summary>
        /// 現在のテーマに応じた前景色を取得します。
        /// Foreground プロパティが明示的に設定されている場合はその値を、
        /// そうでない場合はテーマに応じた色（ダーク時は白、ライト時は黒）を返します。
        /// </summary>
        private IBrush GetEffectiveForeground()
        {
            if (Foreground != null)
            {
                return Foreground;
            }

            // テーマに応じたデフォルト色
            var isDark = ActualThemeVariant == ThemeVariant.Dark;
            return isDark ? Brushes.White : Brushes.Black;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var width = Bounds.Width;
            var height = Bounds.Height;

            // 背景
            context.FillRectangle(Brushes.Transparent, new Rect(0, 0, width, height));

            var framePerDIP = FramePerDIP;
            var frameRate = FrameRate;
            if (framePerDIP <= 0 || frameRate <= 0) return;

            // 1秒あたりの幅
            double pixelsPerSecond = frameRate * framePerDIP;

            // 目盛りの間隔を決定
            // 最小でも10ピクセルくらいは空けたい
            double minInterval = 10.0;

            // 秒単位の描画
            // 1秒ごとに大きな目盛りとテキスト
            // 0.1秒ごとに小さな目盛り

            // 1秒ごとの線を描画
            var effectiveForeground = GetEffectiveForeground();
            var penMain = new Pen(effectiveForeground, 1);
            var penSub = new Pen(effectiveForeground, 1);
            var textBrush = effectiveForeground;
            var typeface = new Typeface(FontFamily.Default);

            // 表示範囲の計算（クリッピングされている場合は最適化できるが、今回は全範囲走査しつつ、描画範囲内かチェックする簡易実装）
            // 実際にはScrollViewerの中にあるため、Bounds.Widthは非常に大きい可能性がある（5000など）
            // 仮想化されていないため、全て描画すると重いが、とりあえず実装する。

            // 秒数でループ
            int totalSeconds = (int)(width / pixelsPerSecond) + 1;

            for (int sec = 0; sec < totalSeconds; sec++)
            {
                double x = sec * pixelsPerSecond;

                // 1秒の目盛り
                context.DrawLine(penMain, new Point(x, height), new Point(x, height - 20));

                // 時間テキスト
                var timeText = TimeSpan.FromSeconds(sec).ToString(@"hh\:mm\:ss");
                var formattedText = new FormattedText(
                    timeText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    12,
                    textBrush
                );

                // 基本は中央揃え（X - 幅/2）
                double textX = x - (formattedText.Width / 2);

                // ただし、0秒地点（左端）の場合は見切れないように左揃え気味にする
                if (textX < 0)
                {
                    textX = x;
                }

                context.DrawText(formattedText, new Point(textX, 5));

                // サブ目盛り (0.1秒ごと)
                // 間隔が狭すぎる場合は描画しない
                if (pixelsPerSecond / 10.0 > minInterval)
                {
                    for (int sub = 1; sub < 10; sub++)
                    {
                        double subX = x + (sub * pixelsPerSecond / 10.0);
                        if (subX > width) break;

                        double lineLength = (sub == 5) ? 15 : 10; // 0.5秒は少し長く
                        context.DrawLine(penSub, new Point(subX, height), new Point(subX, height - lineLength));
                    }
                }
                else if (pixelsPerSecond / 2.0 > minInterval)
                {
                    // 0.5秒ごとだけ描画
                    double subX = x + (pixelsPerSecond / 2.0);
                    if (subX <= width)
                    {
                        context.DrawLine(penSub, new Point(subX, height), new Point(subX, height - 15));
                    }
                }
            }
        }
    }
}
