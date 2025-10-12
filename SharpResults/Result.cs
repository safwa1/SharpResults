using System.Diagnostics;
using System.Runtime.CompilerServices;
using SharpResults.Exceptions;
using SharpResults.Types;

namespace SharpResults;

/// <summary>
/// This class contains static methods for creating a <see cref="Result{T, TErr}"/>.
/// </summary>
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
    /// <param name="errMsg">The error message to store in the result.</param>
    /// <returns>A result object containing the given error message.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if the error message is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, string> Err<T>(string errMsg)
        where T : notnull
    {
        return new Result<T, string>(errMsg);
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
    public static Result<T, Exception> Err<T>(Exception ex)
        where T : notnull
    {
        return new Result<T, Exception>(ex);
    }

    /// <summary>
    /// Attempts to call <paramref name="func"/>, wrapping the returned value in an <c>Ok</c> result.
    /// Any exceptions will be caught and returned in an <c>Err</c> result.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the given function.</typeparam>
    /// <param name="func">The function to attempt to call.</param>
    /// <returns>The return value of <paramref name="func"/> wrapped in <c>Ok</c>, or <c>Err</c> containing any exception that was thrown.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, Exception> Try<T>(Func<T> func)
        where T : notnull
    {
        try
        {
            return Ok<T, Exception>(func());
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }
    
    /// <summary>
    /// Attempts to call an asynchronous <paramref name="func"/>, wrapping the returned value in an <c>Ok</c> result.
    /// Any exceptions will be caught and returned in an <c>Err</c> result.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the given function.</typeparam>
    /// <param name="func">The asynchronous function to attempt to call.</param>
    /// <returns>The return value of <paramref name="func"/> wrapped in <c>Ok</c>, or <c>Err</c> containing any exception that was thrown.</returns>
    public static async Task<Result<T, Exception>> TryAsync<T>(Func<Task<T>> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok<T, Exception>(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

    /// <summary>
    /// Overload for functions returning ValueTask.
    /// </summary>
    public static async Task<Result<T, Exception>> TryAsync<T>(Func<ValueTask<T>> func)
        where T : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Ok<T, Exception>(value);
        }
        catch (Exception ex)
        {
            return Err<T>(ex);
        }
    }

    /// <summary>
    /// Executes the specified function and returns a <see cref="Result{T, Exception}"/>. 
    /// If the function throws, it captures the exception as an error.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A successful or failed result depending on whether the function throws.</returns>
    [DebuggerStepThrough]
    public static Result<T, Exception> From<T>(Func<T> func) where T : notnull
    {
        try
        {
            return Ok<T, Exception>(func());
        }
        catch (Exception ex)
        {
            return Err<T, Exception>(ex);
        }
    }

    /// <summary>
    /// Converts an <see cref="Option{T}"/> to a <see cref="Result{T, Exception}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the option.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>
    /// A successful <see cref="Result{T, Exception}"/> if the option is <c>Some</c>;
    /// otherwise, an error <see cref="Result{T, Exception}"/> with a default error message.
    /// </returns>
    [DebuggerStepThrough]
    public static Result<T, Exception> From<T>(Option<T> option) where T : notnull
    {
        return option.IsSome
            ? Ok<T, Exception>(option.Unwrap())
            : Err<T>(new NoneValueException());
    }

    /// <summary>
    /// Executes the specified action and returns a <see cref="Result{Unit, Exception}"/>. 
    /// If the action throws, it captures the exception as an error.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A successful or failed result depending on whether the action throws.</returns>
    [DebuggerStepThrough]
    public static Result<Unit, Exception> From(Action action)
    {
        try
        {
            action();
            return Ok<Unit, Exception>(Unit.Default);
        }
        catch (Exception ex)
        {
            return Err<Unit>(ex);
        }
    }
    
    /// <summary>
    /// Executes the specified function and returns a <see cref="Result{T, TError}"/>.
    /// If an exception is thrown, the provided <paramref name="errorConverter"/> maps it to an error value.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <typeparam name="TError">The type of the error to return on exception.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorConverter">A function that converts an exception to a <typeparamref name="TError"/>.</param>
    /// <returns>A successful result or a failed result with a converted error.</returns>
    [DebuggerStepThrough]
    public static Result<T, TError> From<T, TError>(
        Func<T> func,
        Func<Exception, TError> errorConverter
    )
        where T : notnull
        where TError : notnull
    {
        try
        {
            return Ok<T, TError>(func());
        }
        catch (Exception ex)
        {
            return Err<T, TError>(errorConverter(ex));
        }
    }
}