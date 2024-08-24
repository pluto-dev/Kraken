using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Kraken.Desktop.Controls;
using Kraken.Desktop.Services;
using Kraken.Desktop.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kraken.Desktop.Views;

public sealed partial class LightingPage : Page, INotifyPropertyChanged
{
    public LightingPage()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly KrakenService _krakenService = KrakenService.Instance;
    private readonly StorageService _storageService = StorageService.Instance;

    private KrakenDevice? _krakenDevice;
    private string _krakenDeviceName = string.Empty;

    public string KrakenDeviceName
    {
        get => _krakenDeviceName;
        set
        {
            _krakenDeviceName = value;
            OnPropertyChanged();
        }
    }

    private async void LightingPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        var devices = await _krakenService.GetDevices();

        _krakenDevice = devices?.FirstOrDefault(x => x is { ProductId: 8199, VendorId: 7793 });

        if (_krakenDevice is null)
            return;

        KrakenDeviceName = _krakenDevice.Description;

        IList<Tuple<string, IReadOnlyList<int?>>> deviceColorModes = [];

        foreach (var (key, value) in _krakenDevice.ColorModes)
        {
            deviceColorModes.Add(new Tuple<string, IReadOnlyList<int?>>(key, value));
        }

        var colorModesSettings = await _storageService.ReadColorModeSettingsAsync();
        if (colorModesSettings is null)
        {
            throw new InvalidOperationException("Couldn't retrieve color modes settings");
        }
        var textInfo = CultureInfo.CurrentCulture.TextInfo;

        foreach (var key in _krakenDevice.ColorChannels.Keys.Where(key => key is "ring" or "logo"))
        {
            LedStackPanel.Children.Add(
                new LedControl(
                    _krakenDevice,
                    deviceColorModes,
                    colorModesSettings,
                    textInfo.ToTitleCase(key)
                )
            );
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
