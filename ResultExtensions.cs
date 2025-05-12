using System.Diagnostics;
using SharpResults.Types;

namespace SharpResults;

/// <summary>
/// Provides extension methods for <see cref="Result{T}"/> and <see cref="Result{T, TError}"/> types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">The action to execute if the result is successful.</param>
    public static Result<T> Inspect<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsOk)
        {
            action(result.Value);
        }
        return result;
    }

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    /// <param name="result">The result to inspect.</param>
    /// <param name="action">The action to execute if the result is a failure.</param>
    public static Result<T> InspectErr<T>(this Result<T> result, Action<Exception> action)
    {
        if (result.IsErr)
        {
            action(result.Exception);
        }
        return result;
    }

    /// <summary>
    /// Transforms the result value if successful.
    /// </summary>
    /// <param name="result">The result to transform.</param>
    /// <param name="mapper">The function to apply to the value.</param>
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
    {
        return result.IsOk 
            ? Result.Ok(mapper(result.Value)) 
            : Result.Err<U>(result.Exception);
    }

    /// <summary>
    /// Chains the result to another result-returning function.
    /// </summary>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">The function to apply if the result is successful.</param>
    public static Result<U> AndThen<T, U>(this Result<T> result, Func<T, Result<U>> binder)
    {
        return result.IsOk 
            ? binder(result.Value) 
            : Result.Err<U>(result.Exception);
    }

    /// <summary>
    /// Recovers from a failure with another result.
    /// </summary>
    /// <param name="result">The result to recover from.</param>
    /// <param name="op">The recovery function to apply if the result is a failure.</param>
    public static Result<T> OrElse<T>(this Result<T> result, Func<Exception, Result<T>> op)
    {
        return result.IsOk 
            ? result 
            : op(result.Exception);
    }

    /// <summary>
    /// Transforms the result value or returns a default value.
    /// </summary>
    /// <param name="result">The result to transform.</param>
    /// <param name="defaultValue">The value to return if the result is a failure.</param>
    /// <param name="mapper">The function to apply if the result is successful.</param>
    public static U MapOr<T, U>(this Result<T> result, U defaultValue, Func<T, U> mapper)
    {
        return result.IsOk 
            ? mapper(result.Value) 
            : defaultValue;
    }

    /// <summary>
    /// Transforms the result value or uses a fallback function for the error.
    /// </summary>
    /// <param name="result">The result to transform.</param>
    /// <param name="fallback">The fallback function for the error.</param>
    /// <param name="mapper">The function to apply if the result is successful.</param>
    public static U MapOrElse<T, U>(this Result<T> result, Func<Exception, U> fallback, Func<T, U> mapper)
    {
        return result.IsOk 
            ? mapper(result.Value) 
            : fallback(result.Exception);
    }

    /// <summary>
    /// Returns the value or a provided default.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    /// <param name="defaultValue">The value to return if the result is a failure.</param>
    public static T UnwrapOr<T>(this Result<T> result, T defaultValue)
    {
        return result.IsOk 
            ? result.Value 
            : defaultValue;
    }

    /// <summary>
    /// Returns the value or uses a fallback function.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    /// <param name="fallback">The fallback function to apply if the result is a failure.</param>
    public static T UnwrapOrElse<T>(this Result<T> result, Func<Exception, T> fallback)
    {
        return result.IsOk 
            ? result.Value 
            : fallback(result.Exception);
    }

    /// <summary>
    /// Returns the value if successful; throws with a custom message otherwise.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    /// <param name="message">The message to include in the thrown exception.</param>
    public static T Expect<T>(this Result<T> result, string message)
    {
        if (result.IsErr)
        {
            throw new InvalidOperationException(message, result.Exception);
        }
        return result.Value;
    }

    /// <summary>
    /// Returns the value or throws if the result is a failure.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    public static T Unwrap<T>(this Result<T> result)
    {
        if (result.IsErr)
        {
            throw new InvalidOperationException("Called Unwrap on a failed result", result.Exception);
        }
        return result.Value;
    }

    /// <summary>
    /// Returns the value or the default for the type if it's a failure.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    public static T UnwrapOrDefault<T>(this Result<T> result) where T : struct
    {
        return result.IsOk 
            ? result.Value 
            : default;
    }

    /// <summary>
    /// Alias for <see cref="AndThen{T, U}(Result{T}, Func{T, Result{U}})"/>.
    /// </summary>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">The function to apply if the result is successful.</param>
    public static Result<U> FlatMap<T, U>(this Result<T> result, Func<T, Result<U>> binder) 
        => AndThen(result, binder);

    /// <summary>
    /// Checks if the result contains the specified value.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="value">The value to compare against the result.</param>
    public static bool Contains<T>(this Result<T> result, T value)
    {
        return result.IsOk && EqualityComparer<T>.Default.Equals(result.Value, value);
    }

    /// <summary>
    /// Checks if the result contains the specified exception.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="exception">The exception to compare against the result.</param>
    public static bool ContainsErr<T>(this Result<T> result, Exception exception)
    {
        return result.IsErr && result.Exception == exception;
    }

    /// <summary>
    /// Transforms the success value of a <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <param name="result">The result to transform.</param>
    /// <param name="mapper">The function to apply to the value.</param>
    public static Result<U, E> Map<T, U, E>(this Result<T, E> result, Func<T, U> mapper)
    {
        return result.IsOk 
            ? Result.Ok<U, E>(mapper(result.Value)) 
            : Result.Err<U, E>(result.Error);
    }

    /// <summary>
    /// Chains a <see cref="Result{T, TError}"/> to another result-returning function.
    /// </summary>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">The function to apply if the result is successful.</param>
    public static Result<U, E> AndThen<T, U, E>(this Result<T, E> result, Func<T, Result<U, E>> binder)
    {
        return result.IsOk 
            ? binder(result.Value) 
            : Result.Err<U, E>(result.Error);
    }

    /// <summary>
    /// Returns the value or a provided default for <see cref="Result{T, TError}"/>.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    /// <param name="defaultValue">The value to return if the result is a failure.</param>
    public static T UnwrapOr<T, E>(this Result<T, E> result, T defaultValue)
    {
        return result.IsOk 
            ? result.Value 
            : defaultValue;
    }

    /// <summary>
    /// Matches over success or error values and applies appropriate function.
    /// </summary>
    /// <param name="result">The result to match on.</param>
    /// <param name="ok">The function to apply if the result is successful.</param>
    /// <param name="err">The function to apply if the result is a failure.</param>
    public static TResult Match<T, TResult>(this Result<T> result, Func<T, TResult> ok, Func<Exception, TResult> err)
    {
        return result.IsOk ? ok(result.Value) : err(result.Exception);
    }

    /// <summary>
    /// Matches over success or error values and applies appropriate function.
    /// </summary>
    /// <param name="result">The result to match on.</param>
    /// <param name="ok">The function to apply if the result is successful.</param>
    /// <param name="err">The function to apply if the result is a failure.</param>
    public static TResult Match<T, E, TResult>(this Result<T, E> result, Func<T, TResult> ok, Func<E, TResult> err)
    {
        return result.IsOk ? ok(result.Value) : err(result.Error);
    }

    /// <summary>
    /// LINQ-compatible select function for result values.
    /// </summary>
    /// <param name="result">The result to transform.</param>
    /// <param name="selector">The transformation function.</param>
    public static Result<U> Select<T, U>(this Result<T> result, Func<T, U> selector) => result.Map(selector);

    /// <summary>
    /// LINQ-compatible select many function for chaining result operations.
    /// </summary>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">The function to produce the next result.</param>
    /// <param name="projector">The function to combine the results.</param>
    public static Result<V> SelectMany<T, U, V>(this Result<T> result, Func<T, Result<U>> binder, Func<T, U, V> projector)
    {
        return result.AndThen(t =>
            binder(t).Map(u => projector(t, u))
        );
        
    }
}