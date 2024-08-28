using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Kraken.Desktop.Utils;

namespace Kraken.Desktop.Services;

public class StorageService
{
    private StorageService()
    {
        _fileService = FileService.Instance;

        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData
        );
        ApplicationDataFolder = Path.Combine(localApplicationData, DefaultApplicationDataFolder);
        _settings = new Dictionary<string, object>();
        Initialize();
    }

    private bool _isInitialized;
    private static readonly Lazy<StorageService> Lazy = new(() => new StorageService());
    private readonly FileService _fileService;
    private IDictionary<string, object> _settings;

    public const string DefaultApplicationDataFolder = "Kraken/ApplicationData";
    public const string DefaultColorModesSettingsFile = "KrakenColorModesSettings.json";
    public const string DefaultSettingsFile = "KrakenSettings.json";
    public const string SelectedRingModeKey = "selectedRingMode";
    public const string SelectedLogoModeKey = "selectedLogoMode";
    public const string FixedColorsKey = "fixedColors";
    public const string RecentColorsKey = "recentColors";

    public readonly string ApplicationDataFolder;
    public static StorageService Instance => Lazy.Value;

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        if (!_settings.TryGetValue(key, out var value))
            return await Task.FromResult<T?>(default);

        var valueString = Convert.ToString(value);

        if (valueString is null)
            return await Task.FromResult<T?>(default);

        var tObject = await Task.Run(() => JsonSerializer.Deserialize<T>(valueString));

        return await Task.FromResult(tObject);
    }

    public async Task SaveSettingAsync<T>(string key, T content)
    {
        Debug.Assert(content != null, nameof(content) + " != null");

        _settings[key] = JsonSerializer.Serialize(content);

        await Task.Run(
            () => _fileService.Save(ApplicationDataFolder, DefaultSettingsFile, _settings)
        );
    }

    private void Initialize()
    {
        if (_isInitialized)
            return;

        _settings =
            _fileService.Read<IDictionary<string, object>>(
                ApplicationDataFolder,
                DefaultSettingsFile
            ) ?? new Dictionary<string, object>();

        _isInitialized = true;

        EnsureDefaultSettings();
    }

    private void EnsureDefaultSettings()
    {
        var colorModesSettingsPath = Path.Combine(
            ApplicationDataFolder,
            DefaultColorModesSettingsFile
        );

        if (!File.Exists(colorModesSettingsPath))
        {
            _fileService.Save(
                ApplicationDataFolder,
                DefaultColorModesSettingsFile,
                KrakenDefaultSettings.ColorDefaultSettings
            );
        }

        if (
            !_settings.TryGetValue(SelectedLogoModeKey, out _)
            || !_settings.TryGetValue(SelectedRingModeKey, out _)
        )
        {
            const string defaultValue = "fixed";

            _settings[SelectedLogoModeKey] = JsonSerializer.Serialize(defaultValue);
            _settings[SelectedRingModeKey] = JsonSerializer.Serialize(defaultValue);
        }

        if (!_settings.TryGetValue(FixedColorsKey, out _))
        {
            var fixedColorsArray = new int[][]
            {
                [255, 196, 0, 255],
                [255, 50, 0, 255],
                [255, 0, 255, 80],
                [255, 0, 255, 180],
                [255, 255, 0, 80],
                [255, 200, 255, 0],
                [255, 255, 100, 255],
                [255, 255, 0, 0],
                [255, 255, 80, 0],
                [255, 255, 150, 0],
                [255, 40, 255, 0],
                [255, 0, 0, 255],
                [255, 255, 255, 255],
                [255, 0, 0, 0]
            };

            _settings[FixedColorsKey] = JsonSerializer.Serialize(fixedColorsArray);
        }

        if (!_settings.TryGetValue(RecentColorsKey, out _))
        {
            var recentColorsArray = new int[][]
            {
                [255, 255, 0, 0],
                [255, 0, 0, 255],
                [255, 0, 255, 255],
                [255, 255, 0, 255],
                [255, 255, 255, 0],
            };
            _settings[RecentColorsKey] = JsonSerializer.Serialize(recentColorsArray);
        }

        _fileService.Save(ApplicationDataFolder, DefaultSettingsFile, _settings);
    }
}
