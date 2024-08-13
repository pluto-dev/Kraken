using System;
using Microsoft.UI.Xaml.Data;

namespace Kraken.Desktop.Converters;

public class SliderAnimationSpeedsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value switch
        {
            0.0 => "Slowest",
            1.0 => "Slower",
            2.0 => "Normal",
            3.0 => "Faster",
            4.0 => "Fastest",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
