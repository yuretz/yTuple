using System.Collections;
using System.Runtime.CompilerServices;

namespace yTuple;

public static class Extensions
{

    public static IEnumerable<object?> ToEnumerable(this ITuple tuple)
    {
        for(var i = 0; i < tuple.Length; i++)
        {
            yield return tuple[i] switch
            {
                ITuple { Length: 0 } => Lisp.NilResult,
                ITuple nested => nested.ToEnumerable(),
                _ => tuple[i]
            };
        }
    }

}
