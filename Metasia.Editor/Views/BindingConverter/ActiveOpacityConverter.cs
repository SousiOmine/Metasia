using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Metasia.Editor.Views.BindingConverter;

public class ActiveOpacityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? 1.0 : 0.5;
        }

        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue >= 0.75;
        }

        return true;
    }
}
