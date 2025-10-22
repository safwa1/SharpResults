﻿using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpResults.Core.Types;
using SharpResults.Simple.Types;
using static System.ArgumentNullException;

namespace SharpResults.Simple.Core;

/// <summary>
/// Supports <see cref="Result{T}"/> in System.Text.Json serialization.
/// </summary>
internal sealed class ResultJsonConverter : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        ThrowIfNull(typeToConvert);

        return typeToConvert.IsGenericType
            && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ThrowIfNull(typeToConvert);
        ThrowIfNull(options);

        var genericArgs = typeToConvert.GetGenericArguments();
        Type valueType = genericArgs[0];
        Type errType = genericArgs[1];

        var converter = Activator.CreateInstance(
            typeof(ResultJsonConverterInner<>).MakeGenericType([valueType, errType]),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [options],
            culture: null
        ) as JsonConverter;

        return converter;
    }

    private sealed class ResultJsonConverterInner<T> : JsonConverter<Result<T>>
        where T : notnull 
    {
        private readonly JsonConverter<T> _valueConverter;
        private readonly JsonConverter<ResultError> _errConverter;
        private readonly Type _valueType;
        private readonly Type _errType;

        public ResultJsonConverterInner(JsonSerializerOptions options)
        {
            // Cache the types.
            _valueType = typeof(T);
            _errType = typeof(ResultError);

            // For performance, use the existing converter.
            _valueConverter = (JsonConverter<T>)options.GetConverter(_valueType);
            _errConverter = (JsonConverter<ResultError>)options.GetConverter(_errType);
        }

        public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            Result<T> output;

            if (reader.ValueSpan.SequenceEqual("ok"u8) && reader.Read())
            {
                output = Result.Ok<T>(_valueConverter.Read(ref reader, _valueType, options)!);
            }
            else if (reader.ValueSpan.SequenceEqual("err"u8) && reader.Read())
            {
                output = Result.Err<T>(_errConverter.Read(ref reader, _errType, options)!);
            }
            else
            {
                throw new NotSupportedException($"Unable to read property: '{reader.GetString()}'");
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
                throw new JsonException();

            return output;
        }

        public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.WhenOk(out var val))
            {
                writer.WritePropertyName("ok"u8);
                _valueConverter.Write(writer, val, options);
            }
            else
            {
                writer.WritePropertyName("err"u8);
                _errConverter.Write(writer, value.UnwrapErr(), options);
            }

            writer.WriteEndObject();
        }
    }
}
