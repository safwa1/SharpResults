using SharpResults.Types;
using static SharpResults.Option;

namespace SharpResults.Extensions;

public static class OptionExtensions
{
    public static void Match<T>(this T? value, Action<T> some, Action none) where T : class
    {
        if (value != null) some(value);
        else none();
    }

    public static R Match<T, R>(this T? value, Func<T, R> some, Func<R> none) where T : class
    {
        return value != null ? some(value) : none();
    }

    public static U? Bind<T, U>(this T? value, Func<T, U?> binder) where T : class where U : class
    {
        return value != null ? binder(value) : null;
    }
    
    public static Option<T> ToSome<T>(this T value) => Some(value);
    
    public static Option<T> ToOption<T>(this T? value) where T : class => value != null ? Some(value) : Defaults.None;
}

public static class OptionLinqExtensions
{
    public static Option<U> Select<T, U>(this Option<T> option, Func<T, U> selector)
        => option.Map(selector);

    public static Option<V> SelectMany<T, U, V>(
        this Option<T> option,
        Func<T, Option<U>> binder,
        Func<T, U, V> projector)
    {
        return option.AndThen(t =>
            binder(t).AndThen(u =>
                Option<V>.Some(projector(t, u))));
    }

    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate)
    {
        return option.AndThen(t => predicate(t) ? option : Option<T>.None());
    }
}

public static class OptionCollectionExtensions
{
    // Convert Option<T> to List<T> (empty if None)
    public static List<T> ToList<T>(this Option<T> option)
    {
        return option.Match(
            some: value => new List<T> { value },
            none: () => new List<T>()
        );
    }

    // Convert IEnumerable<Option<T>> to Option<IEnumerable<T>>
    // (None if any element is None)
    public static Option<IEnumerable<T>> Sequence<T>(
        this IEnumerable<Option<T>> options)
    {
        var list = new List<T>();
        foreach (var option in options)
        {
            if (option.IsNone)
                return Option<IEnumerable<T>>.None();
                
            list.Add(option.Value);
        }
        return Option<IEnumerable<T>>.Some(list);
    }

    // Convert IEnumerable<Option<T>> to Option<List<T>>
    public static Option<List<T>> SequenceList<T>(
        this IEnumerable<Option<T>> options)
    {
        return options.Sequence().Map(x => x.ToList());
    }

    // Extract all Some values
    public static IEnumerable<T> Values<T>(
        this IEnumerable<Option<T>> options)
    {
        return options.Where(o => o.IsSome).Select(o => o.Value);
    }

    // Partition into (List<T> some, List<T> none)
    public static (List<T> Some, List<T> None) Partition<T>(
        this IEnumerable<Option<T>> options)
    {
        var some = new List<T>();
        var none = new List<T>();
            
        foreach (var option in options)
        {
            option.Match(
                some: value => some.Add(value),
                none: () => none.Add(default!)
            );
        }
            
        return (some, none);
    }
}