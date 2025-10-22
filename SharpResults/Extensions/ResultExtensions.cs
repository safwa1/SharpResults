﻿using System.Runtime.CompilerServices;
using SharpResults.Core;
using SharpResults.Core.Types;
using SharpResults.Types;
using static System.ArgumentNullException;

namespace SharpResults.Extensions;

/// <summary>
/// Extension methods for the <see cref="Result{T, TErr}"/> type.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a result by applying a function to a contained <c>Ok</c> value, leaving an <c>Err</c> value untouched.
    /// </summary>
    /// <typeparam name="T1">The <c>Ok</c> type contained by <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2">The <c>Ok</c> type contained by the return value.</typeparam>
    /// <typeparam name="TErr">The <c>Err</c> type.</typeparam>
    /// <param name="self">The result to map.</param>
    /// <param name="mapper">The function that converts a contained <c>Ok</c> value to <typeparamref name="T2"/>.</param>
    /// <returns>A result containing the mapped value, or <c>Err</c>.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="mapper"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T2, TErr> Map<T1, T2, TErr>(this Result<T1, TErr> self, Func<T1, T2> mapper)
        where T1 : notnull
        where T2 : notnull
        where TErr : notnull
    {
        return self.Match(
            ok: x => new Result<T2, TErr>(mapper(x)),
            err: Result.Err<T2, TErr>
        );
    }
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T2, ResultError> MapSafe<T1, T2>(this Result<T1, ResultError> self, Func<T1, T2> mapper)
        where T1 : notnull
        where T2 : notnull
    {
        if(self.IsErr) 
            return Result.Err<T2>(self.UnwrapErr());
        
        try
        {
            return self.Match(
                ok: x => new Result<T2, ResultError>(mapper(x)),
                err: Result.Err<T2, ResultError>
            );

        }
        catch (Exception ex)
        {
            return Result.Err<T2>(ex);
        }
    }

    /// <summary>
    /// Maps a result by applying a function to a contained <c>Err</c> value, leaving an <c>Ok</c> value untouched.
    /// </summary>
    /// <typeparam name="T">The <c>Ok</c> type.</typeparam>
    /// <typeparam name="T1Err">The <c>Err</c> type contained by <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2Err">The <c>Err</c> type contained by the return value.</typeparam>
    /// <param name="self">The result to map.</param>
    /// <param name="errMapper">The function that converts a contained <typeparamref name="T1Err"/> value to <typeparamref name="T2Err"/>.</param>
    /// <returns>A result containing the mapped error, or <c>Ok</c>.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="errMapper"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, T2Err> MapErr<T, T1Err, T2Err>(this Result<T, T1Err> self, Func<T1Err, T2Err> errMapper)
        where T : notnull
        where T2Err : notnull
        where T1Err : notnull
    {
        return self.Match(
            ok: Result.Ok<T, T2Err>,
            err: e => new(errMapper(e))
        );
    }

    /// <summary>
    /// Returns the provided default (if <c>Err</c>), or applies a function to the contained value (if <c>Ok</c>).
    /// <para>
    /// Arguments passed to <see cref="MapOr{T1, T2, TErr}(Result{T1, TErr}, Func{T1, T2}, T2)"/> are eagerly evaluated;
    /// if you are passing the result of a function call, it is recommended to use
    /// <see cref="MapOrElse{T1, T2, TErr}(Result{T1, TErr}, Func{T1, T2}, Func{TErr, T2})"/>, which is lazily evaluated.
    /// </para>
    /// </summary>
    /// <typeparam name="T1">The <c>Ok</c> type contained by <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2">The <c>Ok</c> type contained by the return value.</typeparam>
    /// <typeparam name="TErr">The <c>Err</c> type.</typeparam>
    /// <param name="self">The result to map.</param>
    /// <param name="mapper">The function that converts a contained <c>Ok</c> value to <typeparamref name="T2"/>.</param>
    /// <param name="defaultValue">The default value to return if the result is <c>Err</c>.</param>
    /// <returns>The mapped value, or the default value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="mapper"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T2 MapOr<T1, T2, TErr>(this Result<T1, TErr> self, Func<T1, T2> mapper, T2 defaultValue)
        where T1 : notnull
        where T2 : notnull
        where TErr : notnull
    {
        ThrowIfNull(mapper);
        return self.WhenOk(out var value) ? mapper(value) : defaultValue;
    }

    /// <summary>
    /// Maps a <c>Result</c> by applying fallback function <paramref name="defaultFactory"/> to a
    /// contained <c>Err</c> value, or function <paramref name="mapper"/> to a contained <c>Ok</c> value.
    /// <para>This function can be used to unpack a successful result while handling an error.</para>
    /// </summary>
    /// <typeparam name="T1">The <c>Ok</c> type contained by <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2">The <c>Ok</c> type contained by the return value.</typeparam>
    /// <typeparam name="TErr">The <c>Err</c> type.</typeparam>
    /// <param name="self">The result to map.</param>
    /// <param name="mapper">The function that converts a contained <c>Ok</c> value to <typeparamref name="T2"/>.</param>
    /// <param name="defaultFactory">The function that converts a contained <c>Err</c> value to <typeparamref name="T2"/>.</param>
    /// <returns>The mapped value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="mapper"/> or <paramref name="defaultFactory"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T2 MapOrElse<T1, T2, TErr>(this Result<T1, TErr> self, Func<T1, T2> mapper,
        Func<TErr, T2> defaultFactory)
        where T1 : notnull
        where T2 : notnull
        where TErr : notnull
    {
        return self.Match(mapper, defaultFactory);
    }

    /// <summary>
    /// Returns the contained <c>Ok</c> value, or a provided default.
    /// <para>
    /// Arguments passed to <see cref="UnwrapOr{T, TErr}(Result{T, TErr}, T)"/> are eagerly evaluated;
    /// if you are passing the result of a function call, it is recommended to use
    /// <see cref="Result{T, TErr}.UnwrapOrElse(Func{TErr, T})"/>, which is lazily evaluated.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The <c>Ok</c> type.</typeparam>
    /// <typeparam name="TErr">The <c>Err</c> type.</typeparam>
    /// <param name="self">The result to unwrap.</param>
    /// <param name="defaultValue">The default value to return if the result is <c>Err</c>.</param>
    /// <returns>The contained <c>Ok</c> value, or the provided default.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T UnwrapOr<T, TErr>(this Result<T, TErr> self, T defaultValue)
        where T : notnull
        where TErr : notnull
    {
        return self.WhenOk(out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Returns <paramref name="other"/> if <paramref name="self"/> is <c>Ok</c>, otherwise
    /// returns the <c>Err</c> value of <paramref name="self"/>.
    /// <para>
    /// Arguments passed to and are eagerly evaluated; if you are passing the result of a function call,
    /// it is recommended to use <see cref="AndThen{T1, T2, TErr}(Result{T1, TErr}, Func{T1, Result{T2, TErr}})"/>, which is lazily evaluated.
    /// </para>
    /// </summary>
    /// <typeparam name="T1">The <c>Ok</c> type of <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2">The <c>Ok</c> type of <paramref name="other"/>.</typeparam>
    /// <typeparam name="TErr">The <c>Err</c> type.</typeparam>
    /// <param name="self">The result.</param>
    /// <param name="other">The other result.</param>
    /// <returns>
    /// <paramref name="other"/> if <paramref name="self"/> is <c>Ok</c>, otherwise
    /// returns the <c>Err</c> value of <paramref name="self"/>.
    /// </returns>
    public static Result<T2, TErr> And<T1, T2, TErr>(this Result<T1, TErr> self, Result<T2, TErr> other)
        where T1 : notnull
        where T2 : notnull
        where TErr : notnull
    {
        var selfOk = !self.WhenErr(out var selfErr);
        return selfOk ? other : Result.Err<T2, TErr>(selfErr!);
    }

    /// <summary>
    /// Calls <paramref name="thenFunc"/> if the result is <c>Ok</c>, otherwise returns the <c>Err</c> value of <paramref name="self"/>.
    /// </summary>
    /// <typeparam name="T1">The <c>Ok</c> type of <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2">The <c>Ok</c> type returned by <paramref name="thenFunc"/>.</typeparam>
    /// <typeparam name="TErr">The <c>Err</c> type.</typeparam>
    /// <param name="self">The result.</param>
    /// <param name="thenFunc">The function to call with the <c>Ok</c> value, if any.</param>
    /// <returns>The result of calling <paramref name="thenFunc"/> if the result is <c>Ok</c>, otherwise the <c>Err</c> value of <paramref name="self"/>.</returns>
    public static Result<T2, TErr> AndThen<T1, T2, TErr>(
        this Result<T1, TErr> self,
        Func<T1, Result<T2, TErr>> thenFunc
    )
        where T1 : notnull
        where T2 : notnull
        where TErr : notnull
    {
        return self.Match(
            ok: thenFunc,
            err: Result.Err<T2, TErr>
        );
    }

    /// <summary>
    /// Returns <paramref name="other"/> if the result is <c>Err</c>, otherwise returns the <c>Ok</c> value of <paramref name="self"/>.
    /// <para>
    /// Arguments passed to <see cref="Or{T, T1Err, T2Err}(Result{T, T1Err}, Result{T, T2Err})"/> are eagerly evaluated;
    /// if you are passing the result of a function call, it is recommended to use
    /// <see cref="OrElse{T, T1Err, T2Err}(Result{T, T1Err}, Func{T1Err, Result{T, T2Err}})"/>, which is lazily evaluated.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The <c>Ok</c> type of the result.</typeparam>
    /// <typeparam name="T1Err">The <c>Err</c> type of <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2Err">The <c>Err</c> type of <paramref name="other"/>.</typeparam>
    /// <param name="self">The result.</param>
    /// <param name="other">The other result.</param>
    /// <returns>The <c>Ok</c> value of <paramref name="self"/>, or returns <paramref name="other"/>.</returns>
    public static Result<T, T2Err> Or<T, T1Err, T2Err>(this Result<T, T1Err> self, Result<T, T2Err> other)
        where T : notnull
        where T2Err : notnull
        where T1Err : notnull
    {
        return self.WhenOk(out var value) ? Result.Ok<T, T2Err>(value) : other;
    }

    /// <summary>
    /// Calls <paramref name="elseFunc"/> if the result is <c>Err</c>, otherwise returns the <c>Ok</c> value of <paramref name="self"/>.
    /// </summary>
    /// <typeparam name="T">The <c>Ok</c> type of the result.</typeparam>
    /// <typeparam name="T1Err">The <c>Err</c> type of <paramref name="self"/>.</typeparam>
    /// <typeparam name="T2Err">The <c>Err</c> type returned by <paramref name="elseFunc"/>.</typeparam>
    /// <param name="self">The result.</param>
    /// <param name="elseFunc">The function to call with the <c>Err</c> value, if any.</param>
    /// <returns>The <c>Ok</c> value of the result, or the result of passing the <c>Err</c> value to <paramref name="elseFunc"/>.</returns>
    public static Result<T, T2Err> OrElse<T, T1Err, T2Err>(this Result<T, T1Err> self,
        Func<T1Err, Result<T, T2Err>> elseFunc)
        where T : notnull
        where T2Err : notnull
        where T1Err : notnull
    {
        return self.Match(
            ok: Result.Ok<T, T2Err>,
            err: elseFunc
        );
    }

    /// <summary>
    /// Removes one level of nesting from a nested <see cref="Result{T, TErr}"/>.
    /// </summary>
    /// <typeparam name="T">The <c>Ok</c> type of the result.</typeparam>
    /// <typeparam name="TErr">The <c>Err</c> type of the result.</typeparam>
    /// <param name="self">The result.</param>
    /// <returns>The inner result.</returns>
    public static Result<T, TErr> Flatten<T, TErr>(this Result<Result<T, TErr>, TErr> self)
        where T : notnull
        where TErr : notnull
    {
        return self.Match(
            ok: x => x,
            err: Result.Err<T, TErr>
        );
    }


    public static Result<T, TErr> Inspect<T, TErr>(this Result<T, TErr> self, Action<T> action)
        where T : notnull where TErr : notnull
    {
        ThrowIfNull(action);
        if (self.IsOk)
        {
            action(self.Unwrap());
        }

        return self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> InspectErr<T, TErr>(this Result<T, TErr> self, Action<TErr> action)
        where T : notnull where TErr : notnull
    {
        if (self.IsErr)
        {
            action(self.UnwrapErr());
        }

        return self;
    }

    /// <summary>
    /// Checks if the result contains the specified value.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="value">The value to compare against the result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T, TErr>(this Result<T, TErr> result, T value) where T : notnull where TErr : notnull
    {
        return result.IsOk && EqualityComparer<T>.Default.Equals(result.Unwrap(), value);
    }

    /// <summary>
    /// Checks if the result contains the specified exception.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="exception">The exception to compare against the result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsErr<T>(this Result<T, Exception> result, Exception exception) where T : notnull
    {
        return result.IsErr && result.UnwrapErr() == exception;
    }

    /// <summary>
    /// LINQ-compatible select function for result values.
    /// </summary>
    /// <param name="self">The result to transform.</param>
    /// <param name="selector">The transformation function.</param>
    public static Result<U, TErr> Select<T, U, TErr>(this Result<T, TErr> self, Func<T, U> selector)
        where U : notnull
        where T : notnull
        where TErr : notnull
        => self.Map(selector);

    /// <summary>
    /// LINQ-compatible select many functions for chaining result operations.
    /// </summary>
    /// <param name="self">The result to bind.</param>
    /// <param name="binder">The function to produce the next result.</param>
    /// <param name="projector">The function to combine the results.</param>
    public static Result<V, TErr> SelectMany<T, U, V, TErr>(this Result<T, TErr> self, Func<T, Result<U, TErr>> binder,
        Func<T, U, V> projector)
        where T : notnull
        where V : notnull
        where U : notnull
        where TErr : notnull
    {
        return self.AndThen(t =>
            binder(t).Map(u => projector(t, u))
        );
    }

    /// <summary>
    /// Filters a result based on a predicate.
    /// If the predicate fails, returns an Err with the provided error.
    /// </summary>
    public static Result<T, TErr> Where<T, TErr>(
        this Result<T, TErr> result,
        Func<T, bool> predicate,
        Func<TErr> onFailure
    )
        where T : notnull
        where TErr : notnull
    {
        if (result.IsErr)
            return result;

        var value = result.Unwrap();
        if (!predicate(value))
            return Result<T, TErr>.Err(onFailure());

        return result;
    }

    public static Result<T, List<TErr>> ValidateAll<T, TErr>(
        this T value,
        params Func<T, Result<T, TErr>>[] validators) where T : notnull where TErr : notnull
    {
        var errors = new List<TErr>();
        foreach (var validator in validators)
        {
            if (validator(value).WhenErr(out var err))
                errors.Add(err!);
        }

        return errors.Count != 0
            ? Result.Err<T, List<TErr>>(errors)
            : Result.Ok<T, List<TErr>>(value);
    }

    // public static Result<T, string> Validate<T>(
    //     this T value,
    //     Func<T, bool> predicate,
    //     string errorMessage
    // ) where T : notnull
    // {
    //     return predicate(value)
    //         ? Result.Ok<T, string>(value)
    //         : Result.Err<T, string>(errorMessage);
    // }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultValueEnumerable<T, TErr> AsValueEnumerable<T, TErr>(this Result<T, TErr> result)
        where T : notnull
        where TErr : notnull => new(result);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultValueEnumerable<T, TErr> AsValueEnumerable<T, TErr>(this Result<IReadOnlyList<T>, TErr> result)
        where T : notnull
        where TErr : notnull
        => new(result);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> ToResult<T, TErr>(this T value) where T : notnull where TErr : notnull
        => Result.Ok<T, TErr>(value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, string> ToResult<T>(this T value) where T : notnull
        => Result.Ok<T, string>(value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> ToErr<T, TErr>(this TErr value) where T : notnull where TErr : notnull
        => Result.Err<T, TErr>(value);
    
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, string> ToErr<T>(this string value) where T : notnull
        => Result.Err<T>(value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, ResultError> ToErr<T>(this ResultError value) where T : notnull
        => Result.Err<T>(value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, ResultError> ToErr<T>(this Exception value) where T : notnull
        => Result.Err<T>(value);
}