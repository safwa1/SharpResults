using System.Text.Json;
using SharpResults.Types;

namespace SharpResults.Test;

public class ConvertersTests
{
    [Fact]
    public void OptionJsonConverter_SerializesAndDeserializes()
    {
        var opt = Option.Some(123);
        var json = JsonSerializer.Serialize(opt);
        var deserialized = JsonSerializer.Deserialize<Option<int>>(json);
        Assert.True(deserialized.IsSome);
        Assert.Equal(123, deserialized.Unwrap());
        var none = Option.None<int>();
        var jsonNone = JsonSerializer.Serialize(none);
        var deserializedNone = JsonSerializer.Deserialize<Option<int>>(jsonNone);
        Assert.True(deserializedNone.IsNone);
    }

    [Fact]
    public void ResultJsonConverter_SerializesAndDeserializes()
    {
        var ok = Result.Ok<int, string>(42);
        var json = JsonSerializer.Serialize(ok);
        var deserialized = JsonSerializer.Deserialize<Result<int, string>>(json);
        Assert.True(deserialized.IsOk);
        Assert.Equal(42, deserialized.Unwrap());
        var err = Result.Err<int, string>("fail");
        var jsonErr = JsonSerializer.Serialize(err);
        var deserializedErr = JsonSerializer.Deserialize<Result<int, string>>(jsonErr);
        Assert.True(deserializedErr.IsErr);
        Assert.Equal("fail", deserializedErr.UnwrapErr());
    }

    [Fact]
    public void NumericOptionJsonConverter_SerializesAndDeserializes()
    {
        var opt = NumericOption.Some(123);
        var json = JsonSerializer.Serialize(opt);
        var deserialized = JsonSerializer.Deserialize<NumericOption<int>>(json);
        Assert.True(deserialized.IsSome(out var v));
        Assert.Equal(123, v);
        var none = NumericOption.None<int>();
        var jsonNone = JsonSerializer.Serialize(none);
        var deserializedNone = JsonSerializer.Deserialize<NumericOption<int>>(jsonNone);
        Assert.True(deserializedNone.IsNone);
    }

    [Fact]
    public void UnitJsonConverter_SerializesAndDeserializes()
    {
        var unit = Unit.Default;
        var json = JsonSerializer.Serialize(unit);
        var deserialized = JsonSerializer.Deserialize<Unit>(json);
        Assert.Equal(unit, deserialized);
    }
}