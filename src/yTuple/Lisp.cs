using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static yTuple.Elementary;

namespace yTuple;

public static class Lisp
{
    public static Expression<Func<object?>> Parse(ITuple program) => 
        Expression.Lambda<Func<object?>>(Expression.Convert(ParseExpr(program), typeof(object)));

    internal static IEnumerable<object?> NilResult = Extensions.Empty;

    private static Expression ParseExpr(object? expression) => expression switch
    {
        var value when Equals(value, nil) => Expression.Constant(NilResult),
        
        (Quote, var value) => Expression.Constant(value is ITuple tuple ? tuple.ToEnumerable(true) : value),

        ITuple value when value[0] is Operator op => op.Parse(
            Enumerable.Range(1, value.Length - 1)
                .Select(index => ParseExpr(value[index]))
                .ToArray()),

        ITuple tuple when Equals(tuple[0], cond) => ParseCond(tuple),
        
        ITuple tuple when tuple[0] is ITuple func => 
            Expression.Call(
                _apply,
                ParseExpr(func),
                Expression.NewArrayInit(
                    typeof(object),
                    Enumerable.Range(1, tuple.Length - 1)
                        .Select(index => ParseExpr(tuple[index]))
                        .ToArray())),
        
        ITuple tuple => throw new NotImplementedException($"Unknown function {tuple[0]}"),

        var value => Expression.Constant(value)
    };

    private static Expression ParseCond(ITuple tuple)
    {
        var checkResult = Expression.Variable(typeof(object));
        return Expression.Block(
            new[] { checkResult },
            tuple.ToEnumerable(false).Skip(1).Cast<ITuple>().Reverse().Aggregate(
                (Expression)Expression.Convert(ParseExpr(NilResult), typeof(object)),
                (result, item) => ParseCondClause(result, item, checkResult)));
    }

    private static Expression ParseCondClause(Expression otherwise, ITuple clause, ParameterExpression checkResult)
    {
        var items = clause.ToEnumerable(false).Select(ParseExpr).ToList();
        var check = Expression.Call(_isTrue, Expression.Assign(checkResult, Expression.Convert(items.FirstOrDefault(ParseExpr(NilResult)), typeof(object))));
        var rest = Expression.Convert(Expression.Block(items.Skip(1).Prepend(checkResult)), typeof(object));
        return Expression.Condition(check, rest, otherwise);
    }

    private static object? Apply(object? func, object?[] arguments) => func switch
    {
        Operator op => op.Run.DynamicInvoke(arguments),
        Symbol sym => throw new NotSupportedException($"{sym.Name} cannot be called dynamically"),
        _ => throw new NotImplementedException($"{func} is not a function")
    };

    private static bool IsFalse(object? value) => value is bool flag && !flag || value == NilResult;

    private static bool IsTrue(object? value) => !IsFalse(value);
    
    private static MethodInfo _isTrue = typeof(Lisp)
        .GetMethod("IsTrue", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static MethodInfo _apply = typeof(Lisp)
        .GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic)!;
}
