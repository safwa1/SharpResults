using System.Runtime.Serialization;

namespace SharpResults.Exceptions;

[Serializable]
public sealed class ResultUnwrapErrException : Exception
{
    public ResultUnwrapErrException() : base("Attempted to unwrap the error of a Result that was in the Ok state.")
    {
    }

    public ResultUnwrapErrException(string message) : base(message)
    {
    }

    public ResultUnwrapErrException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private ResultUnwrapErrException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}