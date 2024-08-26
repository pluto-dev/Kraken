using System.Text.Json.Serialization;
using Kraken.Desktop.Converters;

namespace Kraken.Desktop.Models;

public record KrakenStatus(
    [property: JsonConverter(typeof(KrakenStatusConverter))]
        (string? Property, double Value, string? Unit) Temp,
    [property: JsonConverter(typeof(KrakenStatusConverter))]
        (string? Property, double Value, string? Unit) Speed,
    [property: JsonConverter(typeof(KrakenStatusConverter))]
        (string? Property, double Value, string? Unit) Duty
);
