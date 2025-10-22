using System.Runtime.CompilerServices;
using SharpResults.Types;

namespace SharpResults.Awaitables;

/// <summary>
/// Enables <c>await</c> syntax for <see cref="Result{T, TErr}"/> />.
/// </summary>
public static class ResultAwaitableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultAwaiter<T, TErr> GetAwaiter<T, TErr>(this Result<T, TErr> result)
        where T : notnull
        where TErr : notnull
        => new(result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TaskResultAwaiter<T, TErr> GetAwaiter<T, TErr>(this Task<Result<T, TErr>> task)
        where T : notnull
        where TErr : notnull
        => new(task);
}