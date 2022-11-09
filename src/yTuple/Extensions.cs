using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace yTuple;

public static class Extensions
{
    public static readonly IEnumerable<object?> Empty= Enumerable.Empty<object?>();

    public static IEnumerable<object?> ToEnumerable(this ITuple tuple, bool recurse)
    {
        for(var i = 0; i < tuple.Length; i++)
        {
            yield return tuple[i] switch
            {
                ITuple { Length: 0 } when recurse => Empty,
                ITuple nested when recurse => nested.ToEnumerable(recurse),
                _ => tuple[i]
            };
        }
    }

    public static ITuple ToTuple(this IEnumerable<object?> enumerable, bool recurse) =>
        ToTuple(enumerable.GetEnumerator(), recurse);

    public static ITuple ToTuple(IEnumerator<object?> enumerator, bool recurse)
    {
        var items = new List<object?>();

        while(items.Count < 7 && enumerator.MoveNext())
        {
            items.Add(recurse && enumerator.Current is IEnumerable<object> nested 
                ? ToTuple(nested, recurse) 
                : enumerator.Current);
        }

        if(items.Count == 7)
        {
            var rest = ToTuple(enumerator, recurse);
            if(rest.Length != 0)
            {
                items.Add(rest);
            }
        }

        if(items.Count == 0)
        {
            return default(ValueTuple);
        }

        var types = items.Select(item => item?.GetType() ?? typeof(object)).ToArray();
        return (ITuple)_types[items.Count - 1].MakeGenericType(types).GetConstructor(types)!.Invoke(items.ToArray());
    }

    private static readonly Type[] _types = new Type 
                              [8]
    {typeof(ValueTuple        < >        ),
     typeof(ValueTuple       < , >       ),
     typeof(ValueTuple      < , , >      ),
     typeof(ValueTuple     < , , , >     ),
     typeof(ValueTuple    < , , , , >    ),
     typeof(ValueTuple   < , , , , , >   ),
     typeof(ValueTuple  < , , , , , , >  ),
     typeof(ValueTuple < , , , , , , , > )};
     // it's Christmas soon
}
