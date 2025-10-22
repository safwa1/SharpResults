using System.Runtime.CompilerServices;
using SharpResults.Simple.Types;

namespace SharpResults.Simple.Awaitables;

public readonly struct ResultAwaiterSimple<T> : ICriticalNotifyCompletion
    where T : notnull
{
    private readonly Result<T> _result;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultAwaiterSimple(Result<T> result) => _result = result;

    public bool IsCompleted => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult() => _result.Unwrap();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) => continuation?.Invoke();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action continuation) => continuation?.Invoke();
}