using SharpResults.Core;
using SharpResults.Exceptions;
using SharpResults.Extensions;
using SharpResults.Types;
using Xunit.Abstractions;

namespace SharpResults.Test;

public class ResultTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ResultTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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
        Assert.Throws<ResultUnwrapException>(() => err.Expect("fail"));
        Assert.Throws<ResultExpectErrException>(() => ok.ExpectErr("fail"));
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
    
    // =======================
    
      [Fact]
    public void AsValueEnumerable_ForSingleOk_ShouldYieldSingleValue()
    {
        // Arrange
        var result = Result<int, string>.Ok(42);
        var enumerable = result.AsValueEnumerable();

        // Act
        var list = new List<int>();
        foreach (var item in enumerable)
        {
            list.Add(item);
        }

        // Assert
        Assert.Single(list);
        Assert.Equal(42, list[0]);
    }

    [Fact]
    public void AsValueEnumerable_ForSingleErr_ShouldYieldNothing()
    {
        // Arrange
        var result = Result<int, string>.Err("failed");
        var enumerable = result.AsValueEnumerable();

        // Act
        var list = new List<int>();
        foreach (var item in enumerable)
        {
            list.Add(item);
        }

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public void TryGetNext_ForSingleOk_ShouldReturnTrueThenFalse()
    {
        // Arrange
        var result = Result<int, string>.Ok(99);
        var enumerator = result.AsValueEnumerable().GetEnumerator();

        // Act & Assert
        Assert.True(enumerator.TryGetNext(out var item1));
        Assert.Equal(99, item1);

        Assert.False(enumerator.TryGetNext(out _));
    }

    [Fact]
    public void TryGetNext_ForSingleErr_ShouldReturnFalseImmediately()
    {
        // Arrange
        var result = Result<int, string>.Err("error");
        var enumerator = result.AsValueEnumerable().GetEnumerator();

        // Act & Assert
        Assert.False(enumerator.TryGetNext(out _));
    }
    

    [Fact]
    public void TryGetSpan_ForSingleErr_ShouldReturnFalse()
    {
        // Arrange
        var result = Result<int, string>.Err("error");
        var enumerator = result.AsValueEnumerable().GetEnumerator();

        // Act
        var success = enumerator.TryGetSpan(out var span);

        // Assert
        Assert.False(success);
        Assert.True(span.IsEmpty);
    }
    
    [Fact]
    public void AsValueEnumerable_ForCollectionOk_ShouldYieldAllValues()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };
        var result = Result<IReadOnlyList<int>, string>.Ok(list);
        var enumerable = result.AsValueEnumerable();

        // Act
        var yielded = new List<int>();
        foreach (var item in enumerable)
        {
            yielded.Add(item);
        }

        // Assert
        Assert.Equal(list, yielded);
    }

    [Fact]
    public void AsValueEnumerable_ForCollectionErr_ShouldYieldNothing()
    {
        // Arrange
        var result = Result<IReadOnlyList<int>, string>.Err("collection error");
        var enumerable = result.AsValueEnumerable();

        // Act
        var list = new List<int>();
        foreach (var item in enumerable)
        {
            list.Add(item);
        }

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public void TryGetNext_ForCollectionOk_ShouldYieldAllValues()
    {
        // Arrange
        var list = new List<int> { 10, 20 };
        var result = Result<IReadOnlyList<int>, string>.Ok(list);
        var enumerator = result.AsValueEnumerable().GetEnumerator();

        // Act & Assert
        Assert.True(enumerator.TryGetNext(out var item1));
        Assert.Equal(10, item1);

        Assert.True(enumerator.TryGetNext(out var item2));
        Assert.Equal(20, item2);

        Assert.False(enumerator.TryGetNext(out _));
    }

    [Fact]
    public void TryGetSpan_ForCollection_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var result = Result<IReadOnlyList<int>, string>.Ok(new List<int> { 1, 2 });
        var enumerator = result.AsValueEnumerable().GetEnumerator();

        // Act
        var success = enumerator.TryGetSpan(out var span);

        // Assert
        Assert.False(success);
        Assert.True(span.IsEmpty);
    }
    
    [Fact]
    public void Current_BeforeMoveNext_ShouldThrow()
    {
        // Arrange
        var result = Result<int, string>.Ok(1);
        var enumerator = result.AsValueEnumerable().GetEnumerator();

        // Act & Assert
        Exception? thrownException = null;
        try
        {
            var current = enumerator.Current;
        }
        catch (Exception ex)
        {
            thrownException = ex;
        }
        
        Assert.IsType<InvalidOperationException>(thrownException);
    }
    

    [Fact]
    public void TryGetSpan_AfterTryGetNext_ShouldReturnFalse()
    {
        // Arrange
        var result = Result<int, string>.Ok(55);
        var enumerator = result.AsValueEnumerable().GetEnumerator();
        enumerator.TryGetNext(out _); // Consume the item

        // Act
        var success = enumerator.TryGetSpan(out var span);

        // Assert
        Assert.False(success);
        Assert.True(span.IsEmpty);
    }
    
    
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void AsValueEnumerable_ForCollectionOk_ShouldHandleDifferentListSizes(int size)
    {
        // Arrange
        var list = Enumerable.Range(0, size).ToList();
        var result = Result<IReadOnlyList<int>, string>.Ok(list);
        var enumerable = result.AsValueEnumerable();

        // Act
        var yielded = new List<int>();
        foreach (var item in enumerable)
        {
            yielded.Add(item);
        }

        // Assert
        Assert.Equal(size, yielded.Count);
        Assert.Equal(list, yielded);
    }
}
