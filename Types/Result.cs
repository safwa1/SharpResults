using System.Diagnostics;

namespace SharpResults.Types;

/// <summary>
/// Represents the result of an operation that may succeed with a value of type <typeparamref name="T"/> or fail with an <see cref="Exception"/>.
/// </summary>
/// <typeparam name="T">The type of the successful result value.</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Exception? _exception;

    /// <summary>
    /// Gets the value if the result is successful; throws if it is an error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing <see cref="Value"/> on a failed result.</exception>
    public T Value => IsOk 
        ? _value! 
        : throw new InvalidOperationException("Cannot access Value of a failed result", _exception);

    /// <summary>
    /// Gets the exception if the result is an error; throws if it is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing <see cref="Exception"/> on a successful result.</exception>
    public Exception Exception => !IsOk 
        ? _exception! 
        : throw new InvalidOperationException("Cannot access Exception of a successful result");

    /// <summary>
    /// Indicates whether the result is successful.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// Indicates whether the result is an error.
    /// </summary>
    public bool IsErr => !IsOk;

    private Result(T? value, Exception? exception, bool isOk)
    {
        _value = value;
        _exception = exception;
        IsOk = isOk;
    }

    /// <summary>
    /// Creates a successful result containing the given value.
    /// </summary>
    /// <param name="value">The successful value.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    [DebuggerStepThrough]
    public static Result<T> Ok(T value) => new(value, null, true);

    /// <summary>
    /// Creates a failed result with the specified exception.
    /// </summary>
    /// <param name="ex">The exception representing the error.</param>
    /// <returns>An error <see cref="Result{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
    [DebuggerStepThrough]
    public static Result<T> Err(Exception ex) => new(default, ex ?? throw new ArgumentNullException(nameof(ex)), false);

    /// <summary>
    /// Creates a failed result with an exception constructed from the given error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>An error <see cref="Result{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="error"/> is null.</exception>
    [DebuggerStepThrough]
    public static Result<T> Err(string error) => new(default, new Exception(error ?? throw new ArgumentNullException(nameof(error))), false);

    /// <summary>
    /// Returns a string representation of the result.
    /// </summary>
    /// <returns>A string indicating success or failure.</returns>
    public override string ToString() => IsOk 
        ? $"Success({_value})" 
        : $"Failure({_exception?.Message})";

    /// <summary>
    /// Deconstructs the result into its components.
    /// </summary>
    /// <param name="isOk">Whether the result is successful.</param>
    /// <param name="value">The value (may be null if an error).</param>
    /// <param name="exception">The exception (may be null if successful).</param>
    public void Deconstruct(out bool isOk, out T? value, out Exception? exception)
    {
        isOk = IsOk;
        value = _value;
        exception = _exception;
    }

    /// <summary>
    /// Converts a value of type <typeparamref name="T"/> into a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<T>(T value) => Ok(value);
}


/// <summary>
/// Represents the result of an operation that may succeed with a value of type <typeparamref name="T"/> 
/// or fail with an error of type <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="T">The type of the successful result value.</typeparam>
/// <typeparam name="TError">The type of the error.</typeparam>
public readonly struct Result<T, TError>
{
    private readonly T? _value;
    private readonly TError? _error;

    /// <summary>
    /// Gets the value if the result is successful; throws if it is an error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing <see cref="Value"/> on a failed result.</exception>
    public T Value => IsOk
        ? _value!
        : throw new InvalidOperationException("Cannot access Value of a failed result");

    /// <summary>
    /// Gets the error if the result is an error; throws if it is successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing <see cref="Error"/> on a successful result.</exception>
    public TError Error => IsErr
        ? _error!
        : throw new InvalidOperationException("Cannot access Error of a successful result");

    /// <summary>
    /// Indicates whether the result is successful.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    /// Indicates whether the result is an error.
    /// </summary>
    public bool IsErr => !IsOk;

    private Result(T? value, TError? error, bool isOk)
    {
        _value = value;
        _error = error;
        IsOk = isOk;
    }

    /// <summary>
    /// Creates a successful result containing the given value.
    /// </summary>
    /// <param name="value">The successful value.</param>
    /// <returns>A successful <see cref="Result{T, TError}"/>.</returns>
    [DebuggerStepThrough]
    public static Result<T, TError> Ok(T value) => new(value, default, true);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>An error <see cref="Result{T, TError}"/>.</returns>
    [DebuggerStepThrough]
    public static Result<T, TError> Err(TError error) => new(default, error, false);

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<T, TError>(T value) => Ok(value);

    /// <summary>
    /// Implicitly converts an error to a failed result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result<T, TError>(TError error) => Err(error);

    /// <summary>
    /// Returns a string representation of the result.
    /// </summary>
    /// <returns>A string indicating success or failure.</returns>
    public override string ToString() => IsOk
        ? $"Ok({_value})"
        : $"Err({_error})";

    /// <summary>
    /// Deconstructs the result into its components.
    /// </summary>
    /// <param name="isOk">Whether the result is successful.</param>
    /// <param name="value">The value (may be null if an error).</param>
    /// <param name="error">The error (may be null if successful).</param>
    public void Deconstruct(out bool isOk, out T? value, out TError? error)
    {
        isOk = IsOk;
        value = _value;
        error = _error;
    }
}