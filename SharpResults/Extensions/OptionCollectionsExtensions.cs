using SharpResults.Types;

namespace SharpResults.Extensions;

public static class OptionCollectionsExtensions
{
    // Convert Option<T> to List<T> (empty if None)
    public static List<T> ToList<T>(this Option<T> option) where T : notnull
    {
        return option.Match<List<T>>(
            some: value => [value],
            none: () => []
        );
    }

    // Convert IEnumerable<Option<T>> to Option<IEnumerable<T>>
    // (None if any element is None)
    public static Option<IEnumerable<T>> Sequence<T>(
        this IEnumerable<Option<T>> options) where T : notnull
    {
        var list = new List<T>();
        foreach (var option in options)
        {
            if (option.IsNone)
                return Option.None<IEnumerable<T>>();
                
            list.Add(option.Unwrap());
        }
        return Option.Some<IEnumerable<T>>(list);
    }

    // Convert IEnumerable<Option<T>> to Option<List<T>>
    public static Option<List<T>> SequenceList<T>(
        this IEnumerable<Option<T>> options) where T : notnull
    {
        return options.Sequence().Map(x => x.ToList());
    }
}