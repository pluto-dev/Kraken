using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Kraken.Desktop.Models;
using Kraken.Desktop.Services;
using Kraken.Desktop.Utils;
using Kraken.Desktop.Views;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace Kraken.Desktop.Controls;

public sealed partial class LedControl : UserControl, INotifyPropertyChanged
{
    public LedControl(
        KrakenDevice krakenDevice,
        IList<Tuple<string, IReadOnlyList<int?>>> deviceColorModes,
        IDictionary<string, IDictionary<string, ColorModeSettings>> colorModesSettings,
        string channelName
    )
    {
        _colorModesSettings = colorModesSettings;
        KrakenDevice = krakenDevice;
        DeviceColorModes = deviceColorModes;
        ChannelName = channelName;
        InitializeComponent();
    }

    private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;
    private readonly KrakenService _krakenService = KrakenService.Instance;
    private readonly StorageService _storageService = StorageService.Instance;
    private readonly IDictionary<
        string,
        IDictionary<string, ColorModeSettings>
    > _colorModesSettings;
    private ColorModeSettings? _selectedModeOptions;

    public string[] Direction { get; set; } = ["Forward", "Backward"];
    public List<SolidColorBrush> FixColors { get; } = [];
    public ObservableCollection<SolidColorBrush> RecentColors { get; } = [];
    public bool HasAnimationSpeed => SelectedModeOptions?.AnimationSpeed is not null;
    public bool HasDirection => SelectedModeOptions?.Direction is not null;
    public bool IsSingleColor => SelectedModeOptions?.MaxColors == 1;
    public bool IsMultiColors => SelectedModeOptions?.MaxColors > 1;
    public bool IsAutomaticColors => SelectedModeOptions?.MaxColors == 0;

    public bool CanAddColor =>
        SelectedModeOptions?.MaxColors == 1
        || SelectedColors.Count != SelectedModeOptions?.MaxColors;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IList<Tuple<string, IReadOnlyList<int?>>> DeviceColorModes { get; set; }

    public string ChannelName { get; set; }

    public SolidColorBrush? SelectedColor => SelectedColors.FirstOrDefault()?.Color;

    /// <summary>
    /// Update the Colors property of <see cref="SelectedModeOptions" />. The collection will automatically update.
    /// </summary>
    public ObservableCollection<LedColor> SelectedColors { get; } = [];

    public SolidColorBrush LogoColor => GetBrushForSelectedColor("Logo");

    public SolidColorBrush RingColor => GetBrushForSelectedColor("Ring");

    private SolidColorBrush GetBrushForSelectedColor(string? channel)
    {
        Debug.Assert(!string.IsNullOrEmpty(channel), "string.IsNullOrEmpty(channel)");
        Debug.Assert(channel is "Logo" or "Ring", "channel is not 'Logo' or 'Ring'");

        if (SelectedColor is null || ChannelName != channel)
            return new SolidColorBrush(Colors.WhiteSmoke);

        return SelectedColor;
    }

    public ColorModeSettings? SelectedModeOptions
    {
        get => _selectedModeOptions;
        set
        {
            UnregisterCollectionEvents();
            _selectedModeOptions = value;
            RegisterCollectionEvents();

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasAnimationSpeed));
            OnPropertyChanged(nameof(HasDirection));
            OnPropertyChanged(nameof(IsSingleColor));
            OnPropertyChanged(nameof(IsMultiColors));
            OnPropertyChanged(nameof(IsAutomaticColors));

            SelectedColors.Clear();
            var colors = _selectedModeOptions?.Colors;
            if (colors is not null)
            {
                foreach (var color in colors)
                {
                    if (color is null)
                        continue;

                    SelectedColors.Add(
                        new LedColor(
                            new SolidColorBrush(Color.FromArgb(255, color[0], color[1], color[2]))
                        )
                    );
                }
            }

