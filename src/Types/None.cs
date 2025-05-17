namespace SharpResults.Types;


public interface IOption;

/// <summary>
/// Represents the absence of a value for Option.
/// </summary>
public readonly struct None : IEquatable<None>
{
    /// <summary>
    /// The singleton instance of <see cref="None"/>.
    /// </summary>
    public static readonly None Value = new();

    /// <summary>
    /// Always returns <c>true</c>, as all <see cref="None"/> instances are considered equal.
    /// </summary>
    /// <param name="other">Another <see cref="None"/> instance.</param>
    /// <returns><c>true</c>.</returns>
    public bool Equals(None other) => true;

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="None"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="None"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is None;

    /// <summary>
    /// Gets the hash code for this instance. Always returns <c>0</c>.
    /// </summary>
    /// <returns><c>0</c>.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns the string representation of this <see cref="None"/>: <c>"None"</c>.
    /// </summary>
    /// <returns>The string <c>"None"</c>.</returns>
    public override string ToString() => nameof(None);

    /// <summary>
    /// Determines whether two <see cref="None"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="None"/> to compare.</param>
    /// <param name="right">The second <see cref="None"/> to compare.</param>
    /// <returns><c>true</c>.</returns>
    public static bool operator ==(None left, None right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="None"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="None"/> to compare.</param>
    /// <param name="right">The second <see cref="None"/> to compare.</param>
    /// <returns><c>false</c>.</returns>
    public static bool operator !=(None left, None right) => !(left == right);
}
