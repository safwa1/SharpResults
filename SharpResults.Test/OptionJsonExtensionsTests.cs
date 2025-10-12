using System.Text.Json;
using System.Text.Json.Nodes;
using SharpResults.Extensions;

namespace SharpResults.Test;

public class OptionJsonExtensionsTests
{
    [Fact]
    public void GetOption_ReturnsSomeOrNone()
    {
        var json = JsonValue.Create(42);
        var opt = json.GetOption<int>();
        Assert.True(opt.IsSome);
        Assert.Equal(42, opt.Unwrap());
        var jsonStr = JsonValue.Create("abc");
        var optStr = jsonStr.GetOption<int>();
        Assert.True(optStr.IsNone);
    }

    [Fact]
    public void GetPropValue_ReturnsSomeOrNone()
    {
        var obj = new JsonObject { ["a"] = 5 };
        var opt = obj.GetPropValue<int>("a");
        Assert.True(opt.IsSome);
        Assert.Equal(5, opt.Unwrap());
        Assert.True(obj.GetPropValue<int>("b").IsNone);
    }

    [Fact]
    public void GetPropOption_ReturnsSomeOrNone()
    {
        var obj = new JsonObject { ["a"] = 5 };
        var opt = obj.GetPropOption("a");
        Assert.True(opt.IsSome);
        Assert.True(obj.GetPropOption("b").IsNone);
    }

    [Fact]
    public void GetPropOption_JsonElement()
    {
        var doc = JsonDocument.Parse("{\"a\":123}");
        var root = doc.RootElement;
        var opt = root.GetPropOption("a");
        Assert.True(opt.IsSome);
        Assert.Equal(123, opt.Unwrap().GetInt32());
        Assert.True(root.GetPropOption("b").IsNone);
    }
}