using System.Numerics;
using SharpResults.Types;

namespace SharpResults.Extensions;

public static class NumberExtensions
{
    public static Option<T> Parse<T>(this string number) where T : INumber<T>
    {
        return Option.Parse<T>(number);
    }
}