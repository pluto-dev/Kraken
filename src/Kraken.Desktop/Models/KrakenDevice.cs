using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Kraken.Desktop.Models;

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
