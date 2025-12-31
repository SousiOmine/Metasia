using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Metasia.Editor.Controls
{
    public class RangeSelectionLayer : Control
    {
        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<RangeSelectionLayer, double>(nameof(Minimum));

        public double Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<RangeSelectionLayer, double>(nameof(Maximum));

        public double Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly StyledProperty<double?> SelectStartValueProperty =
            AvaloniaProperty.Register<RangeSelectionLayer, double?>(nameof(SelectStartValue));

        public double? SelectStartValue
        {
            get => GetValue(SelectStartValueProperty);
            set => SetValue(SelectStartValueProperty, value);
        }

        public static readonly StyledProperty<double?> SelectEndValueProperty =
            AvaloniaProperty.Register<RangeSelectionLayer, double?>(nameof(SelectEndValue));

        public double? SelectEndValue
        {
            get => GetValue(SelectEndValueProperty);
            set => SetValue(SelectEndValueProperty, value);
        }

        public static readonly StyledProperty<IBrush> SelectionRegionBrushProperty =
            AvaloniaProperty.Register<RangeSelectionLayer, IBrush>(nameof(SelectionRegionBrush));

        public IBrush SelectionRegionBrush
        {
            get => GetValue(SelectionRegionBrushProperty);
            set => SetValue(SelectionRegionBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> OutsideRegionBrushProperty =
            AvaloniaProperty.Register<RangeSelectionLayer, IBrush>(nameof(OutsideRegionBrush));

        public IBrush OutsideRegionBrush
        {
            get => GetValue(OutsideRegionBrushProperty);
            set => SetValue(OutsideRegionBrushProperty, value);
        }

        public static readonly StyledProperty<Orientation> OrientationProperty =
           AvaloniaProperty.Register<RangeSelectionLayer, Orientation>(nameof(Orientation));

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        static RangeSelectionLayer()
        {
            AffectsRender<RangeSelectionLayer>(
                MinimumProperty, MaximumProperty,
                SelectStartValueProperty, SelectEndValueProperty,
                SelectionRegionBrushProperty, OutsideRegionBrushProperty,
                OrientationProperty);
        }

        public override void Render(DrawingContext context)
        {
            var bounds = Bounds;
            var min = Minimum;
            var max = Maximum;
            var range = max - min;

            if (range <= 0) return;

            // Resolve actual start/end
            // "SelectStartValueが指定されてない場合は内部的にMinimumと等しく"
            var start = SelectStartValue ?? min;
            // "SelectEndValueが指定されてない場合にはMaximumと同じ値"
            var end = SelectEndValue ?? max;

            // Clamp and sort? Usually users ensure Start <= End.
            // But good to clamp to min/max.
            // Assuming start <= end. If not, swap or handle?
            // "SelectStartValue... SelectEndValue". Usually strict.
            // If unset, they are Min/Max.
            // Let's also ensure they are within [min, max].

            if (start < min) start = min;
            if (start > max) start = max;
            if (end < min) end = min;
            if (end > max) end = max;

            // If start > end, we might just draw nothing or treat as effective swap?
            // Standard behavior in selection: usually implies empty or swap.
            // Let's assume start <= end. If start > end, effective range is empty?
            if (start > end)
            {
                double temp = start;
                start = end;
                end = temp;
            }

            // Normalization
            // Pos 0.0 corresponds to Min
            // Pos 1.0 corresponds to Max

            double pixelLength = (Orientation == Orientation.Horizontal) ? bounds.Width : bounds.Height;

            double startRatio = (start - min) / range;
            double endRatio = (end - min) / range;

            double startPx = startRatio * pixelLength;
            double endPx = endRatio * pixelLength;

            // Draw 3 rectangles
            // Part 1: [0, startPx] -> OutsideRegionBrush
            // Part 2: [startPx, endPx] -> SelectionRegionBrush
            // Part 3: [endPx, pixelLength] -> OutsideRegionBrush

            // Note: need to handle Orientation
            bool isHorizontal = Orientation == Orientation.Horizontal;

            // Rect 1
            if (startPx > 0)
            {
                var rect = isHorizontal
                    ? new Rect(0, 0, startPx, bounds.Height)
                    : new Rect(0, 0, bounds.Width, startPx); // Vertical usually goes Bottom->Top?
                                                             // Standard Slider Vertical: Bottom is Min? Or Top?
                                                             // Avalonia Slider Vertical: Min is Bottom.
                                                             // If Min is Bottom, 0 coordinate is Top. 
                                                             // So we need to invert if Vertical.

                if (!isHorizontal)
                {
                    // Vertical: 
                    // Y=0 is Top (Max? depends on IsDirectionReversed which defaults to false)
                    // Wait, Avalonia Slider defaults: 
                    // Horizontal: Left=Min, Right=Max.
                    // Vertical: Bottom=Min, Top=Max.
                    // So for Vertical, Min is at bounds.Height, Max is at 0.
                    // ratio 0 -> Y = Height
                    // ratio 1 -> Y = 0

                    // Coordinates:
                    // Y_min = Height
                    // Y_max = 0
                    // Y_start = Height - (startRatio * Height)
                    // Y_end = Height - (endRatio * Height)
                    // Y_end is smaller (higher up) than Y_start.

                    double yMin = bounds.Height;
                    // double yMax = 0;
                    double yStart = bounds.Height - startPx;
                    double yEnd = bounds.Height - endPx;

                    // Segments:
                    // [Min...Start]: Y from yMin down to yStart (visually bottom up)
                    // Rect 1 (Outside): Y from yStart to yMin
                    context.DrawRectangle(OutsideRegionBrush, null, new Rect(0, yStart, bounds.Width, yMin - yStart));

                    // Rect 2 (Selected): Y from yEnd to yStart
                    context.DrawRectangle(SelectionRegionBrush, null, new Rect(0, yEnd, bounds.Width, yStart - yEnd));

                    // Rect 3 (Outside): Y from yMax (0) to yEnd
                    context.DrawRectangle(OutsideRegionBrush, null, new Rect(0, 0, bounds.Width, yEnd));
                }
                else
                {
                    // Horizontal:
                    // Rect 1
                    context.DrawRectangle(OutsideRegionBrush, null, new Rect(0, 0, startPx, bounds.Height));
                    // Rect 2
                    context.DrawRectangle(SelectionRegionBrush, null, new Rect(startPx, 0, endPx - startPx, bounds.Height));
                    // Rect 3
                    context.DrawRectangle(OutsideRegionBrush, null, new Rect(endPx, 0, pixelLength - endPx, bounds.Height));
                }
            }
            else
            {
                // Start is effectively 0 (Min)
                if (!isHorizontal)
                {
                    // Vertical
                    double yEnd = bounds.Height - endPx;
                    // Rect 2 (Selected): Y from yEnd to Height
                    context.DrawRectangle(SelectionRegionBrush, null, new Rect(0, yEnd, bounds.Width, bounds.Height - yEnd));
                    // Rect 3 (Outside): Y from 0 to yEnd
                    context.DrawRectangle(OutsideRegionBrush, null, new Rect(0, 0, bounds.Width, yEnd));
                }
                else
                {
                    // Rect 2
                    context.DrawRectangle(SelectionRegionBrush, null, new Rect(0, 0, endPx, bounds.Height));
                    // Rect 3
                    context.DrawRectangle(OutsideRegionBrush, null, new Rect(endPx, 0, pixelLength - endPx, bounds.Height));
                }
            }
        }
    }
}
