using System.Text.Json;
using System.Text.Json.Serialization;
using SharpResults.Types;

namespace SharpResults.Converters;

/// <summary>
/// Supports <see cref="Unit"/> in System.Text.Json serialization.
/// </summary>
internal sealed class UnitJsonConverter : JsonConverter<Unit>
{
    /// <summary>
    /// Creates a new instance of UnitJsonConverter.
    /// </summary>
    public UnitJsonConverter()
    {
    }

    /// <inheritdoc/>
    public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Null)
            throw new JsonException();

        return default;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }
}
