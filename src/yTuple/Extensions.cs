using System.Collections;
using System.Runtime.CompilerServices;

namespace yTuple;

public static class Extensions
{

    public static IEnumerable<object?> ToEnumerable(this ITuple tuple, bool recurse)
    {
        for(var i = 0; i < tuple.Length; i++)
        {
            yield return tuple[i] switch
            {
                ITuple { Length: 0 } when recurse => Lisp.NilResult,
                ITuple nested when recurse => nested.ToEnumerable(recurse),
                _ => tuple[i]
            };
        }
    }

}
