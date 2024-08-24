using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kraken.Desktop.Models;

public class ColorModeSettings(
    string mode,
    string? animationSpeed = null,
    string? direction = null,
    ObservableCollection<List<byte>?>? colors = null,
    int? maxColors = null,
    int? minColors = null
)
{
    public string Mode { get; set; } = mode;
    public string? AnimationSpeed { get; set; } = animationSpeed;
    public string? Direction { get; set; } = direction;
    public ObservableCollection<List<byte>?>? Colors { get; set; } = colors;
    public int? MaxColors { get; set; } = maxColors;
    public int? MinColors { get; set; } = minColors;
}
