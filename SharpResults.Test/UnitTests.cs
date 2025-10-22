using SharpResults.Core;
using SharpResults.Core.Types;

namespace SharpResults.Test;

public class UnitTests
{
    [Fact]
    public void Unit_Equality_And_Operators()
    {
        var a = Unit.Default;
        var b = new Unit();
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.True(a <= b);
        Assert.True(a >= b);
        Assert.False(a < b);
        Assert.False(a > b);
    }

    [Fact]
    public void Unit_ToString_And_Conversions()
    {
        var u = Unit.Default;
        Assert.Equal("()", u.ToString());
        ValueTuple tuple = u;
        Unit u2 = tuple;
        Assert.Equal(u, u2);
    }

    [Fact]
    public void Unit_TryFormat()
    {
        var u = Unit.Default;
        Span<char> buffer = stackalloc char[2];
        Assert.True(u.TryFormat(buffer, out int written, default, null));
        Assert.Equal(2, written);
        Assert.Equal("()", new string(buffer));
    }
}