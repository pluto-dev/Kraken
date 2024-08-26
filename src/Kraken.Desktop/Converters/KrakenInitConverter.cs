using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kraken.Desktop.Converters;

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
