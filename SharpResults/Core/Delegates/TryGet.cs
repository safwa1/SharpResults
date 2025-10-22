using System.Diagnostics.CodeAnalysis;

namespace SharpResults.Core.Delegates;

/// <summary>
/// Delegate that represents a fallible attempt to get a value of a given type.
/// </summary>
/// <typeparam name="T">The type of value to get.</typeparam>
/// <param name="value">The value retrieved, if any.</param>
/// <returns><c>true</c> if the value was retrieved, otherwise <c>false</c>.</returns>
public delegate bool TryGet<T>([MaybeNullWhen(false)] out T? value);