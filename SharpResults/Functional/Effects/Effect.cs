using SharpResults.Core;
using SharpResults.Core.Types;
using SharpResults.Types;

namespace SharpResults.Functional.Effects;

/// <summary>
/// Describes an effect without executing it
/// </summary>
public abstract class Effect<T> where T : notnull
{
    public abstract Task<Result<T, ResultError>> Run();
}
