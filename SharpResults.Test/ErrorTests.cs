using SharpResults.Core;

namespace SharpResults.Test;

public class ErrorTests
{
    [Fact]
    public void Error_String_And_Exception_Conversion()
    {
        Error e1 = "fail";
        Error e2 = new InvalidOperationException("bad");
        Assert.Equal("fail", (string?)e1);
        Assert.Equal("bad", (string?)e2);
        Assert.Equal("fail", e1.ToString());
        Assert.Equal("bad", e2.ToString());
    }

    [Fact]
    public void Error_From_Methods()
    {
        var ex = new InvalidOperationException("fail");
        var e1 = Error.From(ex);
        var e2 = Error.From("fail");
        Assert.Equal("fail", (string?)e1);
        Assert.Equal("fail", (string?)e2);
    }
}