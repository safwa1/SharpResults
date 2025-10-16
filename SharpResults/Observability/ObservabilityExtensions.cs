using SharpResults.Types;

namespace SharpResults.Observability;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Track Option operations with telemetry
    /// </summary>
    public static Option<T> Trace<T>(
        this Option<T> option,
        Action<bool, T?>? onComplete = null)
        where T : notnull
    {
        var isSome = option.WhenSome(out var value);
        onComplete?.Invoke(isSome, value);
        return option;
    }
    
    public static Option<T> Trace<T>(
        this Option<T> option,
        string operationName,
        Action<string, bool, T?>? onComplete = null)
        where T : notnull
    {
        var isSome = option.WhenSome(out var value);
        onComplete?.Invoke(operationName, isSome, value);
        return option;
    }


    /// <summary>
    /// Measure execution time of Result operations
    /// </summary>
    public static async Task<Result<T, TErr>> MeasureAsync<T, TErr>(
        this Task<Result<T, TErr>> resultTask,
        Action<TimeSpan, bool> onComplete)
        where T : notnull
        where TErr : notnull
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = await resultTask;
        sw.Stop();
        onComplete(sw.Elapsed, result.IsOk);
        return result;
    }
}