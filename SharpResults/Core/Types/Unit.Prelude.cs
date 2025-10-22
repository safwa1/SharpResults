using System.Runtime.CompilerServices;
using SharpResults.Core.Types;

namespace SharpResults;

public static partial class Prelude
{
    /// <summary>
    /// Unit constructor
    /// </summary>
    public static Unit Unit =>
        Unit.Default;

    /// <summary>
    /// Takes any value, ignores it, returns a unit
    /// </summary>
    /// <param name="anything">Value to ignore</param>
    /// <returns>Unit</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit Ignore<A>(this A anything) =>
        default;
}
