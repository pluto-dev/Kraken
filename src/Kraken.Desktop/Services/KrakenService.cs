using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Kraken.Desktop.Converters;
using Kraken.Desktop.Models;

namespace Kraken.Desktop.Services;

public class KrakenService
{
    private KrakenService() { }

    private static readonly HttpClient Http =
        new() { BaseAddress = new Uri("http://127.0.0.1:5000") };

    private static readonly Lazy<KrakenService> Lazy = new(() => new KrakenService());

    private readonly JsonSerializerOptions _deSerializerOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly JsonSerializerOptions _serializerColorModeOptions =
        new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    private readonly JsonSerializerOptions _serializerSpeedProfileOptions =
        new() { Converters = { new SpeedProfileConverter() } };

    private List<KrakenDevice>? _devices = [];
    public static KrakenService Instance => Lazy.Value;

    private KrakenDevice? _cachedDevice;

    public async Task<KrakenDevice> GetDeviceAsync(int productId, int vendorId)
    {
        if (_cachedDevice is not null)
            return _cachedDevice;

        if (_devices?.Count == 0)
        {
            await GetDevicesAsync();
        }

        _cachedDevice = _devices?.FirstOrDefault(x =>
            x.ProductId == productId && x.VendorId == vendorId
        );

        if (_cachedDevice is null)
        {
            throw new InvalidOperationException("Failed to retrieve the device.");
        }
        return _cachedDevice;
    }

    public async Task<List<KrakenDevice>?> GetDevicesAsync()
    {
        if (_devices?.Count > 0)
            return _devices;
        using var response = await Http.GetAsync("devices");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        _devices = await Task.Run(() => JsonSerializer.Deserialize<List<KrakenDevice>>(content));

        if (_devices?.Count > 0)
        {
            foreach (var device in _devices)
            {
                await InitializeDeviceAsync(device.Id);
            }
        }
        return _devices;
    }

    private async Task InitializeDeviceAsync(int? id)
    {
        using var response = await Http.GetAsync($"devices/{id}/initialize");

        response.EnsureSuccessStatusCode();
    }

    public async Task SetSpeedProfile(int? id, string channel, IEnumerable<(int X, int Y)> profile)
    {
        using var json = new StringContent(
            JsonSerializer.Serialize(new { channel, profile }, _serializerSpeedProfileOptions),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await Http.PostAsync($"devices/{id}/speed", json);
        response.EnsureSuccessStatusCode();
    }

    public async Task<KrakenStatus?> GetKrakenStatus(int? id)
    {
        using var response = await Http.GetAsync($"devices/{id}/status");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<KrakenStatus>(content, _deSerializerOptions);
    }

    public async Task SetColorMode(
        int? id,
        string channel,
        string mode,
        List<List<byte>?>? colors,
        string? speed,
        string? direction
    )
    {
        using var json = new StringContent(
            JsonSerializer.Serialize(
                new
                {
                    channel,
                    mode,
                    colors,
                    speed,
                    direction
                },
                _serializerColorModeOptions
            ),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await Http.PostAsync($"devices/{id}/color", json);
        response.EnsureSuccessStatusCode();
    }

    public Task<object?> Disconnect()
    {
        throw new NotImplementedException();
    }
}
