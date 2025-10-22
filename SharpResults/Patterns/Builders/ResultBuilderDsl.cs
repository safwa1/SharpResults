using System.Runtime.CompilerServices;
using SharpResults.Core;

namespace SharpResults.Patterns.Builders;

public static class ResultBuilderDsl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultBuilder<T, TErr> Require<T, TErr>(
        this ResultBuilder<T, TErr> builder,
        Func<T, bool> predicate,
        TErr error)
        where T : notnull where TErr : notnull
    {
        return builder.With(obj =>
            predicate(obj)
                ? Result.Ok<T, TErr>(obj)
                : Result.Err<T, TErr>(error)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultBuilder<T, TErr> Require<T, TErr>(
        this ResultBuilder<T, TErr> builder,
        Func<T, bool> predicate,
        Func<TErr> errorFactory)
        where T : notnull where TErr : notnull
    {
        return builder.With(obj =>
            predicate(obj)
                ? Result.Ok<T, TErr>(obj)
                : Result.Err<T, TErr>(errorFactory())
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultBuilder<T, TErr> Transform<T, TErr>(
        this ResultBuilder<T, TErr> builder,
        Func<T, T> transformer)
        where T : notnull where TErr : notnull
    {
        return builder.With(obj => Result.Ok<T, TErr>(transformer(obj)));
    }
}