using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kraken.Desktop.Services;
using Kraken.Desktop.Views;

namespace Kraken.Desktop.Utils;

public static class StorageServiceHelper
{
    private static readonly FileService FileService = FileService.Instance;
    private static IDictionary<
        string,
        IDictionary<string, ColorModeSettings>
    >? _colorModesSettings = new Dictionary<string, IDictionary<string, ColorModeSettings>>();

    public static async Task<IDictionary<
        string,
        IDictionary<string, ColorModeSettings>
    >?> ReadColorModeSettingsAsync(this StorageService storage)
    {
        if (_colorModesSettings?.Count > 0)
            return _colorModesSettings;

        return _colorModesSettings = await Task.Run(
            () =>
                FileService.Read<IDictionary<string, IDictionary<string, ColorModeSettings>>>(
                    storage.ApplicationDataFolder,
                    StorageService.DefaultColorModesSettingsFile
                )
        );
    }

    public static async Task SaveColorModeSettingsAsync(
        this StorageService storage,
        ColorModeSettings setting,
        string channel
    )
    {
        if (_colorModesSettings is null)
        {
            throw new InvalidOperationException($"{nameof(_colorModesSettings)} is null");
        }

        var colorMode = _colorModesSettings[channel][setting.Mode];

        colorMode.Mode = setting.Mode;
        colorMode.AnimationSpeed = setting.AnimationSpeed;
        colorMode.Direction = setting.Direction;
        colorMode.Colors = setting.Colors;
        colorMode.MaxColors = setting.MaxColors;
        colorMode.MinColors = setting.MinColors;

        await Task.Run(
            () =>
                FileService.Save(
                    storage.ApplicationDataFolder,
                    StorageService.DefaultSettingsFile,
                    _colorModesSettings
                )
        );
    }

    public static async Task<byte[][]> ReadRecentColors(this StorageService storage)
    {
        var recentColorsBase64 = await storage.ReadSettingAsync<string>(
            StorageService.RecentColorsKey
        );

        if (recentColorsBase64 is null)
        {
            throw new InvalidOperationException("Couldn't retrieve setting");
        }

        return Util.UnBaseifyColorsArray(recentColorsBase64);
    }

    public static async Task<byte[][]> ReadFixedColors(this StorageService storage)
    {
        var fixedColorsBase64 = await storage.ReadSettingAsync<string>(
            StorageService.FixedColorsKey
        );

        if (fixedColorsBase64 is null)
        {
            throw new InvalidOperationException("Couldn't retrieve setting");
        }

        return Util.UnBaseifyColorsArray(fixedColorsBase64);
    }

    public static async Task SaveRecentColors(this StorageService storage, byte[][] recentColors)
    {
        var baseifiedArray = Util.BaseifyColorsArray(recentColors);

        await storage.SaveSettingAsync(StorageService.RecentColorsKey, baseifiedArray);
    }
}
