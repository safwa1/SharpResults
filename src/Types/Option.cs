using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SharpResults.Types;


/// <summary>
/// Represents an optional value: every Option is either Some and contains a value, or None.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[JsonConverter(typeof(OptionJsonConverter))]
[DebuggerDisplay("{ToString()}")]
public readonly struct Option<T> : IEquatable<Option<T>>
{
    private static readonly Option<T> _none = new(new None());
    
    private readonly T _value;

    /// <summary>
    /// Indicates whether the option contains a value.
    /// </summary>
    public bool IsSome { get; }

    /// <summary>
    /// Indicates whether the option does not contain a value.
    /// </summary>
    public bool IsNone => !IsSome;

    /// <summary>
    /// Gets the contained value or throws if None.
    /// </summary>
    public T Value => IsSome
        ? _value
        : throw new InvalidOperationException("Cannot access the value of a None Option.");

    private Option(T value)
    {
        _value = value!;
        IsSome = true;
    }

    private Option(None _)
    {
        _value = default!;
        IsSome = false;
    }

    /// <summary>
    /// Creates an Option with a value.
    /// </summary>
    public static Option<T> Some(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "Cannot assign null to Some.");
        return new Option<T>(value);
    }

    /// <summary>
    /// Creates an Option with no value.
    /// </summary>
    public static Option<T> None() => _none;
    
    public bool TryUnwrap(out T value)
    {
        value = _value;
        return IsSome;
    }

    /// <summary>
    /// Returns the value or a fallback.
    /// </summary>
    [DebuggerStepThrough]
    public T UnwrapOr(T defaultValue) => IsSome ? _value : defaultValue;
    
    public T UnwrapOrElse(Func<T> factory)
    {
        return IsSome ? _value : factory();
    }
    
    public Result<T, E> OkOr<E>(E error)
    {
        return IsSome ? Result<T, E>.Ok(_value) : Result<T, E>.Err(error);
    }

    public Result<T, E> OkOrElse<E>(Func<E> errorFactory)
    {
        return IsSome ? Result<T, E>.Ok(_value) : Result<T, E>.Err(errorFactory());
    }

    /// <summary>
    /// Maps the value if present.
    /// </summary>
    [DebuggerStepThrough]
    public Option<U> Map<U>(Func<T, U> mapper)
    {
        return IsSome ? Option<U>.Some(mapper(_value)) : Option<U>.None();
    }

    /// <summary>
    /// Applies a function returning another Option if present.
    /// </summary>
    public Option<U> AndThen<U>(Func<T, Option<U>> binder)
    {
        return IsSome ? binder(_value) : Option<U>.None();
    }

    public bool Equals(Option<T> other)
    {
        if (IsNone && other.IsNone) return true;
        if (IsSome && other.IsSome) return EqualityComparer<T>.Default.Equals(_value, other._value);
        return false;
    }

    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    public override int GetHashCode() => IsSome ? _value?.GetHashCode() ?? 0 : 0;

    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);
    public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

    /// <summary>
    /// Explicit conversion from T to Option&lt;T&gt;.
    /// </summary>
    public static explicit operator Option<T>(T? value) => value is null ? None() : Some(value);

    /// <summary>
    /// Explicit conversion from Option&lt;T&gt; to T.
    /// </summary>
    public static explicit operator T(Option<T> option) => option.Value;

    /// <summary>
    /// Implicit conversion from T to Option&lt;T&gt; (wraps non-null values).
    /// </summary>
    public static implicit operator Option<T>?(T? value)
        => value is null ? None() : Some(value);

    /// <summary>
    /// Implicit conversion from None to Option&lt;T&gt;.
    /// </summary>
    public static implicit operator Option<T>(None _) => _none;

    /// <summary>
    /// Implicit conversion from Option&lt;T&gt; to bool indicating presence.
    /// </summary>
    public static implicit operator bool(Option<T> option) => option.IsSome;
    

    /// <summary>
    /// Returns the original option if it contains a value, otherwise returns the alternative.
    /// </summary>
    public Option<T> Or(Option<T> alternative) => IsSome ? this : alternative;

    /// <summary>
    /// Returns the original option if it contains a value, otherwise evaluates and returns the alternative.
    /// </summary>
    public Option<T> OrElse(Func<Option<T>> alternativeFactory) => IsSome ? this : alternativeFactory();

    /// <summary>
    /// Maps the value if present, otherwise returns the provided default.
    /// </summary>
    public Option<U> MapOr<U>(Func<T, U> mapper, U defaultValue)
        => IsSome ? Option<U>.Some(mapper(_value)) : Option<U>.Some(defaultValue);

    /// <summary>
    /// Maps the value if present, otherwise evaluates and returns the default.
    /// </summary>
    public Option<U> MapOrElse<U>(Func<T, U> mapper, Func<U> defaultValueFactory)
        => IsSome ? Option<U>.Some(mapper(_value)) : Option<U>.Some(defaultValueFactory());

    /// <summary>
    /// Executes the action if the value is present.
    /// </summary>
    public Option<T> Inspect(Action<T> action)
    {
        if (IsSome) action(_value);
        return this;
    }

    /// <summary>
    /// Asynchronously maps the value if present.
    /// </summary>
    public async Task<Option<U>> MapAsync<U>(Func<T, Task<U>> mapperAsync)
        => IsSome ? Option<U>.Some(await mapperAsync(_value)) : Option<U>.None();

    /// <summary>
    /// Asynchronously applies a function returning another Option if present.
    /// </summary>
    public async Task<Option<U>> AndThenAsync<U>(Func<T, Task<Option<U>>> binderAsync)
        => IsSome ? await binderAsync(_value) : Option<U>.None();

    
    /// <summary>
    /// Pattern matches the Option's state
    /// </summary>
    /// <typeparam name="TResult">Result type</typeparam>
    /// <param name="some">Handler for Some case</param>
    /// <param name="none">Handler for None case</param>
    /// <returns>The result of the matched handler</returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    {
        if (some == null) throw new ArgumentNullException(nameof(some));
        if (none == null) throw new ArgumentNullException(nameof(none));
        
        return IsSome ? some(_value) : none();
    }

    /// <summary>
    /// Pattern matches the Option's state (void version)
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<T> some, Action none)
    {
        if (some == null) throw new ArgumentNullException(nameof(some));
        if (none == null) throw new ArgumentNullException(nameof(none));
        
        if (IsSome) some(_value); else none();
    }

    /// <summary>
    /// Pattern matches the Option's state with a default None value
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<T, TResult> some, TResult none)
    {
        if (some == null) throw new ArgumentNullException(nameof(some));
        return IsSome ? some(_value) : none;
    }
    
    public Option<(T, U)> Zip<U>(Option<U> other) =>
        IsSome && other.IsSome ? Option<(T, U)>.Some((Value, other.Value)) : Option<(T, U)>.None();

    public Option<T> Filter(Func<T, bool> predicate) =>
        IsSome && predicate(_value) ? this : None();
    
    /// <summary>
    /// Deconstructs the option into a flag and value.
    /// </summary>
    public void Deconstruct(out bool isSome, out T? value)
    {
        isSome = IsSome;
        value = IsSome ? _value : default;
    }

    /// <summary>
    /// Returns a string that represents the current option.
    /// </summary>
    public override string ToString() => IsSome ? $"Some({_value})" : "None";
}

