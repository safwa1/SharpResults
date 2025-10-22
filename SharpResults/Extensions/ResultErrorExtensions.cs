using System.Runtime.CompilerServices;
using SharpResults.Core.Types;

namespace SharpResults.Extensions;

public static class ResultErrorExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultError ToError(this string s) => new(s);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ResultError ToError(this Exception ex) => new(ex.Message, ex);
}