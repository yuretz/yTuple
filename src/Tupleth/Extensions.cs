using System.Collections;
using System.Runtime.CompilerServices;

namespace Tupleth;

internal static class Extensions
{
    public static IEnumerable<object?> ToEnumerable(this ITuple tuple)
    {
        for(var i = 0; i < tuple.Length; i++)
        {
            yield return tuple[i] is ITuple nested ? nested.ToEnumerable() : tuple[i];
        }
    }
}
