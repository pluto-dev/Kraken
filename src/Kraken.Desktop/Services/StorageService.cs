using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Kraken.Desktop.Utils;

namespace Kraken.Desktop.Services;

public class StorageService
{
    private StorageService()
    {
        _jsonDeserializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _fileService = FileService.Instance;

        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData
        );
        ApplicationDataFolder = Path.Combine(localApplicationData, DefaultApplicationDataFolder);
        _settings = new Dictionary<string, object>();
        InitializeAsync();
    }

    private bool _isInitialized;
    private static readonly Lazy<StorageService> Lazy = new(() => new StorageService());
    private readonly JsonSerializerOptions _jsonDeserializerOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
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
        if (_settings.TryGetValue(key, out var obj))
        {
            var str = Convert.ToString(obj);

            return await Task.Run(
                () => JsonSerializer.Deserialize<T>(str!, _jsonDeserializerOptions)
            );
        }

        return default;
    }

    public async Task SaveSettingAsync<T>(string key, T content)
    {
        _settings[key] = await Task.Run(
            () => JsonSerializer.Serialize(content, _jsonSerializerOptions)
        );

        await Task.Run(
            () => _fileService.Save(ApplicationDataFolder, DefaultSettingsFile, _settings)
        );
    }

    private void InitializeAsync()
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

            _settings[SelectedLogoModeKey] = JsonSerializer.Serialize(
                defaultValue,
                _jsonSerializerOptions
            );
            _settings[SelectedRingModeKey] = JsonSerializer.Serialize(
                defaultValue,
                _jsonSerializerOptions
            );
        }

        if (!_settings.TryGetValue(FixedColorsKey, out _))
        {
            var fixedColorsArray = new byte[][]
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

            var base64 = Util.BaseifyColorsArray(fixedColorsArray);
            _settings[FixedColorsKey] = JsonSerializer.Serialize(base64, _jsonSerializerOptions);
        }

        if (!_settings.TryGetValue(RecentColorsKey, out _))
        {
            var recentColorsArray = new byte[][]
            {
                [255, 255, 0, 0],
                [255, 0, 0, 255],
                [255, 0, 255, 255],
                [255, 255, 0, 255],
                [255, 255, 255, 0],
            };

            var base64 = Util.BaseifyColorsArray(recentColorsArray);
            _settings[RecentColorsKey] = JsonSerializer.Serialize(base64, _jsonSerializerOptions);
        }

        _fileService.Save(ApplicationDataFolder, DefaultSettingsFile, _settings);
    }
}
