using SharpResults.Core;
using SharpResults.Extensions.Collections;
using SharpResults.Types;

namespace SharpResults.Test;

public class OptionCollectionsExtensionsTests
{
    [Fact]
    public void ToList_ConvertsOptionToList()
    {
        var some = Option.Some(5);
        var none = Option.None<int>();
        Assert.Equal([5], some.ToList());
        Assert.Empty(none.ToList());
    }

    [Fact]
    public void Sequence_ReturnsOptionOfAllValuesOrNone()
    {
        var options = new List<Option<int>> { Option.Some(1), Option.Some(2) };
        var seq = options.Sequence();
        Assert.True(seq.IsSome);
        Assert.Equal([1, 2], seq.Unwrap());
        var options2 = new List<Option<int>> { Option.Some(1), Option.None<int>() };
        Assert.True(options2.Sequence().IsNone);
    }

    [Fact]
    public void SequenceList_ReturnsOptionOfListOrNone()
    {
        var options = new List<Option<int>> { Option.Some(1), Option.Some(2) };
        var seq = options.SequenceList();
        Assert.True(seq.IsSome);
        Assert.Equal(new List<int> { 1, 2 }, seq.Unwrap());
        var options2 = new List<Option<int>> { Option.Some(1), Option.None<int>() };
        Assert.True(options2.SequenceList().IsNone);
    }

    [Fact]
    public void Values_ExtractsAllSomeValues()
    {
        var options = new List<Option<int>> { Option.Some(1), Option.None<int>(), Option.Some(3) };
        var values = options.Values().ToList();
        Assert.Equal(new[] { 1, 3 }, values);
    }
}