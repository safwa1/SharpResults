using System.Runtime.CompilerServices;
using SharpResults.Patterns.Builders;

namespace SharpResults;

public static partial class Prelude
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultBuilder<T, TErr> ResultBuilderOf<T, TErr>() where T : notnull where TErr : notnull => new();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultBuilder<T, string> ResultBuilderOf<T>() where T : notnull => new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public static AsyncResultBuilder<T, TErr> AsyncResultBuilderOf<T, TErr>() where T : notnull where TErr : notnull => new();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public static AsyncResultBuilder<T, string> AsyncResultBuilderOf<T>() where T : notnull => new();
}
