using SharpResults.Core;
using SharpResults.Types;

namespace SharpResults.Functional.Lenses;

public static class LensExtensions
{
    /// <summary>
    /// Safely get a property using a lens, returning Option
    /// </summary>
    public static Option<TProp> GetOption<TObj, TProp>(
        this Lens<TObj, TProp> lens,
        TObj obj)
        where TObj : notnull
        where TProp : notnull
    {
        try
        {
            return Option.Some(lens.Get(obj));
        }
        catch
        {
            return Option<TProp>.None;
        }
    }
}
