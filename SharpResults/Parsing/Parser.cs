using SharpResults.Core;
using SharpResults.Extensions;
using SharpResults.Types;

namespace SharpResults.Parsing;

public class Parser<T> where T : notnull
{
    private readonly Func<string, Result<(T, string), string>> _parse;

    public Parser(Func<string, Result<(T, string), string>> parse)
    {
        _parse = parse;
    }

    public Result<(T, string), string> Parse(string input) => _parse(input);

    public Parser<U> Select<U>(Func<T, U> f) where U : notnull
    {
        return new Parser<U>(input =>
        {
            var result = _parse(input);
            return result.Map(t => (f(t.Item1), t.Item2));
        });
    }

    public Parser<U> SelectMany<U>(Func<T, Parser<U>> f) where U : notnull
    {
        return new Parser<U>(input =>
        {
            var result1 = _parse(input);
            if (result1.IsErr)
                return Result.Err<(U, string), string>(result1.UnwrapErr());

            var (value1, remaining1) = result1.Unwrap();
            var parser2 = f(value1);
            return parser2.Parse(remaining1);
        });
    }

    public Parser<T> Or(Parser<T> other)
    {
        return new Parser<T>(input =>
        {
            var result1 = _parse(input);
            return result1.IsOk ? result1 : other.Parse(input);
        });
    }
}
