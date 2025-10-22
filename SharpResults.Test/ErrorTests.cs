using SharpResults.Core.Types;

namespace SharpResults.Test;

public class ErrorTests
{
    [Fact]
    public void Error_String_And_Exception_Conversion()
    {
        ResultError e1 = "fail";
        ResultError e2 = new InvalidOperationException("bad");
        Assert.Equal("fail", e1.ToString());
        Assert.Equal("bad", e2.ToString());
    }
}