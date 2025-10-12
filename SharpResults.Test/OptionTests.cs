using SharpResults.Extensions;
using SharpResults.Types;

namespace SharpResults.Test;

public class OptionTests
{
    [Fact]
    public void Some_ContainsValue()
    {
        var opt = Option.Some(123);
        Assert.True(opt.IsSome);
        Assert.False(opt.IsNone);
        Assert.Equal(123, opt.Unwrap());
    }

    [Fact]
    public void None_HasNoValue()
    {
        var opt = Option.None<int>();
        Assert.False(opt.IsSome);
        Assert.True(opt.IsNone);
        Assert.Throws<InvalidOperationException>(() => opt.Unwrap());
    }

    [Fact]
    public void Option_Equality()
    {
        var a = Option.Some("abc");
        var b = Option.Some("abc");
        var c = Option.None<string>();
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
        Assert.True(c == Option.None<string>());
    }

    [Fact]
    public void Option_Match_SomeAndNone()
    {
        var some = Option.Some(5);
        var none = Option.None<int>();
        Assert.Equal("some", some.Match(_ => "some", () => "none"));
        Assert.Equal("none", none.Match(_ => "some", () => "none"));
    }

    [Fact]
    public void Option_Map_MapsValue()
    {
        var some = Option.Some(2);
        var mapped = some.Map(x => x * 10);
        Assert.True(mapped.IsSome);
        Assert.Equal(20, mapped.Unwrap());
    }

    [Fact]
    public void Option_MapOrElse_And_MapOr()
    {
        var some = Option.Some(2);
        var none = Option.None<int>();
        Assert.Equal(4, some.MapOr(x => x * 2, 0));
        Assert.Equal(0, none.MapOr(x => x * 2, 0));
        Assert.Equal(4, some.MapOrElse(x => x * 2, () => 0));
        Assert.Equal(0, none.MapOrElse(x => x * 2, () => 0));
    }

    [Fact]
    public void Option_And_AndThen()
    {
        var some = Option.Some(1);
        var other = Option.Some("a");
        var none = Option.None<int>();
        Assert.True(some.And(other).IsSome);
        Assert.True(none.And(other).IsNone);
        Assert.True(some.AndThen(x => Option.Some(x + 1)).IsSome);
        Assert.True(none.AndThen(x => Option.Some(x + 1)).IsNone);
    }

    [Fact]
    public void Option_Or_OrElse()
    {
        var some = Option.Some(1);
        var none = Option.None<int>();
        Assert.True(some.Or(Option.Some(2)).IsSome);
        Assert.Equal(1, some.Or(Option.Some(2)).Unwrap());
        Assert.Equal(2, none.Or(Option.Some(2)).Unwrap());
        Assert.Equal(3, none.OrElse(() => Option.Some(3)).Unwrap());
    }

    [Fact]
    public void Option_Xor()
    {
        var some = Option.Some(1);
        var none = Option.None<int>();
        Assert.True(some.Xor(none).IsSome);
        Assert.True(none.Xor(some).IsSome);
        Assert.True(some.Xor(some).IsNone);
        Assert.True(none.Xor(none).IsNone);
    }

    [Fact]
    public void Option_Filter()
    {
        var some = Option.Some(5);
        var filtered = some.Filter(x => x > 3);
        Assert.True(filtered.IsSome);
        Assert.True(some.Filter(x => x < 3).IsNone);
    }

    [Fact]
    public void Option_Zip_ZipWith()
    {
        var a = Option.Some(1);
        var b = Option.Some(2);
        var zipped = a.Zip(b);
        Assert.True(zipped.IsSome);
        Assert.Equal((1, 2), zipped.Unwrap());
        var zippedWith = a.ZipWith(b, (x, y) => x + y);
        Assert.True(zippedWith.IsSome);
        Assert.Equal(3, zippedWith.Unwrap());
    }

    [Fact]
    public void Option_ImplicitConversion()
    {
        Option<int> a = 5;
        Assert.True(a.IsSome);
        Option<string> b = null;
        Assert.True(b.IsNone);
    }

    [Fact]
    public void Option_AsEnumerable_AsSpan()
    {
        var some = Option.Some(7);
        var list = some.AsEnumerable().ToList();
        Assert.Single(list);
        Assert.Equal(7, list[0]);
        var none = Option.None<int>();
        Assert.Empty(none.AsEnumerable());
        Assert.Equal(1, some.AsSpan().Length);
        Assert.Equal(0, none.AsSpan().Length);
    }
}