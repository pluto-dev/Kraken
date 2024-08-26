using System.Text.Json.Serialization;
using Kraken.Desktop.Converters;

namespace Kraken.Desktop.Models;

public record KrakenInitStatus(
    [property: JsonConverter(typeof(KrakenInitConverter))]
        (string? Property, string? Value, string? Unit) Firmware,
    [property: JsonConverter(typeof(KrakenInitConverter))]
        (string? Property, string Value, string? Unit) Logo,
    [property: JsonConverter(typeof(KrakenInitConverter))]
        (string? Property, string Value, string? Unit) Ring
);
