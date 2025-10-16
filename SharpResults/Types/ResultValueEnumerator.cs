using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpResults.Types;

/// <summary>
/// A stack-only enumerator for iterating over <see cref="Result{T,TErr}"/> values
/// in a zero-allocation manner.  
/// </summary>
/// <typeparam name="T">The type of the successful value contained in the <see cref="Result{T,TErr}"/>.</typeparam>
/// <typeparam name="TErr">The type of the error contained in the <see cref="Result{T,TErr}"/>.</typeparam>
/// <remarks>
/// This struct supports enumeration over both:
/// <list type="bullet">
/// <item><description>A single successful <see cref="Result{T,TErr}"/> value.</description></item>
/// <item><description>A <see cref="Result{IReadOnlyList{T},TErr}"/> representing a collection of values.</description></item>
/// </list>
/// It is designed for high-performance, stack-only iteration without heap allocation.
/// </remarks>
public ref struct ResultValueEnumerator<T, TErr>
    where T : notnull
    where TErr : notnull
{
    private readonly Result<T, TErr> _single;
    private readonly IReadOnlyList<T>? _collection;
    private int _index;
    private bool _yieldedSingle;

    /// <summary>
    /// Initializes the enumerator with either a single result or a collection result.
    /// </summary>
    /// <param name="single">The single <see cref="Result{T,TErr}"/> value (if applicable).</param>
    /// <param name="collection">The collection of <typeparamref name="T"/> values (if applicable).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultValueEnumerator(Result<T, TErr> single, IReadOnlyList<T>? collection)
    {
        _single = single;
        _collection = collection;
        _index = 0;
        _yieldedSingle = false;
    }

    /// <summary>
    /// Efficiently retrieves the next value in the sequence, combining <c>MoveNext</c> and <c>Current</c>
    /// in a single operation to reduce overhead.
    /// </summary>
    /// <param name="current">The next available value, if any.</param>
    /// <returns><see langword="true"/> if a value was retrieved; otherwise, <see langword="false"/>.</returns>
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
    /// Moves to the next element in the sequence.
    /// </summary>
    /// <returns><see langword="true"/> if the enumerator successfully advanced; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() => TryGetNext(out _);

    /// <summary>
    /// Gets the current element in the sequence.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the enumerator is not in a valid position.</exception>
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
    /// Attempts to retrieve the value as a <see cref="ReadOnlySpan{T}"/> for zero-allocation access.
    /// </summary>
    /// <param name="span">A span containing the single successful value, if available.</param>
    /// <returns>
    /// <see langword="true"/> if a valid span was created for a single successful result;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method is useful when you want to avoid allocation and directly access the value
    /// as a span when dealing with a single <see cref="Result{T,TErr}"/> instance.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetSpan(out ReadOnlySpan<T> span)
    {
        if (_collection == null && !_yieldedSingle && _single.IsOk)
        {
            var value = _single.Unwrap();
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in value), 1);
            _yieldedSingle = true;
            return true;
        }

        span = ReadOnlySpan<T>.Empty;
        return false;
    }
}
