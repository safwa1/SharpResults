using System.Diagnostics;
using System.Runtime.CompilerServices;
using SharpResults.Core.Attributes;
using SharpResults.Core.Delegates;
using SharpResults.Core.Types;
using SharpResults.Exceptions;
using SharpResults.Types;

namespace SharpResults.Core;

public delegate TSource TryFunc<out TSource>();

/// <summary>
/// This class contains static methods for creating a <see cref="Result{T, TErr}"/>.
/// </summary>
[PreludeExport]
public static class Result
{
    /// <summary>
    /// Creates a <see cref="Result{T, TErr}"/> in the <c>Ok</c> state, containing
    /// the given value.
    /// </summary>
    /// <typeparam name="T">The type of value the result contains.</typeparam>
    /// <typeparam name="TErr">The type of error the result may contain.</typeparam>
    /// <param name="value">The value to store in the result.</param>
    /// <returns>A result object containing the given value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the value is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Ok<T, TErr>(T value)
        where T : notnull
        where TErr : notnull
    {
        return new Result<T, TErr>(value);
    }

    /// <summary>
    /// Creates a <see cref="Result{T, TErr}"/> in the <c>Ok</c> state,
    /// with <c>string</c> as the error type.
    /// <para>This overload avoids explicit generic annotations when you want the error to be a simple message.</para>
    /// </summary>
    /// <typeparam name="T">The type of the value the result contains.</typeparam>
    /// <param name="value">The value to store in the result.</param>
    /// <returns>A result object containing the given value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the value is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, string> Ok<T>(T value)
        where T : notnull
    {
        return new Result<T, string>(value);
    }

    /// <summary>
    /// Creates a <see cref="Result{T, TErr}"/> in the <c>Err</c> state,
    /// containing the given error value.
    /// </summary>
    /// <typeparam name="T">The type of the value the result would contain if it were not in the <c>Err</c> state.</typeparam>
    /// <typeparam name="TErr">The type of the error the result contains.</typeparam>
    /// <param name="error">The error value to store in the result.</param>
    /// <returns>A result object containing the given error value.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the error value is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Err<T, TErr>(TErr error)
        where T : notnull
        where TErr : notnull
    {
        return new Result<T, TErr>(error);
    }

    /// <summary>
    /// Creates a <see cref="Result{T, TErr}"/> in the <c>Err</c> state,
    /// containing the given error message.
    /// </summary>
    /// <typeparam name="T">The type of the value the result would contain if it were not in the <c>Err</c> state.</typeparam>
    /// <param name="errorMessage">The error message to store in the result.</param>
    /// <returns>A result object containing the given error message.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the error message is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, string> Err<T>(string errorMessage)
        where T : notnull
    {
        return new Result<T, string>(errorMessage);
    }

    /// <summary>
    /// Creates a <see cref="Result{T, TErr}"/> in the <c>Err</c> state,
    /// containing the given exception.
    /// </summary>
    /// <typeparam name="T">The type of the value the result would contain if it were not in the <c>Err</c> state.</typeparam>
    /// <param name="resultError">The struct to store in the result.</param>
    /// <returns>A result object containing the given error.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, ResultError> Err<T>(ResultError resultError)
        where T : notnull
    {
        return new Result<T, ResultError>(resultError);
    }

    /// <summary>
    /// Creates a <see cref="Result{T, TErr}"/> in the <c>Err</c> state,
    /// containing the given exception.
    /// </summary>
    /// <typeparam name="T">The type of the value the result would contain if it were not in the <c>Err</c> state.</typeparam>
    /// <param name="ex">The exception to store in the result.</param>
    /// <returns>A result object containing the given exception.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the exception is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, ResultError> Err<T>(Exception ex)
        where T : notnull
    {
        return new Result<T, ResultError>(ex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, ResultError> Try<T>(TryFunc<T> func)
        where T : notnull
    {
        try
        {
            return Ok<T, ResultError>(func());
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
    public static async Task<Result<T, ResultError>> TryAsync<T>(AsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok<T, ResultError>(value);
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
    public static async Task<Result<T, ResultError>> TryAsync<T>(ValueAsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok<T, ResultError>(value);
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
    public static async Task<Result<T, ResultError>> TryAsync<T>(AsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok<T, ResultError>(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

    /// <summary>
    /// Overload for functions returning ValueTask.
    /// </summary>
    public static async Task<Result<T, ResultError>> TryAsync<T>(ValueAsyncFunc<T> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok<T, ResultError>(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

#endif

    /// <summary>
    /// Executes the specified function and returns a <see cref="Result{T, ResultError}"/>. 
    /// If the function throws, it captures the exception as an error.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A successful or failed result depending on whether the function throws.</returns>
    [DebuggerStepThrough]
    public static Result<T, ResultError> From<T>(Func<T> func) where T : notnull
    {
        try
        {
            return Ok<T, ResultError>(func());
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

    /// <summary>
    /// Converts an <see cref="Option{T}"/> to a <see cref="Result{T, ResultError}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>
    /// A successful <see cref="Result{T, ResultError}"/> if the option is <c>Some</c>;
    /// otherwise, an error <see cref="Result{T, ResultError}"/> with a default error message.
    /// </returns>
    [DebuggerStepThrough]
    public static Result<T, ResultError> From<T>(Option<T> option) where T : notnull
    {
        return option.IsSome
            ? Ok<T, ResultError>(option.Unwrap())
            : Err<T>(new NoneValueException());
    }

    /// <summary>
    /// Executes the specified action and returns a <see cref="Result{Unit, ResultError}"/>. 
    /// If the action throws, it captures the exception as an error.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A successful or failed result depending on whether the action throws.</returns>
    [DebuggerStepThrough]
    public static Result<Unit, ResultError> From(Action action)
    {
        try
        {
            action();
            return Ok<Unit, ResultError>(Unit.Default);
        }
        catch (Exception ex)
        {
            return Err<Unit>(ex);
        }
    }

    /// <summary>
    /// Executes the specified function and returns a <see cref="Result{T, TErr}"/>.
    /// If an exception is thrown, the provided <paramref name="errorConverter"/> maps it to an error value.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <typeparam name="TErr">The type of the error to return on exception.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorConverter">A function that converts an exception to a <typeparamref name="TErr"/>.</param>
    /// <returns>A successful result or a failed result with a converted error.</returns>
    [DebuggerStepThrough]
    public static Result<T, TErr> From<T, TErr>(
        Func<T> func,
        Func<Exception, TErr> errorConverter
    )
        where T : notnull
        where TErr : notnull
    {
        try
        {
            return Ok<T, TErr>(func());
        }
        catch (Exception ex)
        {
            return Err<T, TErr>(errorConverter(ex));
        }
    }

    // ===============
    public static async Task<Result<T, ResultError>> TryAsync<T, TParam>(
        TParam parameter,
        AsyncFunc<T, TParam> asyncFunc)
        where T : notnull
    {
        try
        {
            var value = await asyncFunc(parameter).ConfigureAwait(false);
            return Result.Ok<T, ResultError>(value);
        }
        catch (Exception ex)
        {
            return Result.Err<T>(new ResultError(ex.Message, ex));
        }
    }

    public static async Task<Result<T, ResultError>> TryAsync<T, TParam>(
        TParam parameter,
        ValueAsyncFunc<T, TParam> asyncFunc)
        where T : notnull
    {
        try
        {
            var value = await asyncFunc(parameter).ConfigureAwait(false);
            return Result.Ok<T, ResultError>(value);
        }
        catch (Exception ex)
        {
            return Result.Err<T>(new ResultError(ex.Message, ex));
        }
    }
}