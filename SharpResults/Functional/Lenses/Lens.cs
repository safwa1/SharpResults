namespace SharpResults.Functional.Lenses;

/// <summary>
/// Immutable property access and modification
/// </summary>
public class Lens<TObj, TProp>
    where TObj : notnull
    where TProp : notnull
{
    private readonly Func<TObj, TProp> _getter;
    private readonly Func<TObj, TProp, TObj> _setter;

    public Lens(Func<TObj, TProp> getter, Func<TObj, TProp, TObj> setter)
    {
        _getter = getter;
        _setter = setter;
    }

    public TProp Get(TObj obj) => _getter(obj);

    public TObj Set(TObj obj, TProp value) => _setter(obj, value);

    public TObj Modify(TObj obj, Func<TProp, TProp> f)
        => _setter(obj, f(_getter(obj)));

    // Lens composition
    public Lens<TObj, TProp2> Compose<TProp2>(Lens<TProp, TProp2> other)
        where TProp2 : notnull
    {
        return new Lens<TObj, TProp2>(
            obj => other.Get(_getter(obj)),
            (obj, val) => _setter(obj, other.Set(_getter(obj), val))
        );
    }
}