﻿using System.Linq.Expressions;
using System.Reflection;

namespace yTuple;

public abstract record Symbol(string Name)
{
    public sealed override string ToString() => Name;
}

internal record Var(string Name): Symbol(Name);

public abstract record Op(string Name, int Arity): Symbol(Name)
{
    public abstract Expression Parse(params Expression[] arguments);

    public Delegate Run => _run ??= GetRun();

    protected virtual Delegate GetRun()
    {
        var arguments = Enumerable
            .Range(0, Arity)
            .Select(_ => Expression.Parameter(typeof(object)))
            .ToArray();

        return Expression.Lambda(Parse(arguments), arguments).Compile();
    }

    protected static MethodInfo GetMethod(LambdaExpression lambda)
    {
        ArgumentNullException.ThrowIfNull(lambda);
        return ((MethodCallExpression)lambda.Body).Method;
    }

    private Delegate? _run;
}

internal record Quote(): Symbol("quote");

internal record Cond(): Symbol("cond");

internal record Lambda(): Symbol("lambda");

internal record Define(): Symbol("define");

internal record Atom(): Op("atom", 1)
{
    public override Expression Parse(params Expression[] arguments) => 
        Expression.Not(Expression.TypeIs(arguments[0], typeof(IEnumerable<object>)));
}

internal record Car() : Op("car", 1)
{
    public override Expression Parse(params Expression[] arguments) =>
        Expression.Call(
                _first,
                Expression.Convert(arguments[0], typeof(IEnumerable<object>)),
                Expression.Constant(Lisp.NilResult));

    private static readonly MethodInfo _first = 
        GetMethod((IEnumerable<object?> items) => items.FirstOrDefault(Lisp.NilResult));
}

internal record Cdr() : Op("cdr", 1)
{
    public override Expression Parse(params Expression[] arguments) =>
        Expression.Call(
                    _skip,
                    Expression.Convert(arguments[0], typeof(IEnumerable<object>)),
                    Expression.Constant(1));

    private static readonly MethodInfo _skip =
        GetMethod((IEnumerable<object?> items) => items.Skip(1));
}

internal record Cons() : Op("cons", 2)
{
    public override Expression Parse(params Expression[] arguments) =>
        Expression.Call(
            _prepend,
            Expression.Convert(arguments[1], typeof(IEnumerable<object>)),
            Expression.Convert(arguments[0], typeof(object)));

    private static readonly MethodInfo _prepend =
        GetMethod((IEnumerable<object?> items, object element) => items.Prepend(element));
}

internal record Eq() : Op("eq", 2)
{
    public override Expression Parse(params Expression[] arguments) =>
        Expression.Call(
            _equals,
            Expression.Convert(arguments[0], typeof(object)),
            Expression.Convert(arguments[1], typeof(object)));

    private static readonly MethodInfo _equals =
        GetMethod((object left, object right) => Equals(left, right));
}

internal abstract record NumericOp(string Name, ConstantExpression Identity, ExpressionType Type) : Op(Name, -1)
{
    public override Expression Parse(params Expression[] arguments) => 
        arguments.Length > 1 
            ? arguments.Aggregate(MakeBinary) 
            : MakeBinary(Identity, arguments[0]);

    protected override Delegate GetRun()
    {
        _typed ??= Types.Numeric
            .ToDictionary(
                type => type,
                type =>
                {
                    var item = Expression.Parameter(typeof(object));
                    var result = Expression.Parameter(typeof(object));
                    return Expression.Lambda<Func<object, object, object>>(
                        Expression.Convert(
                            Expression.MakeBinary(Type, Expression.Convert(result, type), Expression.Convert(item, type)),
                            typeof(object)),
                        result,
                        item).Compile();
                });

        return (object[] args) =>
            args.Length > 1
                ? args.Aggregate(
                    (result, item) => _typed[Types.PromoteNumeric(result.GetType(), item.GetType())](result, item))
                : _typed[Types.PromoteNumeric(Identity.GetType(), args[0].GetType())](Identity, args[0]);
    }

    private Expression MakeBinary(Expression left, Expression right)
    {
        var type = Types.PromoteNumeric(left.Type, right.Type);
        left = left.Type == type ? left : Expression.Convert(left, type);
        right = right.Type == type ? right : Expression.Convert(right, type);
        return Expression.MakeBinary(Type, left, right);
    }

    private Dictionary<Type, Func<object, object, object>>? _typed;
}

internal record Add(): NumericOp("+", Expression.Constant(0), ExpressionType.Add);
internal record Sub(): NumericOp("-", Expression.Constant(0), ExpressionType.Subtract);
internal record Mul(): NumericOp("*", Expression.Constant(1), ExpressionType.Multiply);
internal record Div(): NumericOp("/", Expression.Constant(1), ExpressionType.Divide);
