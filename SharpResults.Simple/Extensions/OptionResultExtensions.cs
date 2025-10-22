using System.Numerics;
using SharpResults.Core;
using SharpResults.Core.Types;
using SharpResults.Simple.Types;
using SharpResults.Types;
using static System.ArgumentNullException;
using Result = SharpResults.Simple.Core.Result;

namespace SharpResults.Simple.Extensions;

/// <summary>
/// Extension methods that transform <see cref="Option{T}"/> to <see cref="Result{T}"/>, or vice versa.
/// </summary>
public static class OptionResultExtensions
{
    /// <summary>
    /// Transforms the <see cref="Option{T}"/> into a <see cref="Result{T}"/>,
    /// mapping <c>Some</c> to <c>Ok</c> and <c>None</c> to <c>Err</c> using the provided
    /// <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="self">The option to transform.</param>
    /// <param name="error">The error to use if the option is <c>None</c>.</param>
    /// <returns>A <see cref="Result{T}"/> that contains either the option's value, or the provided error.</returns>
    public static Result<T> OkOr<T>(this Option<T> self, ResultError error)
        where T : notnull
    {
        return self.WhenSome(out var value)
            ? new Result<T>(value) 
            : new Result<T>(error);
    }

    /// <summary>
    /// Transforms the <see cref="Option{T}"/> into a <see cref="Result{T}"/>,
    /// mapping <c>Some</c> to <c>Ok</c> and <c>None</c> to <c>Err</c> using the provided
    /// <paramref name="errorFactory"/>.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="self">The option to transform.</param>
    /// <param name="errorFactory">A function that creates an error object to be used if the option is <c>None</c>.</param>
    /// <returns>A <see cref="Result{T}"/> that contains either the option's value, or the provided error.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="errorFactory"/> is null.</exception>
    public static Result<T> OkOrElse<T>(this Option<T> self, Func<ResultError> errorFactory)
        where T : notnull
    {
        ThrowIfNull(errorFactory);
        return self.WhenSome(out var value)
            ? new Result<T>(value) 
            : new Result<T>(errorFactory());
    }
    
    /// <summary>
    /// Transforms the <see cref="NumericOption{T}"/> into a <see cref="Result{T}"/>,
    /// mapping <c>Some</c> to <c>Ok</c> and <c>None</c> to <c>Err</c> using the provided
    /// <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="self">The option to transform.</param>
    /// <param name="error">The error to use if the option is <c>None</c>.</param>
    /// <returns>A <see cref="Result{T}"/> that contains either the option's value, or the provided error.</returns>
    public static Result<T> OkOr<T>(this NumericOption<T> self, ResultError error)
        where T : struct, INumber<T>
    {
        return self.IsSome(out var value)
            ? new Result<T>(value) 
            : new Result<T>(error);
    }

    /// <summary>
    /// Transforms the <see cref="NumericOption{T}"/> into a <see cref="Result{T}"/>,
    /// mapping <c>Some</c> to <c>Ok</c> and <c>None</c> to <c>Err</c> using the provided
    /// <paramref name="errorFactory"/>.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="self">The option to transform.</param>
    /// <param name="errorFactory">A function that creates an error object to be used if the option is <c>None</c>.</param>
    /// <returns>A <see cref="Result{T,TErr}"/> that contains either the option's value, or the provided error.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="errorFactory"/> is null.</exception>
    public static Result<T> OkOrElse<T>(this NumericOption<T> self, Func<ResultError> errorFactory)
        where T : struct, INumber<T>
    {
        ThrowIfNull(errorFactory);
        return self.IsSome(out var value)
            ? new(value) : new(errorFactory());
    }

    /// <summary>
    /// Transposes an <c>Option</c> of a <c>Result</c> into a <c>Result</c> of an <c>Option</c>.
    /// <para>
    ///     <c>None</c> will be mapped to <c>Ok(None)</c>. 
    ///     <c>Some(Ok(_))</c> and <c>Some(Err(_))</c> will be mapped to <c>Ok(Some(_))</c> and <c>Err(_)</c>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="self">An option containing a result.</param>
    /// <returns>An equivalent result containing an option.</returns>
    public static Result<Option<T>> Transpose<T>(this Option<Result<T>> self)
        where T : notnull
    {
        if (self.WhenSome(out var result))
        {
            var x = result.Match(
                ok: val => Result.Ok(Option.Some(val)),
                err: Result.Err<Option<T>>
            );
        }

        return Result.Ok<Option<T>>(default);
    }

    /// <summary>
    /// Converts from the <c>Err</c> state of <see cref="Result{T}"/> to <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="self">The result to be converted.</param>
    /// <returns><c>Some(TErr)</c> if the result is <c>Err</c>, otherwise <c>None</c>.</returns>
    public static Option<ResultError> Err<T>(this Result<T> self)
        where T : notnull
    {
        return self.WhenErr(out var err)
            ? Option.Some(err!)
            : default;
    }

    /// <summary>
    /// Converts from the <c>Ok</c> state of <see cref="Result{T}"/> to <see cref="Option{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="self">The result to be converted.</param>
    /// <returns><c>Some(T)</c> if the result is <c>Ok</c>, otherwise <c>None</c>.</returns>
    public static Option<T> Ok<T>(this Result<T> self)
        where T : notnull
    {
        return self.WhenOk(out var value)
            ? Option.Some(value)
            : default;
    }

    /// <summary>
    /// Converts from the <c>Ok</c> state of <see cref="Result{T}"/> to <see cref="NumericOption{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="self">The result to be converted.</param>
    /// <returns><c>Some(T)</c> if the result is <c>Ok</c>, otherwise <c>None</c>.</returns>
    public static NumericOption<T> OkNumber<T>(this Result<T> self)
        where T : struct, INumber<T>
    {
        return self.WhenOk(out var value)
            ? NumericOption.Some(value)
            : default;
    }

    /// <summary>
    /// Transposes a <c>Result</c> of an <c>Option</c> into an <c>Option</c> of a <c>Result</c>.
    /// <para>
    ///     <c>Ok(None)</c> will be mapped to <c>None</c>. 
    ///     <c>Ok(Some(_))</c> and <c>Err(_)</c> will be mapped to <c>Some(Ok(_))</c> and <c>Some(Err(_))</c>.
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="self">A result containing an option.</param>
    /// <returns>An equivalent option containing a result.</returns>
    public static Option<Result<T>> Transpose<T>(this Result<Option<T>> self)
        where T : notnull
    {
        return self.Match<Option<Result<T>>>(
            ok: x =>
            {
                if (x.WhenSome(out var value))
                    return Option.Some(new Result<T>(value));
                return Option<Result<T>>.None;
            },
            err: e => Option<Result<T>>.None
        );
    }
}

