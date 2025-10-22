using SharpResults.Types;

namespace SharpResults.Patterns.Builders;

public static class ResultBuilderExtensions
{
    public static Result<T, IReadOnlyList<TErr>> BuildResult<T, TErr>(this T self, Action<ResultBuilder<T, TErr>> funcBuilder) where T : notnull where TErr : notnull
    {
        var builder = new ResultBuilder<T, TErr>();
        funcBuilder(builder);
        return builder.Build(self);
    }
    
    public static Task<Result<T, IReadOnlyList<TErr>>> BuildResultAsync<T, TErr>(this T self, Action<AsyncResultBuilder<T, TErr>> funcBuilder) where T : notnull where TErr : notnull
    {
        var builder = new AsyncResultBuilder<T, TErr>();
        funcBuilder(builder);
        return builder.BuildAsync(self);
    }
    
    public static Lazy<Result<T, IReadOnlyList<TErr>>> LazyBuildResult<T,TErr>(
        this T self,
        Action<ResultBuilder<T, TErr>> funcBuilder) where T : notnull where TErr : notnull
    {
        return new Lazy<Result<T, IReadOnlyList<TErr>>>(() =>
        {
            var builder = new ResultBuilder<T, TErr>();
            funcBuilder(builder);
            return builder.Build(self);
        });
    }

    public static Lazy<Task<Result<T, IReadOnlyList<TErr>>>> LazyBuildResultAsync<T, TErr>(
        this T self,
        Action<AsyncResultBuilder<T, TErr>> funcBuilder) where T : notnull where TErr : notnull
    {
        return new Lazy<Task<Result<T, IReadOnlyList<TErr>>>>(() =>
        {
            var builder = new AsyncResultBuilder<T, TErr>();
            funcBuilder(builder);
            return builder.BuildAsync(self);
        });
    }
}