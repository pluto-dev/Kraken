using System;
using Microsoft.UI.Xaml.Media;

namespace Kraken.Desktop.Models;

public class LedColor(SolidColorBrush color)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public SolidColorBrush Color { get; set; } = color;
}
