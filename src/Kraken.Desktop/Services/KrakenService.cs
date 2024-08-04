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
    public static KrakenService Instance => Lazy.Value;

    private KrakenService()
    {
        Http.BaseAddress = new Uri("http://127.0.0.1:5000");
    }

    public async Task<List<KrakenDevice>?> GetDevices()
    {
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
        var status = JsonSerializer.Deserialize<List<KrakenDevice>>(content);
        return status;
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

public record ColorChannels(
    [property: JsonPropertyName("external")] int? External,
    [property: JsonPropertyName("logo")] int? Logo,
    [property: JsonPropertyName("ring")] int? Ring,
    [property: JsonPropertyName("sync")] int? Sync
);

public record ColorModes(
    [property: JsonPropertyName("alternating-3")] IReadOnlyList<int?> Alternating3,
    [property: JsonPropertyName("alternating-4")] IReadOnlyList<int?> Alternating4,
    [property: JsonPropertyName("alternating-5")] IReadOnlyList<int?> Alternating5,
    [property: JsonPropertyName("alternating-6")] IReadOnlyList<int?> Alternating6,
    [property: JsonPropertyName("backwards-marquee-3")] IReadOnlyList<int?> BackwardsMarquee3,
    [property: JsonPropertyName("backwards-marquee-4")] IReadOnlyList<int?> BackwardsMarquee4,
    [property: JsonPropertyName("backwards-marquee-5")] IReadOnlyList<int?> BackwardsMarquee5,
    [property: JsonPropertyName("backwards-marquee-6")] IReadOnlyList<int?> BackwardsMarquee6,
    [property: JsonPropertyName("backwards-moving-alternating-3")]
        IReadOnlyList<int?> BackwardsMovingAlternating3,
    [property: JsonPropertyName("backwards-moving-alternating-4")]
        IReadOnlyList<int?> BackwardsMovingAlternating4,
    [property: JsonPropertyName("backwards-moving-alternating-5")]
        IReadOnlyList<int?> BackwardsMovingAlternating5,
    [property: JsonPropertyName("backwards-moving-alternating-6")]
        IReadOnlyList<int?> BackwardsMovingAlternating6,
    [property: JsonPropertyName("backwards-rainbow-flow")] IReadOnlyList<int?> BackwardsRainbowFlow,
    [property: JsonPropertyName("backwards-rainbow-pulse")]
        IReadOnlyList<int?> BackwardsRainbowPulse,
    [property: JsonPropertyName("backwards-spectrum-wave")]
        IReadOnlyList<int?> BackwardsSpectrumWave,
    [property: JsonPropertyName("backwards-super-rainbow")]
        IReadOnlyList<int?> BackwardsSuperRainbow,
    [property: JsonPropertyName("breathing")] IReadOnlyList<int?> Breathing,
    [property: JsonPropertyName("candle")] IReadOnlyList<int?> Candle,
    [property: JsonPropertyName("covering-backwards-marquee")]
        IReadOnlyList<int?> CoveringBackwardsMarquee,
    [property: JsonPropertyName("covering-marquee")] IReadOnlyList<int?> CoveringMarquee,
    [property: JsonPropertyName("fading")] IReadOnlyList<int?> Fading,
    [property: JsonPropertyName("fixed")] IReadOnlyList<int?> Fixed,
    [property: JsonPropertyName("loading")] IReadOnlyList<int?> Loading,
    [property: JsonPropertyName("marquee-3")] IReadOnlyList<int?> Marquee3,
    [property: JsonPropertyName("marquee-4")] IReadOnlyList<int?> Marquee4,
    [property: JsonPropertyName("marquee-5")] IReadOnlyList<int?> Marquee5,
    [property: JsonPropertyName("marquee-6")] IReadOnlyList<int?> Marquee6,
    [property: JsonPropertyName("moving-alternating-3")] IReadOnlyList<int?> MovingAlternating3,
    [property: JsonPropertyName("moving-alternating-4")] IReadOnlyList<int?> MovingAlternating4,
    [property: JsonPropertyName("moving-alternating-5")] IReadOnlyList<int?> MovingAlternating5,
    [property: JsonPropertyName("moving-alternating-6")] IReadOnlyList<int?> MovingAlternating6,
    [property: JsonPropertyName("off")] IReadOnlyList<int?> Off,
    [property: JsonPropertyName("pulse")] IReadOnlyList<int?> Pulse,
    [property: JsonPropertyName("rainbow-flow")] IReadOnlyList<int?> RainbowFlow,
    [property: JsonPropertyName("rainbow-pulse")] IReadOnlyList<int?> RainbowPulse,
    [property: JsonPropertyName("spectrum-wave")] IReadOnlyList<int?> SpectrumWave,
    [property: JsonPropertyName("starry-night")] IReadOnlyList<int?> StarryNight,
    [property: JsonPropertyName("super-breathing")] IReadOnlyList<int?> SuperBreathing,
    [property: JsonPropertyName("super-fixed")] IReadOnlyList<int?> SuperFixed,
    [property: JsonPropertyName("super-rainbow")] IReadOnlyList<int?> SuperRainbow,
    [property: JsonPropertyName("tai-chi")] IReadOnlyList<int?> TaiChi,
    [property: JsonPropertyName("water-cooler")] IReadOnlyList<int?> WaterCooler,
    [property: JsonPropertyName("wings")] IReadOnlyList<int?> Wings
);

public record KrakenDevice(
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("animation_speeds")] IReadOnlyList<string> AnimationSpeeds,
    [property: JsonPropertyName("bus")] string Bus,
    [property: JsonPropertyName("color_channels")] ColorChannels ColorChannels,
    [property: JsonPropertyName("color_modes")] ColorModes ColorModes,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("port")] int? Port,
    [property: JsonPropertyName("product_id")] int? ProductId,
    [property: JsonPropertyName("release_number")] int? ReleaseNumber,
    [property: JsonPropertyName("serial_number")] string SerialNumber,
    [property: JsonPropertyName("speed_channels")] SpeedChannels SpeedChannels,
    [property: JsonPropertyName("vendor_id")] int? VendorId
);

public record SpeedChannels([property: JsonPropertyName("pump")] IReadOnlyList<int?> Pump);
