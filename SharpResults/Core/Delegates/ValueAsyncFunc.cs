namespace SharpResults.Core.Delegates;

/// <summary>
/// Represents an async operation that returns ValueTask{T}.
/// </summary>
public delegate ValueTask<T> ValueAsyncFunc<T>() where T : notnull;

public delegate ValueTask<T> ValueAsyncFunc<T, in TParam>(TParam param) where T : notnull;