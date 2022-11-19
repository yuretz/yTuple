using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static yTuple.Elementary;

namespace yTuple;

public static class Lisp
{
    public static Expression<Func<object?>> Parse(ITuple program) => 
        Expression.Lambda<Func<object?>>(Expression.Convert(ParseExpr(program, new()), typeof(object)));

    internal static readonly IEnumerable<object?> NilResult = Extensions.Empty;

    private static Expression ParseExpr(object? expression, Dictionary<Symbol, Expression> scope) => expression switch
    {
        // nil
        var value when Equals(value, nil) => Expression.Constant(NilResult),
        
        // quote
        (Quote, var value) => Expression.Constant(value is ITuple tuple ? tuple.ToEnumerable(true) : value),

        // operator
        ITuple value when value[0] is Op op => op.Parse(
            Enumerable.Range(1, value.Length - 1)
                .Select(index => ParseExpr(value[index], scope))
                .ToArray()),
        
        // cond
        ITuple tuple when Equals(tuple[0], cond) => ParseCond(tuple, scope),

        ITuple tuple when Equals(tuple[0], lambda) => ParseLambda(tuple, scope),
        
        // dynamic call
        ITuple tuple when tuple[0] is ITuple or Symbol => 
            Expression.Call(
                _apply,
                ParseExpr(tuple[0], scope),
                Expression.NewArrayInit(
                    typeof(object),
                    Enumerable.Range(1, tuple.Length - 1)
                        .Select(index => Expression.Convert(ParseExpr(tuple[index], scope), typeof(object)))
                        .ToArray())),

        ITuple tuple => throw new NotImplementedException($"Unknown function {tuple[0]}"),

        Symbol symbol => scope.TryGetValue(symbol, out var result) 
            ? result 
            : Expression.Constant(symbol),

        var value => Expression.Constant(value)
    };

    private static Expression ParseCond(ITuple tuple, Dictionary<Symbol, Expression> scope)
    {
        var checkResult = Expression.Variable(typeof(object));
        return Expression.Block(
            new[] { checkResult },
            tuple.ToEnumerable(false).Skip(1).Cast<ITuple>().Reverse().Aggregate(
                (Expression)Expression.Convert(ParseExpr(NilResult, scope), typeof(object)),
                (result, item) => ParseCondClause(result, item, checkResult, scope)));
    }

    private static Expression ParseCondClause(
        Expression otherwise, 
        ITuple clause, 
        ParameterExpression checkResult,
        Dictionary<Symbol, Expression> scope)
    {
        var items = clause.ToEnumerable(false).Select(item => ParseExpr(item, scope)).ToList();
        var check = Expression.Call(
            _isTrue, 
            Expression.Assign(
                checkResult, 
                Expression.Convert(items.FirstOrDefault(ParseExpr(NilResult, scope)), typeof(object))));
        var rest = Expression.Convert(Expression.Block(items.Skip(1).Prepend(checkResult)), typeof(object));
        return Expression.Condition(check, rest, otherwise);
    }

    private static Expression ParseLambda(ITuple tuple, Dictionary<Symbol, Expression> scope)
    {
        var items = tuple.ToEnumerable(false).Skip(1).ToList();
        var args = ((ITuple?)items
            .FirstOrDefault())
            ?.ToEnumerable(false)
            .Cast<Symbol>()
            .Select(item => new KeyValuePair<Symbol, ParameterExpression>(
                item, 
                Expression.Parameter(typeof(object), item.Name)))
            .ToList()
            ?? throw new InvalidOperationException("Cannot read lambda arguments");

        scope = new(args
            .Select(item => new KeyValuePair<Symbol, Expression>(item.Key, item.Value))
            .UnionBy(scope, item => item.Key));

        return Expression.Lambda(
            Expression.Block(items.Skip(1).Select(item => ParseExpr(item, scope))),
            args.Select(item => item.Value));
    }


    private static object? Apply(object? func, object?[] arguments) => func switch
    {
        Op op when op.Arity >= 0 => op.Run.DynamicInvoke(arguments),
        Op op when op.Arity < 0 => op.Run.DynamicInvoke(new[] { arguments }),
        Delegate dlg => dlg.DynamicInvoke(arguments),
        Symbol sym => throw new NotSupportedException($"{sym.Name} cannot be called dynamically"),
        _ => throw new NotImplementedException($"{func} is not a function")
    };

    private static bool IsFalse(object? value) => value is bool flag && !flag || value == NilResult;

    private static bool IsTrue(object? value) => !IsFalse(value);
    
    private static readonly MethodInfo _isTrue = typeof(Lisp)
        .GetMethod(nameof(IsTrue), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo _apply = typeof(Lisp)
        .GetMethod(nameof(Apply), BindingFlags.Static | BindingFlags.NonPublic)!;
}
