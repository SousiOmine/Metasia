using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Metasia.Editor.Views.BindingConverter;

public class ClipSelectConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? 0.5 : 1.0;
        }

        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}