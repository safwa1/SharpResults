using SharpResults.Core;
using SharpResults.Extensions;

namespace SharpResults.Test;

public class NumericOptionExtensionsTests
{
    [Fact]
    public void Map_MapsValue()
    {
        var some = NumericOption.Some(2);
        var mapped = some.Map(x => x * 10);
        Assert.True(mapped.IsSome(out var v));
        Assert.Equal(20, v);
    }

    [Fact]
    public void MapOrElse_And_MapOr()
    {
        var some = NumericOption.Some(2);
        var none = NumericOption.None<int>();
        Assert.Equal(4, some.MapOr(x => x * 2, 0));
        Assert.Equal(0, none.MapOr(x => x * 2, 0));
        Assert.Equal(4, some.MapOrElse(x => x * 2, () => 0));
        Assert.Equal(0, none.MapOrElse(x => x * 2, () => 0));
    }

    [Fact]
    public void And_AndThen()
    {
        var some = NumericOption.Some(1);
        var other = NumericOption.Some(2);
        var none = NumericOption.None<int>();
        Assert.True(some.And(other).IsSome(out _));
        Assert.True(none.And(other).IsNone);
        Assert.True(some.AndThen(x => NumericOption.Some(x + 1)).IsSome(out var v));
        Assert.Equal(2, v);
        Assert.True(none.AndThen(x => NumericOption.Some(x + 1)).IsNone);
    }

    [Fact]
    public void Or_OrElse()
    {
        var some = NumericOption.Some(1);
        var none = NumericOption.None<int>();
        Assert.True(some.Or(NumericOption.Some(2)).IsSome(out var v1));
        Assert.Equal(1, v1);
        Assert.True(none.Or(NumericOption.Some(2)).IsSome(out var v2));
        Assert.Equal(2, v2);
        Assert.True(none.OrElse(() => NumericOption.Some(3)).IsSome(out var v3));
        Assert.Equal(3, v3);
    }

    [Fact]
    public void Xor()
    {
        var some = NumericOption.Some(1);
        var none = NumericOption.None<int>();
        Assert.True(some.Xor(none).IsSome(out _));
        Assert.True(none.Xor(some).IsSome(out _));
        Assert.True(some.Xor(some).IsNone);
        Assert.True(none.Xor(none).IsNone);
    }

    [Fact]
    public void Filter()
    {
        var some = NumericOption.Some(5);
        var filtered = some.Filter(x => x > 3);
        Assert.True(filtered.IsSome(out _));
        Assert.True(some.Filter(x => x < 3).IsNone);
    }

    [Fact]
    public void ZipWith_ZipsValues()
    {
        var a = NumericOption.Some(1);
        var b = NumericOption.Some(2);
        var zipped = a.ZipWith(b, (x, y) => x + y);
        Assert.True(zipped.IsSome(out var v));
        Assert.Equal(3, v);
    }

    [Fact]
    public void Values_ExtractsAllSomeValues()
    {
        var options = new[] { NumericOption.Some(1), NumericOption.None<int>(), NumericOption.Some(3) };
        var values = options.Values().ToList();
        Assert.Equal(new[] { 1, 3 }, values);
    }
}