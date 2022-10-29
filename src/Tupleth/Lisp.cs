using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Tupleth.Elementary;

namespace Tupleth;


public static class Lisp
{
    public static Expression<Func<object?>> Parse(ITuple program) => 
        Expression.Lambda<Func<object?>>(ParseTuple(program));

    private static Expression ParseTuple(ITuple program) => program switch
    {
        (atom, ITuple value) => Expression.Not(Expression.TypeIs(ParseTuple(value), typeof(IEnumerable<object>))),

        (car, ITuple value) => Expression.Call(
            _first, 
            Expression.Convert(ParseTuple(value), typeof(IEnumerable<object>))),

        (cdr, ITuple value) => Expression.Call(
            _skip, 
            Expression.Convert(ParseTuple(value), typeof(IEnumerable<object>)), 
            Expression.Constant(1)),

        (cons, ITuple left, ITuple right) => Expression.Call(
            _prepend, 
            Expression.Convert(ParseTuple(right), typeof(IEnumerable<object>)),
            ParseTuple(left)),

        (quote, ITuple value) => Expression.Constant(value.ToEnumerable()),

        (eq, ITuple left, ITuple right) => Expression.Equal(ParseTuple(left), ParseTuple(right)),

        _ => throw new NotImplementedException()
    };

    private static MethodInfo _first = typeof(Enumerable)
        .GetMethods()
        .Where(item => item.Name == "First" && item.GetParameters().Length == 1)
        .First()
        .MakeGenericMethod(typeof(object));

    private static MethodInfo _skip = typeof(Enumerable)
        .GetMethod("Skip", BindingFlags.Static | BindingFlags.Public)!
        .MakeGenericMethod(typeof(object));

    private static MethodInfo _prepend = typeof(Enumerable)
        .GetMethod("Prepend", BindingFlags.Static | BindingFlags.Public)!
        .MakeGenericMethod(typeof(object));
}
