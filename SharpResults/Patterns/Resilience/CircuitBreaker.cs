using SharpResults.Core;
using SharpResults.Types;

namespace SharpResults.Patterns.Resilience;

/// <summary>
/// Circuit breaker pattern for Results
/// </summary>
public class CircuitBreaker<T> where T : notnull
{
    private int _failureCount;
    private DateTime _lastFailureTime;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private bool _isOpen;

    public CircuitBreaker(int failureThreshold = 5, TimeSpan? timeout = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout ?? TimeSpan.FromSeconds(60);
    }

    public async Task<Result<T, string>> ExecuteAsync(Func<Task<Result<T, Exception>>> operation)
    {
        if (_isOpen)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                _isOpen = false;
                _failureCount = 0;
            }
            else
            {
                return Result.Err<T, string>("Circuit breaker is open");
            }
        }

        var result = await operation();

        if (result.IsErr)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= _failureThreshold)
            {
                _isOpen = true;
            }

            return Result.Err<T, string>($"Operation failed: {result.UnwrapErr().Message}");
        }

        _failureCount = 0;
        return Result.Ok<T, string>(result.Unwrap());
    }
}