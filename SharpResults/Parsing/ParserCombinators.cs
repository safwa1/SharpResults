using SharpResults.Core;

namespace SharpResults.Parsing;

public static class ParserCombinators
{
    public static Parser<char> Char(char c)
    {
        return new Parser<char>(input =>
        {
            if (string.IsNullOrEmpty(input))
                return Result.Err<(char, string), string>("Unexpected end of input");

            if (input[0] == c)
                return Result.Ok<(char, string), string>((c, input[1..]));

            return Result.Err<(char, string), string>($"Expected '{c}' but got '{input[0]}'");
        });
    }

    public static Parser<string> String(string s)
    {
        return new Parser<string>(input =>
        {
            if (input.StartsWith(s))
                return Result.Ok<(string, string), string>((s, input[s.Length..]));

            return Result.Err<(string, string), string>($"Expected '{s}'");
        });
    }
}