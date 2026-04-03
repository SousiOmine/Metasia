using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Metasia.Editor.Views.BindingConverter;

public class LayerStateBackgroundConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 &&
            values[0] is bool isSelected &&
            values[1] is bool isActive)
        {
            if (!isActive)
            {
                return new SolidColorBrush(Avalonia.Media.Color.FromArgb(48, 128, 128, 128));
            }
            if (isSelected)
            {
                return new SolidColorBrush(Avalonia.Media.Color.FromArgb(48, 100, 149, 237));
            }
        }
        return Brushes.Transparent;
    }
}