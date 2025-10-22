using System.Collections.Immutable;
using SharpResults.Core;
using SharpResults.Types;
using static System.ArgumentNullException;

namespace SharpResults.Patterns.Builders;

/// <summary>
/// Synchronous version of ResultBuilder for validating and transforming objects 
/// through a pipeline without async I/O.
/// </summary>
/// <typeparam name="T">The type being validated and transformed.</typeparam>
/// <typeparam name="TErr"></typeparam>
public class ResultBuilder<T, TErr> where T : notnull where TErr : notnull
{
    private ImmutableArray<Func<T, Result<T, TErr>>> _steps = ImmutableArray<Func<T, Result<T, TErr>>>.Empty;

    /// <summary>
    /// Adds a synchronous validation/transformation step to the pipeline.
    /// </summary>
    /// <param name="step">The function that validates or transforms the object.</param>
    /// <returns>The builder for method chaining.</returns>
    public ResultBuilder<T, TErr> With(Func<T, Result<T, TErr>> step)
    {
        ThrowIfNull(step);
        _steps = _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Executes all steps and returns the transformed object or all collected errors.
    /// </summary>
    /// <param name="initial">Initial object to validate and transform.</param>
    public Result<T, IReadOnlyList<TErr>> Build(T initial)
    {
        ThrowIfNull(initial);

        var errors = new List<TErr>();
        var current = initial;

        foreach (var step in _steps)
        {
            var result = step(current);

            if (result.IsErr)
            {
                errors.Add(result.UnwrapErr());
            }
            else
            {
                current = result.Unwrap();
            }
        }

        return errors.Count == 0
            ? Result.Ok<T, IReadOnlyList<TErr>>(current)
            : Result.Err<T, IReadOnlyList<TErr>>(errors);
    }

    /// <summary>
    /// Executes all steps but stops at the first error (fail-fast).
    /// </summary>
    /// <param name="initial">Initial object to validate and transform.</param>
    public Result<T, TErr> BuildFastFail(T initial)
    {
        ThrowIfNull(initial);

        var current = initial;

        foreach (var step in _steps)
        {
            var result = step(current);

            if (result.IsErr)
                return Result.Err<T, TErr>(result.UnwrapErr());

            current = result.Unwrap();
        }

        return Result.Ok<T, TErr>(current);
    }
}