using System.Diagnostics;
using SharpResults.Types;

namespace SharpResults;

/// <summary>
/// Provides factory methods and support for working with <see cref="Option{T}"/> values.
/// </summary>
public static class Option
{

    public static class Defaults
    {
        /// <summary>
        /// Singleton instance representing the None value. Use for convenience when assigning to <see cref="Option{T}"/>.
        /// </summary>
        public static readonly None None = default;
    }

    /// <summary>
    /// Creates an <see cref="Option{T}"/> that contains the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap in an Option. Must not be null.</param>
    /// <returns>An <see cref="Option{T}"/> in the same state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static Option<T> Some<T>(T value) => Option<T>.Some(value);

    /// <summary>
    /// Creates an <see cref="Option{T}"/> that represents no value.
    /// </summary>
    /// <typeparam name="T">The type of the option.</typeparam>
    /// <returns>An <see cref="Option{T}"/> in the None state.</returns>
    public static Option<T> None<T>() => Option<T>.None();
    
    /// <summary>
    /// Converts a <see cref="Result{T}"/> into an <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>
    /// <see cref="Option.Some{T}(T)"/> if the result is <c>Ok</c>;
    /// otherwise, <see cref="Option.None{T}"/>.
    /// </returns>
    [DebuggerStepThrough]
    public static Option<T> From<T>(Result<T> result)
    {
        return result.IsOk
            ? Some(result.Value)
            : None<T>();
    }
    
    
    /// <summary>
    /// Executes the specified function and converts the result into an <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>
    /// <see cref="Option.Some{T}(T)"/> if the function executes without throwing an exception;
    /// otherwise, <see cref="Option.None{T}"/>.
    /// </returns>
    /// <remarks>
    /// This method is useful for wrapping exception-prone logic in a safe optional context.
    /// Internally, it uses <see cref="Result.From(Func{T})"/> to handle exceptions.
    /// </remarks>
    [DebuggerStepThrough]
    public static Option<T> From<T>(Func<T> func)
    {
        var result = Result.From(func);
        return From(result);
    }
}
