using SharpResults.Extensions;

namespace SharpResults.Test;

public class ResultTests
{
    [Fact]
    public void Ok_ContainsValue()
    {
        var res = Result.Ok<int, string>(10);
        Assert.True(res.IsOk);
        Assert.False(res.IsErr);
        Assert.Equal(10, res.Unwrap());
    }

    [Fact]
    public void Err_ContainsError()
    {
        var res = Result.Err<int, string>("fail");
        Assert.True(res.IsErr);
        Assert.False(res.IsOk);
        Assert.Equal("fail", res.UnwrapErr());
    }

    [Fact]
    public void Result_Equality()
    {
        var a = Result.Ok<int, string>(1);
        var b = Result.Ok<int, string>(1);
        var c = Result.Err<int, string>("err");
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void Result_Match_OkAndErr()
    {
        var ok = Result.Ok<int, string>(5);
        var err = Result.Err<int, string>("fail");
        Assert.Equal("ok", ok.Match(_ => "ok", _ => "err"));
        Assert.Equal("err", err.Match(_ => "ok", _ => "err"));
    }

    [Fact]
    public void Result_Map_MapErr()
    {
        var ok = Result.Ok<int, string>(2);
        var mapped = ok.Map(x => x * 10);
        Assert.True(mapped.IsOk);
        Assert.Equal(20, mapped.Unwrap());
        var err = Result.Err<int, string>("fail");
        var mappedErr = err.MapErr(e => e + "ed");
        Assert.True(mappedErr.IsErr);
        Assert.Equal("failed", mappedErr.UnwrapErr());
    }

    [Fact]
    public void Result_And_AndThen()
    {
        var ok = Result.Ok<int, string>(1);
        var other = Result.Ok<string, string>("a");
        var err = Result.Err<int, string>("fail");
        Assert.True(ok.And(other).IsOk);
        Assert.True(err.And(other).IsErr);
        Assert.True(ok.AndThen(x => Result.Ok<string, string>(x.ToString())).IsOk);
        Assert.True(err.AndThen(x => Result.Ok<string, string>(x.ToString())).IsErr);
    }

    [Fact]
    public void Result_Or_OrElse()
    {
        var ok = Result.Ok<int, string>(1);
        var err = Result.Err<int, string>("fail");
        Assert.True(ok.Or(Result.Ok<int, int>(2)).IsOk);
        Assert.Equal(1, ok.Or(Result.Ok<int, int>(2)).Unwrap());
        Assert.Equal(2, err.Or(Result.Ok<int, int>(2)).Unwrap());
        Assert.Equal(3, err.OrElse(_ => Result.Ok<int, int>(3)).Unwrap());
    }

    [Fact]
    public void Result_Contains_ContainsErr()
    {
        var ok = Result.Ok<int, Exception>(5);
        var err = Result.Err<int, Exception>(new InvalidOperationException());
        Assert.True(ok.Contains(5));
        Assert.False(ok.Contains(6));
        Assert.True(err.ContainsErr(err.UnwrapErr()));
    }

    [Fact]
    public void Result_UnwrapOrElse_UnwrapOrDefault()
    {
        var ok = Result.Ok<int, string>(5);
        var err = Result.Err<int, string>("fail");
        Assert.Equal(5, ok.UnwrapOrElse(_ => 10));
        Assert.Equal(10, err.UnwrapOrElse(_ => 10));
        Assert.Equal(5, ok.UnwrapOrDefault());
        Assert.Equal(0, err.UnwrapOrDefault());
    }

    [Fact]
    public void Result_Expect_ExpectErr()
    {
        var ok = Result.Ok<int, string>(5);
        var err = Result.Err<int, string>("fail");
        Assert.Equal(5, ok.Expect("should not throw"));
        Assert.Equal("fail", err.ExpectErr("should not throw"));
        Assert.Throws<InvalidOperationException>(() => err.Expect("fail"));
        Assert.Throws<InvalidOperationException>(() => ok.ExpectErr("fail"));
    }

    [Fact]
    public void Result_AsEnumerable_AsSpan()
    {
        var ok = Result.Ok<int, string>(7);
        var list = ok.AsEnumerable().ToList();
        Assert.Single(list);
        Assert.Equal(7, list[0]);
        var err = Result.Err<int, string>("fail");
        Assert.Empty(err.AsEnumerable());
        Assert.Equal(1, ok.AsSpan().Length);
        Assert.Equal(0, err.AsSpan().Length);
    }
}