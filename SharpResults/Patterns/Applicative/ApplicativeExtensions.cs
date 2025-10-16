using SharpResults.Core;
using SharpResults.Types;

namespace SharpResults.Patterns.Applicative;


public static class ApplicativeExtensions
{
    /// <summary>
    /// Apply a function wrapped in an Option to a value wrapped in an Option
    /// </summary>
    public static Option<TResult> Apply<T, TResult>(
        this Option<Func<T, TResult>> optionFunc,
        Option<T> optionValue)
        where T : notnull
        where TResult : notnull
    {
        if (!optionFunc.WhenSome(out var func) || !optionValue.WhenSome(out var value))
            return Option<TResult>.None;

        return Option.Some(func(value));
    }

    /// <summary>
    /// Accumulate multiple validation results, collecting ALL errors
    /// </summary>
    public static Result<IReadOnlyList<T>, IReadOnlyList<TErr>> Traverse<T, TErr>(
        this IEnumerable<Result<T, TErr>> results)
        where T : notnull
        where TErr : notnull
    {
        var values = new List<T>();
        var errors = new List<TErr>();

        foreach (var result in results)
        {
            if (result.WhenOk(out var value))
                values.Add(value);
            else if (result.WhenErr(out var err))
                errors.Add(err!);
        }

        return errors.Count == 0
            ? Result.Ok<IReadOnlyList<T>, IReadOnlyList<TErr>>(values)
            : Result.Err<IReadOnlyList<T>, IReadOnlyList<TErr>>(errors);
    }

    /// <summary>
    /// Parallel validation - runs all validations and accumulates errors
    /// </summary>
    public static Result<T, IReadOnlyList<string>> ValidateParallel<T>(
        this T value,
        params Func<T, Result<T, string>>[] validators)
        where T : notnull
    {
        var errors = new List<string>();

        foreach (var validator in validators)
        {
            var result = validator(value);
            if (result.WhenErr(out var error))
                errors.Add(error!);
        }

        return errors.Count == 0
            ? Result.Ok<T, IReadOnlyList<string>>(value)
            : Result.Err<T, IReadOnlyList<string>>(errors);
    }
}
