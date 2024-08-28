using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Kraken.Desktop.Models;
using Kraken.Desktop.Services;

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
                    StorageService.DefaultColorModesSettingsFile,
                    _colorModesSettings
                )
        );
    }

    public static async Task<int[][]> ReadRecentColors(this StorageService storage)
    {
        var recentColors = await storage.ReadSettingAsync<int[][]>(StorageService.RecentColorsKey);

        if (recentColors is null)
        {
            throw new InvalidOperationException("Couldn't retrieve setting");
        }

        return recentColors;
    }

    public static async Task<int[][]> ReadFixedColors(this StorageService storage)
    {
        var fixedColors = await storage.ReadSettingAsync<int[][]>(StorageService.FixedColorsKey);

        if (fixedColors is null)
        {
            throw new InvalidOperationException("Couldn't retrieve setting");
        }

        return fixedColors;
    }

    public static async Task SaveRecentColors(this StorageService storage, int[][] recentColors)
    {
        await storage.SaveSettingAsync(StorageService.RecentColorsKey, recentColors);
    }

    public static async Task SavePumpValues(
        this StorageService storage,
        int? krakenDeviceId,
        string pump,
        (int, int)[] values
    )
    {
        throw new NotImplementedException();
        //var aas = values.Select(x => new).ToArray()

        //var s = JsonSerializer.Serialize(
        //    values.ToDictionary(),
        //    new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
        //);
        //var sa = JsonSerializer.Serialize(s);
        //var d = JsonSerializer.Deserialize<Dictionary<string, int>>(s);
        //await storage.SaveSettingAsync("pumpSpeed", values.ToDictionary());
    }
}
