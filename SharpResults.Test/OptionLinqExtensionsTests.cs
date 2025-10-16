using SharpResults.Core;
using SharpResults.Extensions.Linq;

namespace SharpResults.Test;

public class OptionLinqExtensionsTests
{
    [Fact]
    public void Select_MapsValue()
    {
        var opt = Option.Some(5);
        var mapped = opt.Select(x => x * 2);
        Assert.True(mapped.IsSome);
        Assert.Equal(10, mapped.Unwrap());
    }

    [Fact]
    public void SelectMany_ChainsOptions()
    {
        var opt = Option.Some(3);
        var result = opt.SelectMany(x => Option.Some(x + 1), (x, y) => x * y);
        Assert.True(result.IsSome);
        Assert.Equal(12, result.Unwrap());
    }

    [Fact]
    public void Where_FiltersOption()
    {
        var opt = Option.Some(7);
        var filtered = opt.Where(x => x > 5);
        Assert.True(filtered.IsSome);
        Assert.True(opt.Where(x => x < 5).IsNone);
    }
}