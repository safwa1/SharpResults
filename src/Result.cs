using System.Diagnostics;
using SharpResults.Types;

namespace SharpResults;

/// <summary>
/// Provides static helper methods to construct <see cref="Result{T}"/> and <see cref="Result{T, TError}"/> values.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The result value.</param>
    /// <returns>A successful result containing the value.</returns>
    [DebuggerStepThrough]
    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with the specified exception.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="ex">The exception representing the failure.</param>
    /// <returns>A failed result containing the exception.</returns>
    [DebuggerStepThrough]
    public static Result<T> Err<T>(Exception ex) => Result<T>.Err(ex);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with a message wrapped in an exception.
    /// </summary>
    /// <typeparam name="T">The expected result type.</typeparam>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result containing the message as an exception.</returns>
    [DebuggerStepThrough]
    public static Result<T> Err<T>(string error) => Result<T>.Err(error);

    /// <summary>
    /// Executes the specified function and returns a <see cref="Result{T}"/>. 
    /// If the function throws, captures the exception as an error.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A successful or failed result depending on whether the function throws.</returns>
    [DebuggerStepThrough]
    public static Result<T> From<T>(Func<T> func)
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
    /// Executes the specified action and returns a <see cref="Result{Unit}"/>. 
    /// If the action throws, captures the exception as an error.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A successful or failed result depending on whether the action throws.</returns>
    [DebuggerStepThrough]
    public static Result<Unit> From(Action action)
    {
        try
        {
            action();
            return Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            return Err<Unit>(ex);
        }
    }

    /// <summary>
    /// Creates a successful <see cref="Result{T, TError}"/> with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="value">The successful result value.</param>
    /// <returns>A successful result containing the value.</returns>
    [DebuggerStepThrough]
    public static Result<T, TError> Ok<T, TError>(T value) => Result<T, TError>.Ok(value);

    /// <summary>
    /// Creates a failed <see cref="Result{T, TError}"/> with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the expected value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="error">The error value.</param>
    /// <returns>A failed result containing the error.</returns>
    [DebuggerStepThrough]
    public static Result<T, TError> Err<T, TError>(TError error) => Result<T, TError>.Err(error);

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
    public static Result<T, TError> From<T, TError>(Func<T> func, Func<Exception, TError> errorConverter)
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
