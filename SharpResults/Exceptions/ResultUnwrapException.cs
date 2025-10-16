using System.Runtime.Serialization;

namespace SharpResults.Exceptions; 

/// <summary>
/// Exception that is thrown when attempting to unwrap a <see cref="Result{T, TErr}"/> that is in the error state.
/// </summary>
[Serializable]
public sealed class ResultUnwrapException : Exception
{
    public ResultUnwrapException() : base("Attempted to unwrap a Result that was in the Err state.")
    {
    }

    public ResultUnwrapException(string message) : base(message)
    {
    }

    public ResultUnwrapException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private ResultUnwrapException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}