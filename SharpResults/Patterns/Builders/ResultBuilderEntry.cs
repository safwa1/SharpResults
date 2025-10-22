using System.Runtime.CompilerServices;
using SharpResults.Types;

namespace SharpResults.Patterns.Builders;

public static class ResultBuilderEntry
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultBuilder<T, TErr> BeginValidation<T, TErr>(this T self)
        where T : notnull where TErr : notnull
        => new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, IReadOnlyList<TErr>> Validate<T, TErr>(
        this T self,
        Func<ResultBuilder<T, TErr>, ResultBuilder<T, TErr>> build)
        where T : notnull where TErr : notnull
    {
        var builder = new ResultBuilder<T, TErr>();
        return build(builder).Build(self);
    }
}