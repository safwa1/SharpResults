using SharpResults.Extensions;

namespace SharpResults.Test;

public class NumberExtensionsTests
{
    [Fact]
    public void Parse_ParsesNumber()
    {
        var opt = "123".Parse<int>();
        Assert.True(opt.IsSome);
        Assert.Equal(123, opt.Unwrap());
    }

    [Fact]
    public void Parse_ReturnsNoneOnInvalid()
    {
        var opt = "abc".Parse<int>();
        Assert.True(opt.IsNone);
    }
}