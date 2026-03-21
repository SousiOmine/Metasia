using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Metasia.Editor.Views.BindingConverter;

public class BoolToSelectionBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected && isSelected)
        {
            return new SolidColorBrush(Avalonia.Media.Color.FromArgb(32, 128, 128, 128));
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}