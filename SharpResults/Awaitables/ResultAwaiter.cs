using System.Runtime.CompilerServices;
using SharpResults.Types;

namespace SharpResults.Awaitables;

public readonly struct ResultAwaiter<T, TErr> : ICriticalNotifyCompletion
    where T : notnull
    where TErr : notnull
{
    private readonly Result<T, TErr> _result;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultAwaiter(Result<T, TErr> result) => _result = result;

    public bool IsCompleted => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResult() => _result.Unwrap();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnCompleted(Action continuation) => continuation?.Invoke();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeOnCompleted(Action continuation) => continuation?.Invoke();
}