using System;
using Microsoft.UI.Xaml.Data;

// ReSharper disable HeapView.BoxingAllocation

namespace Kraken.Desktop.Converters;

public class AnimationSpeedsToValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value switch
        {
            "Slowest" => 0.0,
            "Slower" => 1.0,
            "Normal" => 2.0,
            "Faster" => 3.0,
            "Fastest" => 4.0,
            _ => 2.0
        };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ValueToAnimationSpeedsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value switch
        {
            0.0 => "Slowest",
            1.0 => "Slower",
            2.0 => "Normal",
            3.0 => "Faster",
            4.0 => "Fastest",
            _ => "Normal"
        };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
