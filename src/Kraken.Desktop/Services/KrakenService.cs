using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kraken.Desktop.Services;

public class KrakenService
{
    private static readonly HttpClient Http = new();
    private static readonly Lazy<KrakenService> Lazy = new(() => new KrakenService());
    private List<KrakenDevice>? _devices;
    public static KrakenService Instance => Lazy.Value;

    private KrakenService()
    {
        Http.BaseAddress = new Uri("http://127.0.0.1:5000");
    }

    public async Task<List<KrakenDevice>?> GetDevices()
    {
        if (_devices is not null)
            return _devices;

        HttpResponseMessage? response = null;
        try
        {
            response = await Http.GetAsync("devices");
        }
        catch (HttpRequestException e) { }
        catch (IOException e) { }
        catch (SocketException e) { }

        if (response is not { IsSuccessStatusCode: true })
            return null;

        var content = await response.Content.ReadAsStringAsync();
        _devices = JsonSerializer.Deserialize<List<KrakenDevice>>(content);
        return _devices;
    }

    public async Task<KrakenInitStatus?> InitializeKraken(int? id)
    {
        Debug.Assert(id is not null, "Id can't be null");
        HttpResponseMessage? response = null;
        try
        {
            response = await Http.GetAsync($"devices/{id}/initialize");
        }
        catch (HttpRequestException e) { }
        catch (IOException e) { }
        catch (SocketException e) { }

        if (response is not { IsSuccessStatusCode: true })
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var status = JsonSerializer.Deserialize<KrakenInitStatus>(
            content,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        return status;
    }

    public async Task SetSpeedProfile(int? id, string channel, IEnumerable<(int X, int Y)> profile)
    {
        Debug.Assert(id is not null, "Id can't be null");
        using var json = new StringContent(
            JsonSerializer.Serialize(
                new { channel = channel, profile = profile },
                options: new() { Converters = { new SpeedProfileConverter() } }
            ),
            Encoding.UTF8,
            "application/json"
        );

        //TODO try
        using var response = await Http.PostAsync($"devices/{id}/speed", json);
        response.EnsureSuccessStatusCode();

        // TODO deserialize content
        var content = await response.Content.ReadAsStringAsync();
    }

    public async Task<KrakenStatus?> GetKrakenStatus(int? id)
    {
        Debug.Assert(id is not null, "Id can't be null");
        HttpResponseMessage? response = null;
        try
        {
            response = await Http.GetAsync($"devices/{id}/status");
        }
        catch (HttpRequestException e) { }
        catch (IOException e) { }
        catch (SocketException e) { }

        if (response is not { IsSuccessStatusCode: true })
            return null;
        var content = await response.Content.ReadAsStringAsync();
        var status = JsonSerializer.Deserialize<KrakenStatus>(
            content,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        return status;
    }

    public async Task SetColor(
        int? id,
        string channel,
        string mode,
        List<List<byte>?>? colors,
        string? speed,
        string? direction
    )
    {
        Debug.Assert(id is not null, "Id can't be null");

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
                options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                }
            ),
            Encoding.UTF8,
            "application/json"
        );

        //TODO try
        using var response = await Http.PostAsync($"devices/{id}/color", json);
        response.EnsureSuccessStatusCode();

        // TODO deserialize content
        var content = await response.Content.ReadAsStringAsync();
    }

    public Task<object?> Disconnect()
    {
        throw new NotImplementedException();
    }
}

public class SpeedProfileConverter : JsonConverter<ValueTuple<int, int>>
{
    public override (int, int) Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        throw new NotImplementedException();
    }

    public override void Write(
        Utf8JsonWriter writer,
        (int, int) value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Item1);
        writer.WriteNumberValue(value.Item2);
        writer.WriteEndArray();
    }
}

public record KrakenInitStatus(
    [property: JsonConverter(typeof(KrakenInitConverter))]
        (string? Property, string? Value, string? Unit) Firmware,
    [property: JsonConverter(typeof(KrakenInitConverter))]
        (string? Property, string Value, string? Unit) Logo,
    [property: JsonConverter(typeof(KrakenInitConverter))]
        (string? Property, string Value, string? Unit) Ring
);

public record KrakenStatus(
    [property: JsonConverter(typeof(KrakenStatusConverter))]
        (string? Property, double Value, string? Unit) Temp,
    [property: JsonConverter(typeof(KrakenStatusConverter))]
        (string? Property, double Value, string? Unit) Speed,
    [property: JsonConverter(typeof(KrakenStatusConverter))]
        (string? Property, double Value, string? Unit) Duty
);

public class KrakenStatusConverter : JsonConverter<(string? Property, double Value, string? Unit)>
{
    public override (string? Property, double Value, string? Unit) Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        reader.Read();
        var property = reader.GetString();
        reader.Read();
        var value = reader.GetDouble();
        reader.Read();
        var unit = reader.GetString();
        reader.Read();

        return (property, value, unit);
    }

    public override void Write(
        Utf8JsonWriter writer,
        (string? Property, double Value, string? Unit) value,
        JsonSerializerOptions options
    )
    {
        throw new NotImplementedException();
    }
}

public class KrakenInitConverter : JsonConverter<(string? Property, string? Value, string? Unit)>
{
    public override (string? Property, string? Value, string? Unit) Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        reader.Read();
        var property = reader.GetString();
        reader.Read();
        var value = reader.GetString();
        reader.Read();
        var unit = reader.GetString();
        reader.Read();

        return (property, value, unit);
    }

    public override void Write(
        Utf8JsonWriter writer,
        (string? Property, string? Value, string? Unit) value,
        JsonSerializerOptions options
    )
    {
        throw new NotImplementedException();
    }
}

public record KrakenDevice(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("animation_speeds")] Dictionary<string, int?> AnimationSpeeds,
    [property: JsonPropertyName("bus")] string Bus,
    [property: JsonPropertyName("color_channels")] Dictionary<string, int?> ColorChannels,
    [property: JsonPropertyName("color_modes")] Dictionary<string, IReadOnlyList<int?>> ColorModes,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("port")] int? Port,
    [property: JsonPropertyName("product_id")] int? ProductId,
    [property: JsonPropertyName("release_number")] int? ReleaseNumber,
    [property: JsonPropertyName("serial_number")] string SerialNumber,
    [property: JsonPropertyName("speed_channels")]
        Dictionary<string, IReadOnlyList<int?>> SpeedChannels,
    [property: JsonPropertyName("vendor_id")] int? VendorId
);
