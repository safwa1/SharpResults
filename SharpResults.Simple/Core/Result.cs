using System.Diagnostics;
using System.Runtime.CompilerServices;
using SharpResults.Core.Delegates;
using SharpResults.Core.Types;
using SharpResults.Exceptions;
using SharpResults.Simple.Types;
using SharpResults.Types;

namespace SharpResults.Simple.Core;

public delegate TSource TryFunc<out TSource>();

/// <summary>
/// This class contains static methods for creating a <see cref="Result{T}"/>.
/// </summary>
public static class Result
{

    /// <summary>
    /// Creates a <see cref="Result{T}"/> in the <c>Ok</c> state, containing
    /// the given value.
    /// </summary>
    /// <typeparam name="T">The type of value the result contains.</typeparam>
    /// <param name="value">The value to store in the result.</param>
    /// <returns>A result object containing the given value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the value is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Ok<T>(T value)
        where T : notnull
    {
        return new Result<T>(value);
    }

    /// <summary>
    /// Creates a <see cref="Result{T}"/> in the <c>Err</c> state,
    /// containing the given error value.
    /// </summary>
    /// <typeparam name="T">The type of the value the result would contain if it were not in the <c>Err</c> state.</typeparam>
    /// <param name="error">The error value to store in the result.</param>
    /// <returns>A result object containing the given error value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the error value is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Err<T>(ResultError error)
        where T : notnull
    {
        return new Result<T>(error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Err<T>(Result<T> result)
        where T : notnull
    {
        return Err<T>(result.UnwrapErr());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T2> Err<T, T2>(Result<T> result)
        where T : notnull
        where T2 : notnull
    {
        return Err<T2>(result.UnwrapErr());
    }

    /// <summary>
    /// Creates a <see cref="Result{T}"/> in the <c>Err</c> state,
    /// containing the given error message.
    /// </summary>
    /// <typeparam name="T">The type of the value the result would contain if it were not in the <c>Err</c> state.</typeparam>
    /// <param name="errorMessage">The error message to store in the result.</param>
    /// <returns>A result object containing the given error message.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the error message is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Err<T>(string errorMessage)
        where T : notnull
    {
        return new Result<T>(errorMessage);
    }

    /// <summary>
    /// Creates a <see cref="Result{T}"/> in the <c>Err</c> state,
    /// containing the given exception.
    /// </summary>
    /// <typeparam name="T">The type of the value the result would contain if it were not in the <c>Err</c> state.</typeparam>
    /// <param name="ex">The exception to store in the result.</param>
    /// <returns>A result object containing the given exception.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the exception is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Err<T>(Exception ex)
        where T : notnull
    {
        return new Result<T>(ex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Try<T>(TryFunc<T> func)
        where T : notnull
    {
        try
        {
            return Ok(func());
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Attempts to call an asynchronous <paramref name="func"/>, wrapping the returned value in an <c>Ok</c> result.
    /// Any exceptions will be caught and returned in an <c>Err</c> result.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the given function.</typeparam>
    /// <param name="func">The asynchronous function to attempt to call.</param>
    /// <returns>The return value of <paramref name="func"/> wrapped in <c>Ok</c>, or <c>Err</c> containing any exception that was thrown.</returns>
    [OverloadResolutionPriority(1)]
    public static async Task<Result<T>> TryAsync<T>(AsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

    /// <summary>
    /// Overload for functions returning ValueTask.
    /// </summary>
    [OverloadResolutionPriority(0)]
    public static async Task<Result<T>> TryAsync<T>(ValueAsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

#else

    /// <summary>
    /// Attempts to call an asynchronous <paramref name="func"/>, wrapping the returned value in an <c>Ok</c> result.
    /// Any exceptions will be caught and returned in an <c>Err</c> result.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the given function.</typeparam>
    /// <param name="func">The asynchronous function to attempt to call.</param>
    /// <returns>The return value of <paramref name="func"/> wrapped in <c>Ok</c>, or <c>Err</c> containing any exception that was thrown.</returns>
    public static async Task<Result<T>> TryAsync<T>(AsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

    /// <summary>
    /// Overload for functions returning ValueTask.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(ValueAsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

#endif

    /// <summary>
    /// Executes the specified function and returns a <see cref="Result{T,TErr}"/>. 
    /// If the function throws, it captures the exception as an error.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A successful or failed result depending on whether the function throws.</returns>
    [DebuggerStepThrough]
    public static Result<T> From<T>(Func<T> func) where T : notnull
    {
        try
        {
            return Ok(func());
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

    /// <summary>
    /// Converts an <see cref="Option{T}"/> to a <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> if the option is <c>Some</c>;
    /// otherwise, an error <see cref="Result{T}"/> with a default error message.
    /// </returns>
    [DebuggerStepThrough]
    public static Result<T> From<T>(Option<T> option) where T : notnull
    {
        return option.IsSome
            ? Ok(option.Unwrap())
            : Err<T>(new NoneValueException());
    }

    /// <summary>
    /// Executes the specified action and returns a <see cref="Result{Unit, ResultError}"/>. 
    /// If the action throws, it captures the exception as an error.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A successful or failed result depending on whether the action throws.</returns>
    [DebuggerStepThrough]
    public static Result<Unit> From(Action action)
    {
        try
        {
            action();
            return Ok(Unit.Default);
        }
        catch (Exception ex)
        {
            return Err<Unit>(ex);
        }
    }

    // ===============
    public static async Task<Result<T>> TryAsync<T, TParam>(
        TParam parameter,
        AsyncFunc<T, TParam> asyncFunc)
        where T : notnull
    {
        try
        {
            var value = await asyncFunc(parameter).ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err<T>(new ResultError(ex.Message, ex));
        }
    }

    public static async Task<Result<T>> TryAsync<T, TParam>(
        TParam parameter,
        ValueAsyncFunc<T, TParam> asyncFunc)
        where T : notnull
    {
        try
        {
            var value = await asyncFunc(parameter).ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return Err<T>(new ResultError(ex.Message, ex));
        }
    }
}