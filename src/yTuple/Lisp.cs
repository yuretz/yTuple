using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static yTuple.Elementary;

namespace yTuple;

public static class Lisp
{
    public static Expression<Func<object?>> Parse(ITuple program) => 
        Expression.Lambda<Func<object?>>(Expression.Convert(ParseExpr(program), typeof(object)));

    internal static IEnumerable<object?> NilResult = Enumerable.Empty<object?>();

    private static Expression ParseExpr(object? expression) => expression switch
    {
        var value when Equals(value, nil) => Expression.Constant(NilResult),

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

        (eq, var left, var right) => Expression.Call(
            _equals,
            Expression.Convert(ParseExpr(left), typeof(object)),
            Expression.Convert(ParseExpr(right), typeof(object))),

        ITuple tuple when Equals(tuple[0], cond) => ParseCond(tuple),
            
        ITuple tuple => throw new NotImplementedException($"Unknown function {tuple[0]}"),

        var value => Expression.Constant(value)
    };

    private static Expression ParseCond(ITuple tuple)
    {
        var checkResult = Expression.Variable(typeof(object));
        return Expression.Block(
            new[] { checkResult },
            tuple.ToEnumerable().Skip(1).Cast<IEnumerable<object?>>().Reverse().Aggregate(
                (Expression)Expression.Convert(ParseExpr(NilResult), typeof(object)),
                (result, item) => ParseCondClause(result, item, checkResult)));
    }

    private static Expression ParseCondClause(Expression otherwise, IEnumerable<object?> clause, ParameterExpression checkResult)
    {
        var items = clause.Select(ParseExpr).ToList();
        var check = Expression.Call(_isTrue, Expression.Assign(checkResult, Expression.Convert(items.FirstOrDefault(ParseExpr(NilResult)), typeof(object))));
        var rest = Expression.Convert(Expression.Block(items.Skip(1).Prepend(checkResult)), typeof(object));
        return Expression.Condition(check, rest, otherwise);
    }

    private static bool IsFalse(object? value) => value is bool flag && !flag || value == NilResult;

    private static bool IsTrue(object? value) => !IsFalse(value);

    private static object? First(IEnumerable<object?> items) => items.FirstOrDefault(NilResult);
    
    private static MethodInfo _isTrue = typeof(Lisp)
        .GetMethod("IsTrue", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static MethodInfo _first = typeof(Lisp)
        .GetMethod("First", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static MethodInfo _skip = typeof(Enumerable)
        .GetMethod("Skip", BindingFlags.Static | BindingFlags.Public)!
        .MakeGenericMethod(typeof(object));

    private static MethodInfo _prepend = typeof(Enumerable)
        .GetMethod("Prepend", BindingFlags.Static | BindingFlags.Public)!
        .MakeGenericMethod(typeof(object));

    private static MethodInfo _equals = typeof(object)
        .GetMethod("Equals", BindingFlags.Static | BindingFlags.Public)!;
}
