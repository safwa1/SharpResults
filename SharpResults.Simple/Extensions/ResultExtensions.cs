using System.Runtime.CompilerServices;
using SharpResults.Core.Types;
using SharpResults.Simple.Types;
using SharpResults.Types;
using static System.ArgumentNullException;
using Result = SharpResults.Simple.Core.Result;

namespace SharpResults.Simple.Extensions;

/// <summary>
/// Extension methods for the single-type <see cref="Result{T}"/> version.
/// Uses <see cref="ResultError"/> as the unified error type.
/// </summary>
public static class ResultTExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T2> Map<T, T2>(this Result<T> self, Func<T, T2> mapper)
        where T : notnull where T2 : notnull
    {
        return self.Match(
            ok => Result<T2>.Ok(mapper(ok)),
            Result<T2>.Err
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T2> MapSafe<T1, T2>(this Result<T1> self, Func<T1, T2> mapper)
        where T1 : notnull
        where T2 : notnull
    {
        if(self.IsErr) 
            return Result<T2>.Err(self.UnwrapErr());
        
        try
        {
            return self.Match(
                ok: x => Result<T2>.Ok(mapper(x)),
                Result<T2>.Err
            );

        }
        catch (Exception ex)
        {
            return Result<T2>.Err(ex);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> MapErr<T, TErr>(this Result<T> self, Func<ResultError, TErr> errMapper)
        where T : notnull
        where TErr : notnull
    {
        return self.Match(
            ok: SharpResults.Core.Result.Ok<T, TErr>,
            err: e => SharpResults.Core.Result.Err<T, TErr>(errMapper(e))
        );
    }

    // ----------------------------
    // And / AndThen
    // ----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T2> And<T, T2>(this Result<T> self, Result<T2> other)
        where T : notnull where T2 : notnull
        => self.IsOk ? other : Result.Err<T2>(self.UnwrapErr());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T2> AndThen<T, T2>(this Result<T> self, Func<T, Result<T2>> binder)
        where T : notnull where T2 : notnull
    {
        return self.Match(
            binder,
            Result.Err<T2>
        );
    }

    // ----------------------------
    // Or / OrElse
    // ----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Or<T>(this Result<T> self, Result<T> other)
        where T : notnull
        => self.IsOk ? self : other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> OrElse<T>(this Result<T> self, Func<ResultError, Result<T>> elseFunc)
        where T : notnull
    {
        return self.Match(
            ok: Result.Ok,
            err: elseFunc
        );
    }

    // ----------------------------
    // MapOr / MapOrElse / UnwrapOr
    // ----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T2 MapOr<T, T2>(this Result<T> self, Func<T, T2> mapper, T2 defaultValue)
        where T : notnull where T2 : notnull
    {
        ThrowIfNull(mapper);
        return self.IsOk ? mapper(self.Unwrap()) : defaultValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T2 MapOrElse<T, T2>(this Result<T> self, Func<T, T2> mapper, Func<ResultError, T2> onError)
        where T : notnull where T2 : notnull
        => self.IsOk ? mapper(self.Unwrap()) : onError(self.UnwrapErr());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UnwrapOr<T>(this Result<T> self, T defaultValue)
        where T : notnull
        => self.IsOk ? self.Unwrap() : defaultValue;

    // ----------------------------
    // Inspect / InspectErr
    // ----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Inspect<T>(this Result<T> self, Action<T> action)
        where T : notnull
    {
        ThrowIfNull(action);
        if (self.IsOk)
        {
            action(self.Unwrap());
        }

        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> InspectErr<T>(this Result<T> self, Action<ResultError> action)
        where T : notnull
    {
        if (self.IsErr)
        {
            action(self.UnwrapErr());
        }

        return self;
    }

    // ----------------------------
    // Flatten / Where / Validate
    // ----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Flatten<T>(this Result<Result<T>> nested)
        where T : notnull
        => nested.Match(
            ok: x => x,
            err: Result.Err<T>
        );
        
            
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<U> Select<T, U>(this Result<T> self, Func<T, U> selector)
        where U : notnull
        where T : notnull
        => self.Map(selector);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<V> SelectMany<T, U, V>(this Result<T> self, Func<T, Result<U>> binder,
        Func<T, U, V> projector)
        where T : notnull
        where V : notnull
        where U : notnull
    {
        return self.AndThen(t =>
            binder(t).Map(u => projector(t, u))
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Where<T>(
        this Result<T> self,
        Func<T, bool> predicate,
        Func<ResultError> onFailure
    )
        where T : notnull
    {
        if (self.IsErr)
            return self;

        var val = self.Unwrap();
        return predicate(val)
            ? self
            : Result<T>.Err(onFailure());
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static Result<T> Validate<T>(
    //     this T value,
    //     Func<T, bool> predicate,
    //     string errorMessage
    // )
    //     where T : notnull
    //     => predicate(value)
    //         ? Result<T>.Ok(value)
    //         : Result<T>.Err(new ResultError(errorMessage));

    // ----------------------------
    // Contains
    // ----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this Result<T> self, T value)
        where T : notnull
        => self.IsOk && EqualityComparer<T>.Default.Equals(self.Unwrap(), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsErr<T>(this Result<T> self, ResultError error)
        where T : notnull
        => self.IsErr && self.UnwrapErr().Equals(error);
}
