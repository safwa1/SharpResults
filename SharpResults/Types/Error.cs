namespace SharpResults.Types;

public sealed record Error(string? Value)
{
    public Exception? Exception { get; }
    public Error(Exception error) : this(error.Message)
    {
        Exception = error;
    }

    public static implicit operator string?(Error value) => value.Value;
    public static implicit operator Error(string value) => new (value);
    public static implicit operator Error(Exception value) => new (value);
    
    public override string ToString() => Value ?? Exception?.Message ?? "<unknown error>";
    
    public static Error From(Exception error) => error.Message;
    public static Error From(string message) => new Exception(message);
}