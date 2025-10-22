using System.Runtime.CompilerServices;
using SharpResults.Simple.Types;

namespace SharpResults.Simple.Awaitables;

public readonly struct TaskResultAwaiterSimple<T> : ICriticalNotifyCompletion
    where T : notnull
{
    private readonly Task<Result<T>> _task;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TaskResultAwaiterSimple(Task<Result<T>> task) => _task = task;

    public bool IsCompleted => _task.IsCompleted;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult() => _task.GetAwaiter().GetResult().Unwrap();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) => _task.GetAwaiter().OnCompleted(continuation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action continuation) => _task.GetAwaiter().UnsafeOnCompleted(continuation);
}