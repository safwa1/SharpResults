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
}
