using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Linq;
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

        /// <summary>
        /// テキストラベル間の最小間隔（ピクセル）。
        /// この値を大きくすると、より粗い粒度で時間が表示されます。
        /// </summary>
        public static readonly StyledProperty<double> MinTextIntervalProperty =
            AvaloniaProperty.Register<TimeRulerControl, double>(nameof(MinTextInterval), 80.0);

        public double MinTextInterval
        {
            get => GetValue(MinTextIntervalProperty);
            set => SetValue(MinTextIntervalProperty, value);
        }

        /// <summary>
        /// 目盛り線間の最小間隔（ピクセル）。
        /// </summary>
        public static readonly StyledProperty<double> MinTickIntervalProperty =
            AvaloniaProperty.Register<TimeRulerControl, double>(nameof(MinTickInterval), 10.0);

        public double MinTickInterval
        {
            get => GetValue(MinTickIntervalProperty);
            set => SetValue(MinTickIntervalProperty, value);
        }

        static TimeRulerControl()
        {
            AffectsRender<TimeRulerControl>(FramePerDIPProperty);
            AffectsRender<TimeRulerControl>(FrameRateProperty);
            AffectsRender<TimeRulerControl>(ForegroundProperty);
            AffectsRender<TimeRulerControl>(MinTextIntervalProperty);
            AffectsRender<TimeRulerControl>(MinTickIntervalProperty);
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

        // 標準的な時間間隔のリスト
        private static readonly double[] StandardIntervals = new double[]
        {
            0.01, 0.025, 0.05,
            0.1, 0.25, 0.5,
            1, 2.5, 5,
            10, 15, 30,
            60,       // 1分
            120,      // 2分
            300,      // 5分
            600,      // 10分
            900,      // 15分
            1800,     // 30分
            3600,     // 1時間
            7200,     // 2時間
            10800,    // 3時間
            21600,    // 6時間
            43200,    // 12時間
            86400     // 24時間
        };

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

            // テキストの最小間隔（ピクセル）
            double minTextInterval = MinTextInterval;
            // 目盛りの最小間隔（ピクセル）
            double minTickInterval = MinTickInterval;

            var effectiveForeground = GetEffectiveForeground();
            var penMain = new Pen(effectiveForeground, 1);
            var penSub = new Pen(effectiveForeground, 0.5);
            var textBrush = effectiveForeground;
            var typeface = new Typeface(FontFamily.Default);

            // 1. 最小目盛り間隔(tickInterval)を決定
            // 定義された間隔リストのうち、minTickIntervalを満たす最小のものを選ぶ
            double tickInterval = StandardIntervals.Last();
            foreach (var interval in StandardIntervals)
            {
                if (interval * pixelsPerSecond >= minTickInterval)
                {
                    tickInterval = interval;
                    break;
                }
            }

            // 2. ラベル表示間隔(labelInterval)を決定
            // tickIntervalの整数倍であり、かつminTextIntervalを満たす最小のものを選ぶ
            double labelInterval = tickInterval;
            foreach (var interval in StandardIntervals)
            {
                if (interval < tickInterval) continue; // tickIntervalより小さいものは無視

                // 整数倍チェック (浮動小数点誤差を考慮)
                double ratio = interval / tickInterval;
                bool isIntegerMultiple = Math.Abs(ratio - Math.Round(ratio)) < 0.001;

                if (!isIntegerMultiple) continue;

                // フォーマット判定して必要幅を調整
                var tempFormat = GetTimeFormat(interval);
                double requiredSpacing = minTextInterval;
                // 短いテキストなら少し詰めても良い
                if (tempFormat != TimeFormat.SubSeconds)
                {
                    requiredSpacing *= 0.6;
                }

                if (interval * pixelsPerSecond >= requiredSpacing)
                {
                    labelInterval = interval;
                    break;
                }
            }

            // 決定された間隔に基づいて描画
            // 画面範囲に含まれる tick を計算
            // 今回はシンプルに0から描画（将来的にオフセット対応が必要なら修正）
            // 画面幅に入る最大の目盛りインデックス
            double totalSecondsWidth = width / pixelsPerSecond;
            int maxTickIndex = (int)(totalSecondsWidth / tickInterval) + 1;

            for (int i = 0; i <= maxTickIndex; i++)
            {
                double seconds = i * tickInterval;
                double x = seconds * pixelsPerSecond;
                if (x > width) break;

                // ラベル目盛りかどうか判定
                // labelInterval の倍数かどうか（誤差許容）
                double ratio = seconds / labelInterval;
                bool isLabelTick = Math.Abs(ratio - Math.Round(ratio)) < 0.001;

                if (isLabelTick)
                {
                    // メイン目盛り
                    context.DrawLine(penMain, new Point(x, height), new Point(x, height - 20));

                    // 時間テキスト
                    var timeFormat = GetTimeFormat(labelInterval); // ラベル間隔に応じたフォーマット
                    // ※0秒などは上位フォーマットで表示したほうが自然な場合もあるが、
                    // ここでは統一して labelInterval 基準のフォーマットとする
                    var timeText = FormatTime(seconds, timeFormat);

                    var formattedText = new FormattedText(
                        timeText,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        12,
                        textBrush
                    );

                    // 中央揃え
                    double textX = x - (formattedText.Width / 2);
                    if (textX < 0) textX = x; // 左端補正

                    context.DrawText(formattedText, new Point(textX, 5));
                }
                else
                {
                    // サブ目盛り
                    // tickIntervalの時点で minTickInterval は満たしているので描画してOK
                    double lineLength = 10;

                    // 中間点（ハーフ）の強調
                    // labelInterval との中間地点なら長くする実装も可能だが、
                    // 汎用ロジックでは複雑になるため、単純な小目盛りとする。
                    // もし "5分割の真ん中" などを判定したければここでロジック追加。

                    context.DrawLine(penSub, new Point(x, height), new Point(x, height - lineLength));
                }
            }
        }

        private static TimeFormat GetTimeFormat(double interval)
        {
            if (interval < 1.0) return TimeFormat.SubSeconds;
            if (interval < 60.0) return TimeFormat.Seconds;
            if (interval < 3600.0) return TimeFormat.Minutes;
            return TimeFormat.Hours;
        }

        /// <summary>
        /// 時間フォーマットの種類
        /// </summary>
        private enum TimeFormat
        {
            SubSeconds,  // 小数秒まで (0:00.00)
            Seconds,     // 秒まで (0:00)
            Minutes,     // 分まで (0m)
            Hours        // 時まで (0h)
        }

        /// <summary>
        /// 指定されたフォーマットで時間を文字列化します。
        /// </summary>
        private static string FormatTime(double seconds, TimeFormat format)
        {
            var ts = TimeSpan.FromSeconds(seconds);

            return format switch
            {
                TimeFormat.SubSeconds => FormatSubSeconds(seconds),
                TimeFormat.Seconds => ts.TotalHours >= 1
                    ? ts.ToString(@"h\:mm\:ss")
                    : ts.ToString(@"m\:ss"),
                TimeFormat.Minutes => ts.TotalHours >= 1
                    ? ts.ToString(@"h\:mm")
                    : $"{(int)ts.TotalMinutes}m",
                TimeFormat.Hours => $"{(int)ts.TotalHours}h",
                _ => ts.ToString(@"hh\:mm\:ss")
            };
        }

        /// <summary>
        /// 小数秒フォーマットで時間を文字列化します。
        /// </summary>
        private static string FormatSubSeconds(double totalSeconds)
        {
            int minutes = (int)(totalSeconds / 60);
            double secs = totalSeconds % 60;

            if (minutes >= 60)
            {
                int hours = minutes / 60;
                minutes = minutes % 60;
                return $"{hours}:{minutes:D2}:{secs:00.00}";
            }
            else if (minutes > 0)
            {
                return $"{minutes}:{secs:00.00}";
            }
            else
            {
                return $"{secs:0.00}";
            }
        }
    }
}
