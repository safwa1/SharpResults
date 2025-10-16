using System.Runtime.CompilerServices;

namespace SharpResults.Types;

/// <summary>
/// A ref struct enumerable for Result values, supporting both single values and collections.
/// Stack-only, no heap allocation.
/// </summary>
public readonly ref struct ResultValueEnumerable<T, TErr>
    where T : notnull
    where TErr : notnull
{
    private readonly Result<T, TErr> _single;
    private readonly IReadOnlyList<T>? _collection;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultValueEnumerable(Result<T, TErr> result)
    {
        _single = result;
        _collection = null;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultValueEnumerable(Result<IReadOnlyList<T>, TErr> result)
    {
        // Unwrap once to avoid checking IsOk inside the enumerator loop
        _collection = result.IsOk ? result.Unwrap() : null;
        _single = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultValueEnumerator<T, TErr> GetEnumerator() 
        => new(_single, _collection);
}
