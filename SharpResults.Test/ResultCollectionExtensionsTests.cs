using SharpResults.Extensions;
using SharpResults.Types;

namespace SharpResults.Test;

public class ResultCollectionExtensionsTests
{
    private static readonly int[] ExpectedInts = [1, 2];
    private static readonly string[] ExpectedStrings = ["fail1", "fail2"];

    [Fact]
    public void Values_ReturnsAllOkValues()
    {
        var results = new List<Result<int, string>>
        {
            Result.Ok<int, string>(1),
            Result.Err<int, string>("fail"),
            Result.Ok<int, string>(2)
        };
        var values = results.Values().ToList();
        Assert.Equal(ExpectedInts, values);
    }

    [Fact]
    public void Errors_ReturnsAllErrorValues()
    {
        var results = new List<Result<int, string>>
        {
            Result.Ok<int, string>(1),
            Result.Err<int, string>("fail1"),
            Result.Err<int, string>("fail2")
        };
        var errors = results.Errors().ToList();
        Assert.Equal(ExpectedStrings, errors);
    }
}