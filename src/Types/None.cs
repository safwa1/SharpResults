namespace SharpResults.Types;


public interface IOption;

/// <summary>
/// Represents the absence of a value for Option.
/// </summary>
public readonly struct None
{
    public override string ToString() => "None";
}
