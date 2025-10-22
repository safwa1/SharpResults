using System.Runtime.CompilerServices;
using SharpResults.Simple.Types;

namespace SharpResults.Simple.Awaitables;

public static class SimpleResultAwaitableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultAwaiterSimple<T> GetAwaiter<T>(this Result<T> result)
        where T : notnull
        => new(result);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TaskResultAwaiterSimple<T> GetAwaiter<T>(this Task<Result<T>> task)
        where T : notnull
        => new(task);
}