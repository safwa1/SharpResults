using System.Runtime.CompilerServices;

namespace SharpResults.Core.Types;

public interface IResultError;

public readonly struct ResultError : IEquatable<ResultError>, IResultError
{
    public string Message { get; }
    public Exception? Exception { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ResultError(string? message, Exception? exception = null)
    {
        Message = message ?? exception?.Message ?? "<unknown error>";
        Exception = exception;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ResultError(string message) => new(message);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ResultError(Exception ex) => new(ex.Message, ex);

    public override string ToString()
        => Exception is null
            ? Message
            : $"{Message}: {Exception.GetType().Name}";

    public bool Equals(ResultError other)
        => Message == other.Message &&
           Exception?.GetType() == other.Exception?.GetType();

    public override bool Equals(object? obj)
        => obj is ResultError other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Message, Exception?.GetType());

    public static bool operator ==(ResultError left, ResultError right) => left.Equals(right);
    public static bool operator !=(ResultError left, ResultError right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out string message, out Exception? exception)
    {
        message = Message;
        exception = Exception;
    }
}