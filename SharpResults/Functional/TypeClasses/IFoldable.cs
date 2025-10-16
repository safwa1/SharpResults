namespace SharpResults.Functional.TypeClasses;

/// <summary>
/// Foldable typeclass - allows folding over container types
/// </summary>
public interface IFoldable<in TContainer, out T> 
    where T : notnull
{
    TResult Fold<TResult>(TContainer container, TResult initial, Func<TResult, T, TResult> f)
        where TResult : notnull;
}