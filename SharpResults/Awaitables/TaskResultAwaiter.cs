using System.Runtime.CompilerServices;
using SharpResults.Types;

namespace SharpResults.Awaitables;

public readonly struct TaskResultAwaiter<T, TErr> : ICriticalNotifyCompletion
    where T : notnull
    where TErr : notnull
{
    private readonly Task<Result<T, TErr>> _task;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TaskResultAwaiter(Task<Result<T, TErr>> task) => _task = task;

    public bool IsCompleted => _task.IsCompleted;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult() => _task.GetAwaiter().GetResult().Unwrap();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) => _task.GetAwaiter().OnCompleted(continuation);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action continuation) => _task.GetAwaiter().UnsafeOnCompleted(continuation);
}