using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static yTuple.Elementary;

namespace yTuple;


public static class Lisp
{
    public static Expression<Func<object?>> Parse(ITuple program) => 
        Expression.Lambda<Func<object?>>(Expression.Convert(ParseExpr(program), typeof(object)));

    private static Expression ParseExpr(object? expression) => expression switch
    {
        var value when Equals(value, nil) => Expression.Constant(_nilResult),

        (atom, ITuple value) => Expression.Not(Expression.TypeIs(ParseExpr(value), typeof(IEnumerable<object>))),        

        (car, ITuple value) => Expression.Call(
            _first, 
            Expression.Convert(ParseExpr(value), typeof(IEnumerable<object>))),

        (cdr, ITuple value) => Expression.Call(
            _skip, 
            Expression.Convert(ParseExpr(value), typeof(IEnumerable<object>)), 
            Expression.Constant(1)),

        (cons, var left, var right) => Expression.Call(
            _prepend, 
            Expression.Convert(ParseExpr(right), typeof(IEnumerable<object>)),
            Expression.Convert(ParseExpr(left), typeof(object))),

        (quote, var value) => Expression.Constant(value is ITuple tuple ? tuple.ToEnumerable() : value),

        (eq, ITuple left, ITuple right) => Expression.Equal(ParseExpr(left), ParseExpr(right)),

        (var name, _) => throw new NotImplementedException($"Unknown function {name}"),

        var value => Expression.Constant(value)
    };

    private static object? First(IEnumerable<object?> items) => items.FirstOrDefault(_nilResult);
    
    private static IEnumerable<object?> _nilResult = Enumerable.Empty<object?>();

    private static MethodInfo _first = typeof(Lisp)
        .GetMethod("First", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static MethodInfo _skip = typeof(Enumerable)
        .GetMethod("Skip", BindingFlags.Static | BindingFlags.Public)!
        .MakeGenericMethod(typeof(object));

    private static MethodInfo _prepend = typeof(Enumerable)
        .GetMethod("Prepend", BindingFlags.Static | BindingFlags.Public)!
        .MakeGenericMethod(typeof(object));
}
