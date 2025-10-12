using SharpResults.Extensions;

namespace SharpResults.Test;

public class BoolExtensionsTests
{
    [Fact]
    public void Then_ReturnsOptionIfTrue()
    {
        bool cond = true;
        var opt = cond.Then(() => 42);
        Assert.True(opt.IsSome);
        Assert.Equal(42, opt.Unwrap());
    }

    [Fact]
    public void Then_ReturnsNoneIfFalse()
    {
        bool cond = false;
        var opt = cond.Then(() => 42);
        Assert.True(opt.IsNone);
    }

    [Fact]
    public void ThenSome_ReturnsOptionIfTrue()
    {
        bool cond = true;
        var opt = cond.ThenSome(99);
        Assert.True(opt.IsSome);
        Assert.Equal(99, opt.Unwrap());
    }

    [Fact]
    public void ThenSome_ReturnsNoneIfFalse()
    {
        bool cond = false;
        var opt = cond.ThenSome(99);
        Assert.True(opt.IsNone);
    }
}