using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpResults.Types;

/// <summary>
/// A ref struct enumerator for ResultValueEnumerable.
/// Stack-only, no heap allocation.
/// </summary>
public ref struct ResultValueEnumerator<T, TErr>
    where T : notnull
    where TErr : notnull
{
    private readonly Result<T, TErr> _single;
    private readonly IReadOnlyList<T>? _collection;
    private int _index; // For collection
    private bool _yieldedSingle; // For single value

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultValueEnumerator(Result<T, TErr> single, IReadOnlyList<T>? collection)
    {
        _single = single;
        _collection = collection;
        _index = 0;
        _yieldedSingle = false;
    }

    /// <summary>
    /// High-performance method to get the next value. Combines MoveNext and Current.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetNext(out T current)
    {
        if (_collection != null)
        {
            if (_index < _collection.Count)
            {
                current = _collection[_index];
                _index++;
                return true;
            }
        }
        else if (!_yieldedSingle && _single.IsOk)
        {
            current = _single.Unwrap();
            _yieldedSingle = true;
            return true;
        }

        current = default!;
        return false;
    }

    /// <summary>
    /// Standard foreach support.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() => TryGetNext(out _);

    /// <summary>
    /// Standard foreach support.
    /// </summary>
    public T Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_collection != null)
            {
                // _index is already advanced by TryGetNext/MoveNext
                return _collection[_index - 1];
            }

            if (!_single.IsOk || !_yieldedSingle)
                throw new InvalidOperationException("Enumerator is not at a valid position.");
            
            return _single.Unwrap();
        }
    }

    /// <summary>
    /// Tries to get a ReadOnlySpan for a single-value Result. Zero allocation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetSpan(out ReadOnlySpan<T> span)
    {
        // Only works for single, successful results that haven't been yielded yet
        if (_collection == null && !_yieldedSingle && _single.IsOk)
        {
            var value = _single.Unwrap();
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in value), 1);
            _yieldedSingle = true; // Mark as yielded to prevent re-use
            return true;
        }

        span = ReadOnlySpan<T>.Empty;
        return false;
    }
}
