using SharpResults.Core;
using SharpResults.Types;

namespace SharpResults.Functional.Effects;

public class PureEffect<T> : Effect<T> where T : notnull
{
    private readonly T _value;
    public PureEffect(T value) => _value = value;
    
    public override Task<Result<T, Exception>> Run()
        => Task.FromResult(Result.Ok<T, Exception>(_value));
}