using System.Diagnostics.CodeAnalysis;

namespace SharpResults.Core.Delegates;

/// <summary>
/// Delegate that represents a fallible attempt to get a value associated with a given key.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of value to get.</typeparam>
/// <param name="key">The key to retrieve the corresponding value for.</param>
/// <param name="value">The value retrieved, if any.</param>
/// <returns><c>true</c> if the value was retrieved, otherwise <c>false</c>.</returns>
public delegate bool TryGetValue<in TKey, TValue>(TKey key, [MaybeNullWhen(false)] out TValue? value);