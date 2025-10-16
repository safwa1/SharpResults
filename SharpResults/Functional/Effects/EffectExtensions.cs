namespace SharpResults.Functional.Effects;

public static class EffectExtensions
{
    public static Effect<U> Select<T, U>(this Effect<T> effect, Func<T, U> f)
        where T : notnull
        where U : notnull
        => new BindEffect<T, U>(effect, x => new PureEffect<U>(f(x)));

    public static Effect<U> SelectMany<T, U>(
        this Effect<T> effect,
        Func<T, Effect<U>> f)
        where T : notnull
        where U : notnull
        => new BindEffect<T, U>(effect, f);
}
