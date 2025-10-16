using System.Runtime.Serialization;

namespace SharpResults.Exceptions;

[Serializable]
public sealed class ResultExpectErrException : Exception
{
    public ResultExpectErrException() : base("Attempted to unwrap the error of a Result that was in the Ok state.")
    {
    }

    public ResultExpectErrException(string message) : base(message)
    {
    }

    public ResultExpectErrException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private ResultExpectErrException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}