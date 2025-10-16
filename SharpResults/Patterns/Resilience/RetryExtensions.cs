using SharpResults.Types;

namespace SharpResults.Patterns.Resilience;

public static class ResilienceExtensions
{
    /// <summary>
    /// Retry an operation with exponential backoff
    /// </summary>
    public static async Task<Result<T, Exception>> RetryAsync<T>(
        this Func<Task<Result<T, Exception>>> operation,
        int maxAttempts = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0)
        where T : notnull
    {
        var delay = initialDelay ?? TimeSpan.FromMilliseconds(100);
        Result<T, Exception> lastResult = default;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            lastResult = await operation();
            
            if (lastResult.IsOk)
                return lastResult;

            if (attempt < maxAttempts - 1)
            {
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * backoffMultiplier);
            }
        }

        return lastResult;
    }
}