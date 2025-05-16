using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharpResults.Types;

public class OptionJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && 
               typeToConvert.GetGenericTypeDefinition() == typeof(Option<>);
    }

    public override JsonConverter CreateConverter(
        Type typeToConvert, 
        JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)Activator.CreateInstance(
            typeof(OptionJsonConverterInner<>).MakeGenericType(valueType))!;
    }

    private class OptionJsonConverterInner<T> : JsonConverter<Option<T>>
    {
        public override Option<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions? options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return Option<T>.None();

            return Option<T>.Some(
                JsonSerializer.Deserialize<T>(ref reader, options)!);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Option<T> value,
            JsonSerializerOptions options)
        {
            if (value.IsNone)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(writer, value.Value, options);
            }
        }
    }
}