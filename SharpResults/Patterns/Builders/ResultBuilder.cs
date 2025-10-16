using SharpResults.Core;
using SharpResults.Types;
using static System.ArgumentNullException;

namespace SharpResults.Patterns.Builders;

/// <summary>
/// Synchronous version of ResultBuilder for validating and transforming objects 
/// through a pipeline without async I/O.
/// </summary>
/// <typeparam name="T">The type being validated and transformed.</typeparam>
public class ResultBuilder<T> where T : notnull
{
    private readonly List<Func<T, Result<T, string>>> _steps = new();

    /// <summary>
    /// Adds a synchronous validation/transformation step to the pipeline.
    /// </summary>
    /// <param name="step">The function that validates or transforms the object.</param>
    /// <returns>The builder for method chaining.</returns>
    public ResultBuilder<T> With(Func<T, Result<T, string>> step)
    {
        ThrowIfNull(step);
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Executes all steps and returns the transformed object or all collected errors.
    /// </summary>
    /// <param name="initial">Initial object to validate and transform.</param>
    public Result<T, IReadOnlyList<string>> Build(T initial)
    {
        ThrowIfNull(initial);

        var errors = new List<string>();
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
            ? Result.Ok<T, IReadOnlyList<string>>(current)
            : Result.Err<T, IReadOnlyList<string>>(errors);
    }

    /// <summary>
    /// Executes all steps but stops at the first error (fail-fast).
    /// </summary>
    /// <param name="initial">Initial object to validate and transform.</param>
    public Result<T, string> BuildFastFail(T initial)
    {
        ThrowIfNull(initial);

        var current = initial;

        foreach (var step in _steps)
        {
            var result = step(current);

            if (result.IsErr)
                return Result.Err<T, string>(result.UnwrapErr());

            current = result.Unwrap();
        }

        return Result.Ok<T, string>(current);
    }
}

/*
public class ResultBuilder<TErr> where TErr : notnull
{
    public ResultAwaiter<T, TErr> GetAwaiter<T>(Result<T, TErr> result) where T : notnull
        => new(result);

    public class ResultAwaiter<T, TErr> : INotifyCompletion
        where T : notnull
        where TErr : notnull
    {
        private readonly Result<T, TErr> _result;

        public ResultAwaiter(Result<T, TErr> result) => _result = result;

        public bool IsCompleted => true;

        public T GetResult() => _result.Unwrap();

        public void OnCompleted(Action continuation) => continuation();
    }
}
*/