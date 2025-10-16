using SharpResults.Types;

namespace SharpResults.Functional.TypeClasses;

public class ResultFoldable<T, TErr> : IFoldable<Result<T, TErr>, T> 
    where T : notnull 
    where TErr : notnull
{
    public TResult Fold<TResult>(Result<T, TErr> container, TResult initial, Func<TResult, T, TResult> f)
        where TResult : notnull
    {
        return container.WhenOk(out var value)
            ? f(initial, value)
            : initial;
    }
}