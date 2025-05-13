namespace SharpResults.Types;

/// <summary>
/// Represents a void or unit type, commonly used as a placeholder for operations
/// that return no meaningful value. Semantically equivalent to <c>void</c> in expressions.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// The singleton instance of <see cref="Unit"/>.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Always returns <c>true</c>, as all <see cref="Unit"/> instances are considered equal.
    /// </summary>
    /// <param name="other">Another <see cref="Unit"/> instance.</param>
    /// <returns><c>true</c>.</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Unit"/>.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="Unit"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Gets the hash code for this instance. Always returns <c>0</c>.
    /// </summary>
    /// <returns><c>0</c>.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns the string representation of this <see cref="Unit"/>: <c>"()"</c>.
    /// </summary>
    /// <returns>The string <c>"()"</c>.</returns>
    public override string ToString() => "()";

    /// <summary>
    /// Determines whether two <see cref="Unit"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Unit"/> to compare.</param>
    /// <param name="right">The second <see cref="Unit"/> to compare.</param>
    /// <returns><c>true</c>.</returns>
    public static bool operator ==(Unit left, Unit right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Unit"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Unit"/> to compare.</param>
    /// <param name="right">The second <see cref="Unit"/> to compare.</param>
    /// <returns><c>false</c>.</returns>
    public static bool operator !=(Unit left, Unit right) => !(left == right);
}
