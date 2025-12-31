using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace Metasia.Editor.Controls
{
    public class PlayerSlider : Slider
    {
        protected override Type StyleKeyOverride => typeof(PlayerSlider);

        public static readonly StyledProperty<double?> SelectStartValueProperty =
            AvaloniaProperty.Register<PlayerSlider, double?>(nameof(SelectStartValue));

        public double? SelectStartValue
        {
            get => GetValue(SelectStartValueProperty);
            set => SetValue(SelectStartValueProperty, value);
        }

        public static readonly StyledProperty<double?> SelectEndValueProperty =
            AvaloniaProperty.Register<PlayerSlider, double?>(nameof(SelectEndValue));

        public double? SelectEndValue
        {
            get => GetValue(SelectEndValueProperty);
            set => SetValue(SelectEndValueProperty, value);
        }

        public static readonly StyledProperty<IBrush> SelectionRegionBrushProperty =
            AvaloniaProperty.Register<PlayerSlider, IBrush>(nameof(SelectionRegionBrush), Brushes.Gray);

        public IBrush SelectionRegionBrush
        {
            get => GetValue(SelectionRegionBrushProperty);
            set => SetValue(SelectionRegionBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> OutsideRegionBrushProperty =
            AvaloniaProperty.Register<PlayerSlider, IBrush>(nameof(OutsideRegionBrush), Brushes.DimGray);

        public IBrush OutsideRegionBrush
        {
            get => GetValue(OutsideRegionBrushProperty);
            set => SetValue(OutsideRegionBrushProperty, value);
        }

        public static readonly StyledProperty<double> TrackThicknessProperty =
            AvaloniaProperty.Register<PlayerSlider, double>(nameof(TrackThickness), 4.0);

        public double TrackThickness
        {
            get => GetValue(TrackThicknessProperty);
            set => SetValue(TrackThicknessProperty, value);
        }
    }
}