            UpdateSelectedColor();
            OnPropertyChanged(nameof(SelectedColor));
            OnPropertyChanged(nameof(CanAddColor));
            OnPropertyChanged(nameof(SelectedDirectionItem));
            OnPropertyChanged(nameof(AnimationSpeedSliderValue));
        }
    }

    public KrakenDevice KrakenDevice { get; set; }

    public string? SelectedDirectionItem
    {
        get
        {
            var direction = SelectedModeOptions?.Direction;
            return direction is null ? null : _textInfo.ToTitleCase(direction);
        }
    }

    public string? AnimationSpeedSliderValue
    {
        get
        {
            var animationSpeed = SelectedModeOptions?.AnimationSpeed;
            return animationSpeed is not null ? _textInfo.ToTitleCase(animationSpeed) : null;
        }
    }

    private void UpdateSelectedColor()
    {
        if (SelectedColor is null)
            return;

        switch (ChannelName)
        {
            case "Ring":
                SetPathColor(SelectedColor, Ring);
                break;
            case "Logo":
                SetPathColor(SelectedColor, Logo);
                break;
        }
    }

    private void Colors_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                SelectedColors.Clear();
                break;
            case NotifyCollectionChangedAction.Add:
            {
                var rgbColor = e.NewItems?[0] as List<byte>;
                Debug.Assert(rgbColor is not null, "Wrong type conversion?");

                var brush = new SolidColorBrush(
                    Color.FromArgb(255, rgbColor[0], rgbColor[1], rgbColor[2])
                );
                SelectedColors.Add(new LedColor(brush));
                OnPropertyChanged(nameof(SelectedColor));
                OnPropertyChanged(nameof(LogoColor));
                OnPropertyChanged(nameof(RingColor));
                break;
            }
            case NotifyCollectionChangedAction.Remove:
                SelectedColors.RemoveAt(e.OldStartingIndex);
                OnPropertyChanged(nameof(SelectedColor));
                OnPropertyChanged(nameof(LogoColor));
                OnPropertyChanged(nameof(RingColor));
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        OnPropertyChanged(nameof(CanAddColor));
    }

    private void RegisterCollectionEvents()
    {
        if (_selectedModeOptions?.Colors is not null)
        {
            _selectedModeOptions.Colors.CollectionChanged += Colors_CollectionChanged;
        }
    }

    private void UnregisterCollectionEvents()
    {
        if (_selectedModeOptions?.Colors is not null)
        {
            _selectedModeOptions.Colors.CollectionChanged -= Colors_CollectionChanged;
        }
    }

    public static string ToHexColor(SolidColorBrush? colorBrush) =>
        $"#{colorBrush?.Color.R:X}{colorBrush?.Color.G:X}{colorBrush?.Color.B:X}";

    private static void SetPathColor(SolidColorBrush e, Panel panel)
    {
        foreach (var child in panel.Children)
        {
            if (child is not Path path)
                continue;
            path.Fill = e;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private async void ColorModesComboBox_OnSelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (e.AddedItems.FirstOrDefault() is not Tuple<string, IReadOnlyList<int?>> colorMode)
            return;

        if (_colorModesSettings.Count > 0)
        {
            var selectedMode = _colorModesSettings[ChannelName.ToLower()][colorMode.Item1];
            SelectedModeOptions = selectedMode;
        }

        await UpdateDeviceColorMode(SelectedModeOptions);
    }

    private void DeleteFlyout_OnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not MenuFlyoutItem { DataContext: LedColor ledColor })
        {
            throw new ArgumentException("Wrong type of OriginalSource", nameof(e));
        }

        var index = SelectedColors.IndexOf(ledColor);
        SelectedModeOptions?.Colors?.RemoveAt(index);
    }

    private async void AnimationSpeedSlider_OnValueChanged(
        object sender,
        RangeBaseValueChangedEventArgs e
    )
    {
        var animationSpeed = e.NewValue switch
        {
            0.0 => "Slowest",
            1.0 => "Slower",
            2.0 => "Normal",
            3.0 => "Faster",
            4.0 => "Fastest",
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };

        if (SelectedModeOptions is not null)
        {
            SelectedModeOptions.AnimationSpeed = animationSpeed;
            await UpdateDeviceColorMode(SelectedModeOptions);
        }
    }

    private async void DirectionCombobox_OnSelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (SelectedModeOptions is null)
            return;

        var direction = e.AddedItems.FirstOrDefault()?.ToString()?.ToLower();
        if (SelectedModeOptions.Direction != direction)
        {
            SelectedModeOptions.Direction = direction;
        }

        await UpdateDeviceColorMode(SelectedModeOptions);
    }

    private async void CustomColorPicker_OnClick(object? sender, SolidColorBrush e)
    {
        var maxColors = SelectedModeOptions?.MaxColors;
        if (maxColors == 1)
        {
            SelectedModeOptions?.Colors?.Clear();
        }

        var color = e.Color;
        SelectedModeOptions?.Colors?.Add([color.R, color.G, color.B]);

        await UpdateDeviceColorMode(SelectedModeOptions);
    }

    private async Task UpdateDeviceColorMode(ColorModeSettings? options)
    {
        if (options is null)
            return;

        // Delay for 1 sec to avoid spamming the device
        await Task.Delay(TimeSpan.FromMilliseconds(1000));

        await _krakenService.SetColor(
            KrakenDevice.Id,
            ChannelName.ToLower(),
            options.Mode.ToLower(),
            options.Colors?.ToList(),
            options.AnimationSpeed?.ToLower(),
            options.Direction?.ToLower()
        );
    }

    private async void LedControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var fixedColor in await _storageService.ReadFixedColors())
        {
            FixColors.Add(
                new SolidColorBrush(
                    Color.FromArgb(fixedColor[0], fixedColor[1], fixedColor[2], fixedColor[3])
                )
            );
        }

        foreach (var recentColor in await _storageService.ReadRecentColors())
        {
            RecentColors.Add(
                new SolidColorBrush(
                    Color.FromArgb(recentColor[0], recentColor[1], recentColor[2], recentColor[3])
                )
            );
        }

        var key =
            ChannelName is { } s && s.ToLower() == "ring"
                ? StorageService.SelectedRingModeKey
                : StorageService.SelectedLogoModeKey;

        var selectedMode = await _storageService.ReadSettingAsync<string>(key);

        if (selectedMode is null)
            throw new InvalidOperationException("Couldn't retrieve selected channel mode");
        var options = _colorModesSettings[ChannelName.ToLower()][selectedMode];

        SelectedModeOptions = options;
        ColorModesComboBox.SelectedItem = DeviceColorModes.First(x =>
            x.Item1.Contains(SelectedModeOptions.Mode)
        );
    }

    private void LedControl_OnUnloaded(object sender, RoutedEventArgs e)
    {
        UnregisterCollectionEvents();
    }
}
