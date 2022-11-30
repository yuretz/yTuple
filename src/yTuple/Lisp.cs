using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static yTuple.Elementary;

namespace yTuple;

public static class Lisp
{
    public static Expression<Func<object?>> Parse(ITuple program) => 
        Expression.Lambda<Func<object?>>(Types.BoxExpr(ParseExpr(program, new())), true);

    internal static readonly IEnumerable<object?> NilResult = Types.Empty;

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

        // lambda
        ITuple tuple when Equals(tuple[0], lambda) => ParseLambda(tuple, scope),

        //// define
        //ITuple tuple when Equals(tuple[0], define) => ParseDefine(tuple, scope),
        
        // dynamic call
        ITuple tuple when tuple[0] is ITuple or Symbol =>
            Expression.Invoke(
                Types.CoerceExpr<Func<object[], object>>(ParseExpr(tuple[0], scope)),
                Expression.NewArrayInit(
                    typeof(object),
                    tuple.ToEnumerable(false)
                        .Skip(1)
                        .Select(item => Types.BoxExpr(ParseExpr(item, scope))))),

        ITuple tuple => throw new NotImplementedException($"Unknown function {tuple[0]}"),

        Op operation => Expression.Constant(operation.Run),

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
                Types.BoxExpr(ParseExpr(NilResult, scope)),
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
                Types.BoxExpr(items.FirstOrDefault(ParseExpr(NilResult, scope)))));
        var rest = Types.CoerceExpr<object>(Expression.Block(items.Skip(1).Prepend(checkResult)));
        return Expression.Condition(check, rest, Types.CoerceExpr<object>(otherwise));
    }

    private static Expression ParseLambda(ITuple tuple, Dictionary<Symbol, Expression> scope)
    {
        var items = tuple.ToEnumerable(false).Skip(1).ToList();
        var namedArgs = ((ITuple?)items
            .FirstOrDefault())
            ?.ToEnumerable(false)
            .Cast<Symbol>()
            .Select(item => new KeyValuePair<Symbol, ParameterExpression>(
                item, 
                Expression.Parameter(typeof(object), item.Name)))
            .ToList()
            ?? throw new InvalidOperationException("Cannot read lambda arguments");

        var args = Expression.Parameter(typeof(object[]), "args");

        var defines = items
            .Skip(1)
            .TakeWhile(item => item is ITuple tuple && tuple[0] is Define)
            .Cast<ITuple>()
            .Select(ParseDefine)
            .ToList();

        scope = new(
             
            defines
                // defines (shadowing args)
                .Select(item => new KeyValuePair<Symbol, Expression>(item.Symbol, item.Parameter))
                // args (shadowing outer scope)
                .Concat(namedArgs.Select(item => new KeyValuePair<Symbol, Expression>(item.Key, item.Value)))
                // outer scope
                .Concat(scope)   
                .DistinctBy(item => item.Key));

        return Expression.Lambda(
            Types.BoxExpr(
                Expression.Block(
                    // args + defines become block variables
                    namedArgs.Select(item => item.Value).Concat(defines.Select(item => item.Parameter)),
                    
                    namedArgs
                        // assign args
                        .Select((item, index) => Expression.Assign(
                            item.Value, 
                            Expression.ArrayAccess(args, Expression.Constant(index))))
                        // assign defines
                        .Concat(defines.Select(
                            item => Expression.Assign(
                                item.Parameter, 
                                Types.BoxExpr(ParseExpr(item.Init, scope)))))
                        // run the rest
                        .Concat(items.Skip(1 + defines.Count).Select(item => ParseExpr(item, scope))))),
            true,
            args);
    }

    private static (Symbol Symbol, object? Init, ParameterExpression Parameter) ParseDefine(ITuple tuple) => tuple switch
    {
        (Define, Symbol symbol) => (symbol, nil, Expression.Parameter(typeof(object), symbol.Name)),
        (Define, Symbol symbol, var value) => (symbol, value, Expression.Parameter(typeof(object), symbol.Name)),
        _ => throw new InvalidOperationException($"Invalid define {tuple}")
    };


    private static readonly MethodInfo _isTrue = typeof(Types)
        .GetMethod(nameof(Types.IsTrue), BindingFlags.Static | BindingFlags.Public)!;
}
