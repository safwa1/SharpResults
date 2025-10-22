// Added to ensure ResultError is in scope
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using SharpResults.Converters;
using SharpResults.Core.Types;
using SharpResults.Exceptions;
using SharpResults.Extensions;
using SharpResults.Simple.Core;
using static System.ArgumentNullException;

namespace SharpResults.Simple.Types;

/// <summary>
/// <see cref="Result{T}"/> is used to return the result of an operation that might fail, without
/// throwing an exception. Either <see cref="WhenOk"/> will return <c>true</c> and the contained result value,
/// or else <see cref="WhenErr"/> will return <c>true</c> and the contained error value.
/// </summary>
/// <typeparam name="T">The type of the return value.</typeparam>
[Serializable]
[JsonConverter(typeof(ResultJsonConverter))]
[StructLayout(LayoutKind.Auto)]
public readonly struct Result<T> : IEquatable<Result<T>>, IComparable<Result<T>>, IFormattable, ISpanFormattable
#if NET8_0_OR_GREATER
    , IUtf8SpanFormattable
#endif
    where T : notnull 
{
    /// <summary>
    /// Initializes a <see cref="Result{T}"/> in the <c>Ok</c> state containing the given value.
    /// </summary>
    /// <param name="value">The value to contain.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if the given value is null.</exception>
    public Result(T value)
    {
        ThrowIfNull(value);
        _value = value;
        _err = default!;
        _isOk = true;
    }

    /// <summary>
    /// Initializes a <see cref="Result{T}"/> in the <c>Err</c> state containing the given error value.
    /// </summary>
    /// <param name="error">The error value to store.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if the given error is null.</exception>
    public Result(ResultError? error)
    {
        ThrowIfNull(error);
        _err = error.Value;
        _value = default!;
        _isOk = false;
    }
    
    /// <summary>
    /// Default state is equivalent to <c>Err(default)</c>.
    /// Use <see cref="SharpResults.Core.Result.Ok{T}(T)"/> or <see cref="SharpResults.Core.Result.Err{T}(ResultError)"/> to create valid instances.
    /// </summary>
    public static Result<T> Default => default;

    private readonly bool _isOk;
    private readonly T _value;
    private readonly ResultError _err;

    /// <summary>
    /// Returns <c>true</c> if the result is in the <c>Ok</c> state, and <paramref name="value"/> will contain the return value.
    /// </summary>
    /// <param name="value">The returned value, if any.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool WhenOk([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return _isOk;
    }
    
    public bool IsOk => _isOk;
    
    public bool IsErr => !_isOk;

    /// <summary>
    /// Returnd <c>true</c> if the result is in the <c>Err</c> state, and <paramref name="error"/> will contain the error value.
    /// <para>
    /// NOTE: While in most cases <paramref name="error"/> will be non-null when this method returns <c>true</c>,
    /// a default instance of this struct will be in the <c>Err</c> state, but the err value will be
    /// the default value for <see cref="ResultError"/>, and therefore possibly null. 
    /// </para>
    /// </summary>
    /// <param name="error">The returned error value, if any.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool WhenErr([MaybeNull] out ResultError error)
    {
        // A default instance of this struct will be in the Err state, but the err value will be
        // the default value for ResultError, and therefore possibly null.
        error = _err;
        return !_isOk;
    }

    /// <summary>
    /// Returns the result of executing the <paramref name="ok"/>
    /// or <paramref name="err"/> functions, depending on the state 
    /// of the <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T2">The return type of the given functions.</typeparam>
    /// <param name="ok">The function to pass the value to, if the result is <c>Ok</c>.</param>
    /// <param name="err">The function to pass the error value to, if the result is <c>Err</c>.</param>
    /// <returns>The value returned by the called function.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if either <paramref name="ok"/> or <paramref name="err"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T2 Match<T2>(Func<T, T2> ok, Func<ResultError, T2> err)
    {
        ThrowIfNull(ok);
        ThrowIfNull(err);
        return _isOk ? ok(_value) : err(_err);
    }

    /// <summary>
    /// Calls the <paramref name="ok"/> with the <c>Ok</c> value, or calls <paramref name="err"/>
    /// with the <c>Err</c> value, as appropriate.
    /// </summary>
    /// <param name="ok">The function to pass the value to, if the result is <c>Ok</c>.</param>
    /// <param name="err">The function to pass the error value to, if the result is <c>Err</c>.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if either <paramref name="ok"/> or <paramref name="err"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> ok, Action<ResultError> err)
    {
        ThrowIfNull(ok);
        ThrowIfNull(err);
        if (_isOk)
            ok(_value);
        else
            err(_err);
    }

    /// <summary>
    /// Returns the contained value if the result is <c>Ok</c>. Otherwise,
    /// throws an <see cref="ResultUnwrapException"/>.
    /// <para>
    /// Note: if the <c>Err</c> is <see cref="Exception"/> it will be contained as an inner exception.
    /// Otherwise, the <c>Err</c> value will be converted to a string and included in the exception message.
    /// </para>
    /// <para>
    /// Because this function may throw an exception, its use is generally discouraged.
    /// Instead, prefer to use <see cref="Match{T2}"/> and handle the Err case explicitly,
    /// or call <see cref="ResultExtensions.UnwrapOr"/> or
    /// <see cref="UnwrapOrElse(Func{ResultError, T})"/>.
    /// </para>
    /// </summary>
    /// <returns>The value inside the result, if the result is <c>Ok</c>.</returns>
    /// <exception cref="ResultUnwrapException">Thrown if the result is in the error state.</exception>
    public T Unwrap()
    {
        if (_isOk)
            return _value;

        if (_err.Exception is not null)
            throw new ResultUnwrapException("Could not unwrap a Result in the Err state.", _err.Exception);

        throw new ResultUnwrapException($"Could not unwrap a Result in the Err state: {_err}");
    }

    /// <summary>
    /// Returns the contained <c>Err</c> value, or throws an <see cref="ResultUnwrapErrException"/>.
    /// </summary>
    /// <returns>The contained <c>Err</c> value.</returns>
    /// <exception cref="ResultUnwrapErrException">Thrown when the result is in the <c>Ok</c> state.</exception>
    public ResultError UnwrapErr()
    {
        if (!_isOk)
            return _err;

        throw new ResultUnwrapErrException($"Expected the result to be in the Err state, but it was Ok: {_value}");
    }

    /// <summary>
    /// Returns the contained <c>Ok</c> value or computes it from the provided <paramref name="elseFunction"/>.
    /// </summary>
    /// <param name="elseFunction">The function that computes the returned value from the <c>Err</c> value.</param>
    /// <returns>The contained <c>Ok</c> value or the value returned by <paramref name="elseFunction"/>.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="elseFunction"/> is null.</exception>
    public T UnwrapOrElse(Func<ResultError, T> elseFunction)
    {
        ThrowIfNull(elseFunction);
        return _isOk ? _value : elseFunction(_err);
    }

    public T? UnwrapOrDefault()
        => !_isOk ? default : _value;

    /// <summary>
    /// Returns the contained value if the result is <c>Ok</c>. Otherwise,
    /// throws an <see cref="ResultUnwrapException"/> with the given message.
    /// <para>
    /// Note: if the <c>Err</c> is <see cref="Exception"/> it will be contained as an inner exception.
    /// Otherwise, the <c>Err</c> value will be converted to a string and included in the exception message.
    /// </para>
    /// <para>
    /// Because this function may throw an exception, its use is generally discouraged.
    /// Instead, prefer to use <see cref="Match{T2}"/> and handle the Err case explicitly,
    /// or call <see cref="ResultExtensions.UnwrapOr"/> or
    /// <see cref="UnwrapOrElse(Func{ResultError, T})"/>.
    /// </para>
    /// </summary>
    /// <returns>The value inside the result, if the result is <c>Ok</c>.</returns>
    /// <exception cref="ResultUnwrapException">Thrown if the result is in the error state.</exception>
    public T Expect(string message)
    {
        if (_isOk)
            return _value;

        if (_err.Exception is not null)
            throw new ResultUnwrapException(message, _err.Exception);

        throw new ResultUnwrapException($"{message} - {_err}");
    }

    /// <summary>
    /// Returns the contained <c>Err</c> value, or throws an <see cref="ResultExpectErrException"/>.
    /// </summary>
    /// <param name="message">The message for the thrown exception, if any.</param>
    /// <returns>The contained <c>Err</c> value.</returns>
    /// <exception cref="ResultExpectErrException">Thrown when the result is in the <c>Ok</c> state.</exception>
    public ResultError ExpectErr(string message)
    {
        if (!_isOk)
            return _err;

        throw new ResultExpectErrException($"{message} - {_value}");
    }

    /// <summary>
    /// Converts the result into a <see cref="ReadOnlySpan{T}"/> that contains either zero or one
    /// items depending on whether the result is <c>Err</c> or <c>Ok</c>.
    /// </summary>
    /// <returns>A span containing the result's value, or an empty span. Error values are omitted.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan()
    {
        return _isOk
            ? MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _value), 1)
            : [];
    }

    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> containing either zero or one value,
    /// depending on whether the result is <c>Err</c> or <c>Ok</c>.
    /// </summary>
    /// <returns>An enumerable containing the result's value, or an empty enumerable. Error values are omitted.</returns>
    public IEnumerable<T> AsEnumerable()
    {
        if (_isOk)
        {
            yield return _value;
        }
    }

    /// <summary>
    /// Returns an enumerator for the option.
    /// </summary>
    /// <returns>The enumerator.</returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IEnumerator<T> GetEnumerator()
    {
        if (_isOk)
        {
            yield return _value;
        }
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Equals(Result<T> other)
    {
        return (_isOk, other._isOk) switch
        {
            (true, true) => EqualityComparer<T>.Default.Equals(_value, other._value),
            (false, false) => EqualityComparer<ResultError>.Default.Equals(_err, other._err),
            _ => false
        };
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object.
    /// </summary>
    /// <param name="obj">An object to compare with this object.</param>
    /// <returns>
    /// <c>true</c> if the current object is equal to the <paramref name="obj"/> parameter;
    /// otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is Result<T> other && Equals(other);

    /// <summary>
    /// Retrieves the hash code of the object contained by the <see cref="Result{T}"/>, if any.
    /// </summary>
    /// <returns>
    /// The hash code of the object returned by the <see cref="WhenOk(out T)"/> method, or <see cref="WhenErr(out ResultError)"/>,
    /// whichever method returns <c>true</c>.
    /// </returns>
    public override int GetHashCode()
        => _isOk ? _value.GetHashCode() : _err.GetHashCode();

    /// <summary>
    /// Returns the text representation of the value of the current <see cref="Result{T}"/> object.
    /// </summary>
    /// <returns>
    /// The text representation of the value of the current <see cref="Result{T}"/> object.
    /// </returns>
    public override string ToString()
    {
        return _isOk ? $"Ok({_value})" : $"Err({_err})";
    }

    /// <summary>
    /// Formats the value of the current <see cref="Result{T}"/> using the specified format.
    /// </summary>
    /// <param name="format">
    /// The format to use, or a null reference to use the default format defined for
    /// the type of the contained value.
    /// </param>
    /// <param name="formatProvider">
    /// The provider to use to format the value, or a null reference to obtain the
    /// format information from the current locale setting of the operating system.
    /// </param>
    /// <returns>The value of the current instance in the specified format.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
        {
            return _isOk
                ? string.Create(formatProvider, $"Ok({_value})")
                : string.Create(formatProvider, $"Err({_err})");
        }

        return _isOk
            ? string.Format(formatProvider, "Ok({0:" + format + "})", _value)
            : string.Format(formatProvider, "Err({0:" + format + "})", _err);
    }

    /// <summary>
    /// Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters that were written in destination.</param>
    /// <param name="format">
    /// A span containing the characters that represent a standard or custom format string that defines the acceptable format for destination.
    /// </param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information for destination.</param>
    /// <returns><c>true</c> if the formatting was successful; otherwise, <c>false</c>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (_isOk)
        {
            if (_value is ISpanFormattable formatVal)
            {
                if ("Ok(".TryCopyTo(destination) &&
                    formatVal.TryFormat(destination[3..], out int valWritten, format, provider))
                {
                    destination = destination[(3 + valWritten)..];
                    if (!destination.IsEmpty)
                    {
                        destination[0] = ')';
                        charsWritten = valWritten + 4;
                        return true;
                    }
                }

                charsWritten = 0;
                return false;
            }
        }
        
        // For the Err case, we always fall back to ToString() to avoid interface dependency on ResultError.
        string output = this.ToString(format.IsEmpty ? null : format.ToString(), provider);

        if (output.TryCopyTo(destination))
        {
            charsWritten = output.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

#if NET8_0_OR_GREATER
    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
         if (_isOk)
        {
            if (_value is IUtf8SpanFormattable formatVal)
            {
                if ("Ok("u8.TryCopyTo(utf8Destination) &&
                    formatVal.TryFormat(utf8Destination[3..], out int valWritten, format, provider))
                {
                    utf8Destination = utf8Destination[(3 + valWritten)..];
                    if (!utf8Destination.IsEmpty)
                    {
                        utf8Destination[0] = (byte)')';
                        bytesWritten = valWritten + 4;
                        return true;
                    }
                }

                bytesWritten = 0;
                return false;
            }
        }

        // For the Err case, we always fall back to ToString() to avoid interface dependency on ResultError.
        string output = this.ToString(format.IsEmpty ? null : format.ToString(), provider);

        if (utf8Destination.Length >= output.Length)
        {
            Utf8.FromUtf16(output, utf8Destination, out _, out bytesWritten);
            return true;
        }

        bytesWritten = 0;
        return false;
    }
#endif

    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer
    /// that indicates whether the current instance precedes, follows, or occurs in the same
    /// position in the sort order as the other object.
    /// <para>
    /// Ok compares as less than any Err, while two Ok or two Err compare as their contained values would in
    /// <typeparamref name="T"/> or <see cref="ResultError"/>, respectively.
    /// </para>
    /// </summary>
    /// <param name="other"></param>
    /// <returns>
    /// <c>-1</c> if this instance precendes <paramref name="other"/>, <c>0</c> if they are equal, and <c>1</c> if this instance follows <paramref name="other"/>.
    /// </returns>
    public int CompareTo(Result<T> other)
    {
        return (_isOk, other._isOk) switch
        {
            (true, true) => Comparer<T>.Default.Compare(_value, other._value),
            (true, false) => -1,
            (false, true) => 1,
            (false, false) => Comparer<ResultError>.Default.Compare(_err, other._err)
        };
    }

    /// <summary>
    /// Determines whether one <c>Result</c> is equal to another <c>Result</c>.
    /// </summary>
    /// <param name="left">The first <c>Result</c> to compare.</param>
    /// <param name="right">The second <c>Result</c> to compare.</param>
    /// <returns><c>true</c> if the two values are equal.</returns>
    public static bool operator ==(Result<T> left, Result<T> right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether one <c>Result</c> is not equal to another <c>Result</c>.
    /// </summary>
    /// <param name="left">The first <c>Result</c> to compare.</param>
    /// <param name="right">The second <c>Result</c> to compare.</param>
    /// <returns><c>true</c> if the two values are not equal.</returns>
    public static bool operator !=(Result<T> left, Result<T> right)
        => !left.Equals(right);

    /// <summary>
    /// Determines whether one <c>Result</c> is greater than another <c>Result</c>.
    /// <para>
    /// Ok compares as less than any Err, while two Ok or two Err compare as their contained values would in
    /// <typeparamref name="T"/> or <see cref="ResultError"/>, respectively.
    /// </para>
    /// </summary>
    /// <param name="left">The first <c>Result</c> to compare.</param>
    /// <param name="right">The second <c>Result</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is greater than the <paramref name="right"/> parameter.</returns>
    public static bool operator >(Result<T> left, Result<T> right)
        => left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one <c>Result</c> is less than another <c>Result</c>.
    /// <para>
    /// Ok compares as less than any Err, while two Ok or two Err compare as their contained values would in
    /// <typeparamref name="T"/> or <see cref="ResultError"/>, respectively.
    /// </para>
    /// </summary>
    /// <param name="left">The first <c>Result</c> to compare.</param>
    /// <param name="right">The second <c>Result</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is less than the <paramref name="right"/> parameter.</returns>
    public static bool operator <(Result<T> left, Result<T> right)
        => left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one <c>Result</c> is greater than or equal to another <c>Result</c>.
    /// <para>
    /// Ok compares as less than any Err, while two Ok or two Err compare as their contained values would in
    /// <typeparamref name="T"/> or <see cref="ResultError"/>, respectively.
    /// </para>
    /// </summary>
    /// <param name="left">The first <c>Result</c> to compare.</param>
    /// <param name="right">The second <c>Result</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is greater than or equal to the <paramref name="right"/> parameter.</returns>
    public static bool operator >=(Result<T> left, Result<T> right)
        => left.CompareTo(right) >= 0;

    /// <summary>
    /// Determines whether one <c>Result</c> is less than or equal to another <c>Result</c>.
    /// <para>
    /// Ok compares as less than any Err, while two Ok or two Err compare as their contained values would in
    /// <typeparamref name="T"/> or <see cref="ResultError"/>, respectively.
    /// </para>
    /// </summary>
    /// <param name="left">The first <c>Result</c> to compare.</param>
    /// <param name="right">The second <c>Result</c> to compare.</param>
    /// <returns><c>true</c> if the <paramref name="left"/> parameter is less than or equal to the <paramref name="right"/> parameter.</returns>
    public static bool operator <=(Result<T> left, Result<T> right)
        => left.CompareTo(right) <= 0;
    
    /// <summary>
    /// Creates a successful result containing the given value.
    /// </summary>
    /// <param name="value">The successful value.</param>
    /// <returns>A successful <see cref="Types.Result{T}"/>.</returns>
    [DebuggerStepThrough]
    public static Result<T> Ok(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>An error <see cref="Types.Result{T}"/>.</returns>
    [DebuggerStepThrough]
    public static Result<T> Err(ResultError error) => new(error);

    /// <summary>
    /// Deconstructs the result into its Ok value and Err value.
    /// Enables syntax like: var (value, err) = result;
    /// Note: Only one of these will be non-null depending on the result state.
    /// </summary>
    /// <param name="value">The Ok value if present, otherwise default.</param>
    /// <param name="err">The Err value if present, otherwise default.</param>
    public void Deconstruct(out T? value, out ResultError? err)
    {
        value = _isOk ? _value : default;
        err = !_isOk ? _err : default;
    }
    
    /// <summary>
    /// Deconstructs the result into its success state, Ok value, and Err value.
    /// Primary method for pattern matching support.
    /// Enables syntax like: if (a result is (true, var value, _)) { ... }
    /// </summary>
    /// <param name="isOk">True if the result is Ok, false if Err.</param>
    /// <param name="value">The Ok value if present, otherwise default.</param>
    /// <param name="err">The Err value if present, otherwise default.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out bool isOk, out T? value, out ResultError? err)
    {
        isOk = _isOk;
        if (isOk)
        {
            value = _value;
            err = default;
        }
        else
        {
            value = default;
            err = _err;
        }
    }
    
    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<T>(T value) => Ok(value);

    /// <summary>
    /// Implicitly converts a <see cref="ResultError"/> value into a failed <see cref="Result{T}"/>.
    /// Enables direct assignment of an error to a result without calling <c>Err()</c>.
    /// </summary>
    /// <param name="error">The error value to wrap as a failed result.</param>
    public static implicit operator Result<T>(ResultError error) => Err(error);
    
    /// <summary>
    /// Implicitly converts a <see cref="string"/> value into a failed <see cref="Result{T}"/>.
    /// Enables direct assignment of an error to a result without calling <c>Err()</c>.
    /// </summary>
    /// <param name="error">The error value to wrap as a failed result.</param>
    public static implicit operator Result<T>(string error) => Err(error);
    
    /// <summary>
    /// Explicitly converts a <see cref="Result{T}"/> to a <see cref="bool"/> indicating success.
    /// Returns <see langword="true"/> if the result is Ok; otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns><see langword="true"/> if Ok; otherwise <see langword="false"/>.</returns>
    public static explicit operator bool(Result<T> result) => result.IsOk;
    
    /// <summary>
    /// Explicitly converts a <see cref="Result{T}"/> to it's Ok value.
    /// Throws an exception if the result represents an error.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    /// <returns>The contained Ok value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is an Err.</exception>
    public static explicit operator T(Result<T> result) => result.Unwrap();
    
    /// <summary>
    /// Explicitly converts a <see cref="Result{T}"/> to its Err value.
    /// Throws an exception if the result represents success.
    /// </summary>
    /// <param name="result">The result to unwrap.</param>
    /// <returns>The contained Err value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is an Ok.</exception>
    public static explicit operator ResultError(Result<T> result) => result.UnwrapErr();
    
    [Obsolete("Using default(Result<>) is unsafe. Use Ok(...) or Err(...) instead.", true)]
    public static Result<T> UnsafeDefault() => default;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSource> TryInvoke<TSource>(Func<TSource> func)
        where TSource : notnull
    {
        try
        {
            return Result<TSource>.Ok(func());
        }
        catch (Exception ex)
        {
            return Result<TSource>.Err(new ResultError(ex.Message, ex));
        }
    }

}