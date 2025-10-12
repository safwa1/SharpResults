using SharpResults.Exceptions;

namespace SharpResults.Test;

public class NoneValueExceptionTests
{
    [Fact]
    public void NoneValueException_HasCorrectMessage()
    {
        var ex = new NoneValueException();
        Assert.Equal("None", ex.Message);
    }
}