using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kraken.Desktop.Converters;

public class SpeedProfileConverter : JsonConverter<(int, int)>
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
