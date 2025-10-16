using SharpResults.Types;

namespace SharpResults.Functional.TypeClasses;

public class OptionFoldable<T> : IFoldable<Option<T>, T> where T : notnull
{
    public TResult Fold<TResult>(Option<T> container, TResult initial, Func<TResult, T, TResult> f)
        where TResult : notnull
    {
        return container.WhenSome(out var value)
            ? f(initial, value)
            : initial;
    }
}