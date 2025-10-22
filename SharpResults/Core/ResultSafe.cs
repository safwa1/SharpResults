using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using SharpResults.Core.Types;
using SharpResults.Types;

namespace SharpResults.Core;

/// <summary>
/// Provides safe execution helpers for Result transformations, ensuring that
/// any exceptions are automatically captured and converted into a typed error.
/// Optimized for performance using cached delegate factories.
/// </summary>
internal static class ResultSafe
{
    /// <summary>
    /// Caches error factory delegates per TErr to avoid repeated reflection or activator calls.
    /// </summary>
    private static class ErrorFactoryCache<TErr>
        where TErr : notnull
    {
        public static readonly Func<Exception, TErr> Create = BuildFactory();

        private static Func<Exception, TErr> BuildFactory()
        {
            var errType = typeof(TErr);

            // Case 1: TErr is a string
            if (errType == typeof(string))
                return ex => (TErr)(object)ex.Message;

            // Case 2: TErr derives from Exception
            if (typeof(Exception).IsAssignableFrom(errType))
                return ex => (TErr)(object)ex;
            
            // Case 3: TErr is ResultError
            if (errType == typeof(ResultError))
                return ex => (TErr)(object)new ResultError(ex.Message, ex);

            // Case 4: TErr has a constructor that accepts a string
            var ctor = errType.GetConstructor([typeof(string)]);
            if (ctor != null)
            {
                // Build an optimized expression-based delegate
                var exParam = Expression.Parameter(typeof(Exception), "ex");
                var messageProp = Expression.Property(exParam, nameof(Exception.Message));
                var newExpr = Expression.New(ctor, messageProp);
                return Expression.Lambda<Func<Exception, TErr>>(newExpr, exParam).Compile();
            }

            // Case 5: Fallback — cannot construct TErr, return a descriptive message if possible
            return ex => throw new InvalidOperationException(
                $"Cannot construct error of type {errType.FullName} from exception {ex.GetType().Name}.");
        }
    }

    /// <summary>
    /// Executes a function safely, returning an Ok result if successful,
    /// or an Err result if an exception is thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T, TErr> TryInvoke<T, TErr>(Func<T> func)
        where T : notnull
        where TErr : notnull
    {
        try
        {
            return Result.Ok<T, TErr>(func());
        }
        catch (Exception ex)
        {
            return Result.Err<T, TErr>(ErrorFactoryCache<TErr>.Create(ex));
        }
    }

    /// <summary>
    /// Executes an action safely, returning an Ok(Unit) or Err depending on exception state.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, TErr> TryInvokeAction<TErr>(Action action)
        where TErr : notnull
    {
        try
        {
            action();
            return Result.Ok<Unit, TErr>(Unit.Default);
        }
        catch (Exception ex)
        {
            return Result.Err<Unit, TErr>(ErrorFactoryCache<TErr>.Create(ex));
        }
    }

    /// <summary>
    /// Async-safe version for async workflows (Task-based).
    /// </summary>
    public static async Task<Result<T, TErr>> TryInvokeAsync<T, TErr>(Func<Task<T>> func)
        where T : notnull
        where TErr : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Result.Ok<T, TErr>(value);
        }
        catch (Exception ex)
        {
            return Result.Err<T, TErr>(ErrorFactoryCache<TErr>.Create(ex));
        }
    }

    /// <summary>
    /// Async-safe version for ValueTask-based workflows.
    /// </summary>
    public static async Task<Result<T, TErr>> TryValueInvokeAsync<T, TErr>(Func<ValueTask<T>> func)
        where T : notnull
        where TErr : notnull
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Result.Ok<T, TErr>(value);
        }
        catch (Exception ex)
        {
            return Result.Err<T, TErr>(ErrorFactoryCache<TErr>.Create(ex));
        }
    }
}
