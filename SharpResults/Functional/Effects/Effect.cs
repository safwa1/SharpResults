using SharpResults.Types;

namespace SharpResults.Functional.Effects;

/// <summary>
/// Describes an effect without executing it
/// </summary>
public abstract class Effect<T> where T : notnull
{
    public abstract Task<Result<T, Exception>> Run();
}
