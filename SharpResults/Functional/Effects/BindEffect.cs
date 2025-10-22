using SharpResults.Core;
using SharpResults.Core.Types;
using SharpResults.Types;

namespace SharpResults.Functional.Effects;

public class BindEffect<T, U> : Effect<U>
    where T : notnull
    where U : notnull
{
    private readonly Effect<T> _effect;
    private readonly Func<T, Effect<U>> _f;

    public BindEffect(Effect<T> effect, Func<T, Effect<U>> f)
    {
        _effect = effect;
        _f = f;
    }

    public override async Task<Result<U, ResultError>> Run()
    {
        var result = await _effect.Run();
        if (result.IsErr)
            return Result.Err<U>(result.UnwrapErr());

        var nextEffect = _f(result.Unwrap());
        return await nextEffect.Run();
    }
}