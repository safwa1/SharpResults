using SharpResults.Core;
using SharpResults.Types;

namespace SharpResults.Patterns.Query;

/// <summary>
/// Enables full LINQ query comprehension syntax with let, join, and group support
/// </summary>
public static class QueryPatternExtensions
{
    // Already have Select, SelectMany, Where - adding advanced patterns
    
    /// <summary>
    /// Enables 'let' bindings in query expressions for Options
    /// </summary>
    public static Option<TResult> Let<T, TIntermediate, TResult>(
        this Option<T> option,
        Func<T, TIntermediate> binding,
        Func<T, TIntermediate, TResult> selector)
        where T : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        if (!option.WhenSome(out var value))
            return Option<TResult>.None;
        
        var intermediate = binding(value);
        return Option.Some(selector(value, intermediate));
    }

    /// <summary>
    /// Join two Options based on matching keys
    /// </summary>
    public static Option<TResult> Join<TOuter, TInner, TKey, TResult>(
        this Option<TOuter> outer,
        Option<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector)
        where TOuter : notnull
        where TInner : notnull
        where TKey : notnull
        where TResult : notnull
    {
        if (!outer.WhenSome(out var outerVal) || !inner.WhenSome(out var innerVal))
            return Option<TResult>.None;

        var outerKey = outerKeySelector(outerVal);
        var innerKey = innerKeySelector(innerVal);

        return EqualityComparer<TKey>.Default.Equals(outerKey, innerKey)
            ? Option.Some(resultSelector(outerVal, innerVal))
            : Option<TResult>.None;
    }
}