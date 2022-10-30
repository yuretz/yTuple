using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Tupleth.Elementary;

namespace Tupleth;


public static class Lisp
{
    public static Expression<Func<object?>> Parse(ITuple program) => 
        Expression.Lambda<Func<object?>>(Expression.Convert(ParseExpr(program), typeof(object)));

    private static Expression ParseExpr(object? expression) => expression switch
    {
        (atom, ITuple value) => Expression.Not(Expression.TypeIs(ParseExpr(value), typeof(IEnumerable<object>))),

        (car, var value) when Equals(value, nil) => Expression.Constant(_nilResult),

        (car, ITuple value) => Expression.Call(
            _first, 
            Expression.Convert(ParseExpr(value), typeof(IEnumerable<object>))),

        (cdr, var value) when Equals(value, nil) => Expression.Constant(_nilResult),

        (cdr, ITuple value) => Expression.Call(
            _skip, 
            Expression.Convert(ParseExpr(value), typeof(IEnumerable<object>)), 
            Expression.Constant(1)),

        (cons, var left, var right) => Expression.Call(
            _prepend, 
            Expression.Convert(ParseExpr(right), typeof(IEnumerable<object>)),
            ParseExpr(left)),

        (quote, var value) => Expression.Constant(value is ITuple tuple ? tuple.ToEnumerable() : value),

        (eq, ITuple left, ITuple right) => Expression.Equal(ParseExpr(left), ParseExpr(right)),

        (var name, _) => throw new NotImplementedException($"Unknown function {name}"),

        var value => Expression.Constant(value)
    };

    private static object? First(IEnumerable<object?> items) => items.FirstOrDefault(Enumerable.Empty<object?>());
    
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
