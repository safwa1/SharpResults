using SharpResults.Core;
using SharpResults.Types;

namespace SharpResults.Extensions.Linq;

public static class OptionLinqExtensions
{
    public static Option<U> Select<T, U>(this Option<T> option, Func<T, U> selector) where T : notnull where U : notnull => option.Map(selector);

    public static Option<V> SelectMany<T, U, V>(
        this Option<T> option,
        Func<T, Option<U>> binder,
        Func<T, U, V> projector) where V : notnull where T : notnull where U : notnull
    {
        return option.AndThen<T, V>(t =>
            binder(t).AndThen<U, V>(u =>
                Option.Some(projector(t, u))));
    }

    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) where T : notnull
    {
        return option.AndThen(t => predicate(t) ? option : Option.None<T>());
    }
}