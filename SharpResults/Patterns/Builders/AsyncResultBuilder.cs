using SharpResults.Core;
using SharpResults.Types;
using static System.ArgumentNullException;

namespace SharpResults.Patterns.Builders;

/// <summary>
/// Async version of ResultBuilder for validating and transforming objects 
/// through a pipeline that involves I/O operations (database, API, etc.).
/// </summary>
/// <typeparam name="T">The type being validated and transformed.</typeparam>
public class AsyncResultBuilder<T> where T : notnull
{
    private readonly List<Func<T, Task<Result<T, string>>>> _steps = new();

    /// <summary>
    /// Adds an async validation/transformation step to the pipeline.
    /// </summary>
    /// <param name="step">The async function that validates or transforms the object.</param>
    /// <returns>The builder for method chaining.</returns>
    public AsyncResultBuilder<T> With(Func<T, Task<Result<T, string>>> step)
    {
        ThrowIfNull(step);
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Adds a synchronous validation/transformation step to the pipeline.
    /// </summary>
    /// <param name="step">The synchronous function that validates or transforms the object.</param>
    /// <returns>The builder for method chaining.</returns>
    public AsyncResultBuilder<T> With(Func<T, Result<T, string>> step)
    {
        ThrowIfNull(step);
        _steps.Add(obj => Task.FromResult(step(obj)));
        return this;
    }

    /// <summary>
    /// Executes all steps in the pipeline and returns either the transformed object
    /// or a collection of all validation errors.
    /// </summary>
    /// <param name="initial">The initial object to validate and transform.</param>
    /// <returns>
    /// A Result containing either the successfully transformed object or 
    /// a list of all validation errors encountered.
    /// </returns>
    public async Task<Result<T, IReadOnlyList<string>>> BuildAsync(T initial)
    {
        ThrowIfNull(initial);

        var errors = new List<string>();
        var current = initial;

        foreach (var step in _steps)
        {
            var result = await step(current).ConfigureAwait(false);
            
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
            ? Result.Ok<T, IReadOnlyList<string>>(current)
            : Result.Err<T, IReadOnlyList<string>>(errors);
    }

    /// <summary>
    /// Executes all steps in the pipeline, stopping at the first error.
    /// This is useful when you want fail-fast behavior instead of collecting all errors.
    /// </summary>
    /// <param name="initial">The initial object to validate and transform.</param>
    /// <returns>
    /// A Result containing either the successfully transformed object or the first error.
    /// </returns>
    public async Task<Result<T, string>> BuildFastFailAsync(T initial)
    {
        ThrowIfNull(initial);

        var current = initial;

        foreach (var step in _steps)
        {
            var result = await step(current).ConfigureAwait(false);
            
            if (result.IsErr)
            {
                return Result.Err<T, string>(result.UnwrapErr());
            }
            
            current = result.Unwrap();
        }

        return Result.Ok<T, string>(current);
    }
}