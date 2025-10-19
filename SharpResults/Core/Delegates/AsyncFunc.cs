namespace SharpResults.Core.Delegates;

/// <summary>
/// Represents an async operation that returns Task{T}.
/// </summary>
public delegate Task<T> AsyncFunc<T>() where T : notnull;

public delegate Task<T> AsyncFunc<T, in TParam>(TParam param) where T : notnull;