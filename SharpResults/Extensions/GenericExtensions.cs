using System.Runtime.CompilerServices;
using SharpResults.Types;

namespace SharpResults.Extensions;

public static class GenericExtensions
{
    /// <summary>
    /// Wraps a value in a successful <see cref="Result{T, TErr}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Ok<T, TErr>(this T self)
        where T : notnull
        where TErr : notnull
        => Result<T, TErr>.Ok(self);

    /// <summary>
    /// Wraps an error value in a failed <see cref="Result{T, TErr}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> Err<T, TErr>(this TErr error)
        where T : notnull
        where TErr : notnull
        => Result<T, TErr>.Err(error);
}