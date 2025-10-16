using SharpResults.Core;
using SharpResults.Extensions;
using SharpResults.Types;

namespace SharpResults.Test;

public class OptionResultExtensionsTests
{
    [Fact]
    public void OkOr_And_OkOrElse_Option()
    {
        var some = Option.Some(5);
        var none = Option.None<int>();
        var ok = some.OkOr("fail");
        var err = none.OkOr("fail");
        Assert.True(ok.IsOk);
        Assert.True(err.IsErr);
        var ok2 = some.OkOrElse(() => "fail");
        var err2 = none.OkOrElse(() => "fail");
        Assert.True(ok2.IsOk);
        Assert.True(err2.IsErr);
    }

    [Fact]
    public void OkOr_And_OkOrElse_NumericOption()
    {
        var some = NumericOption.Some(5);
        var none = NumericOption.None<int>();
        var ok = some.OkOr("fail");
        var err = none.OkOr("fail");
        Assert.True(ok.IsOk);
        Assert.True(err.IsErr);
        var ok2 = some.OkOrElse(() => "fail");
        var err2 = none.OkOrElse(() => "fail");
        Assert.True(ok2.IsOk);
        Assert.True(err2.IsErr);
    }

    [Fact]
    public void Transpose_OptionOfResult()
    {
        var someOk = Option.Some(Result.Ok<int, string>(5));
        var someErr = Option.Some(Result.Err<int, string>("fail"));
        var none = Option.None<Result<int, string>>();
        var t1 = someOk.Transpose();
        var t2 = someErr.Transpose();
        var t3 = none.Transpose();
        Assert.True(t1.IsOk);
        Assert.True(t2.IsErr);
        Assert.True(t3.IsOk);
        Assert.True(t3.Unwrap().IsNone);
    }

    [Fact]
    public void Transpose_ResultOfOption()
    {
        var okSome = Result.Ok<Option<int>, string>(Option.Some(5));
        var okNone = Result.Ok<Option<int>, string>(Option.None<int>());
        var err = Result.Err<Option<int>, string>("fail");
        var t1 = okSome.Transpose();
        var t2 = okNone.Transpose();
        var t3 = err.Transpose();
        Assert.True(t1.IsSome);
        Assert.True(t2.IsNone);
        Assert.True(t3.IsSome);
        Assert.True(t3.Unwrap().IsErr);
    }

    [Fact]
    public void Ok_And_Err_Extensions()
    {
        var ok = Result.Ok<int, string>(5);
        var err = Result.Err<int, string>("fail");
        Assert.True(ok.Ok().IsSome);
        Assert.True(err.Err().IsSome);
    }
}