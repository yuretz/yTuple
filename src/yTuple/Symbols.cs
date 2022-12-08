using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

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

    protected virtual Func<object?[], object?> GetRun()
    {
        var args = Expression.Parameter(typeof(object?[]), "args");
        
        var argVars = Enumerable
            .Range(0, Arity)
            .Select(_ => Expression.Parameter(typeof(object)))
            .ToArray();

        return Expression.Lambda<Func<object?[], object?>>(
            Expression.Block(
                argVars,
                argVars.Select((item, index) => Expression.Assign(item, Expression.ArrayAccess(args, Expression.Constant(index))))
                    .Append(Parse(argVars))),
            args).Compile();
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

internal record Else(): Symbol("else");

internal record Begin(): Symbol("begin");

internal record Atom(): Op("atom", 1)
{
    public override Expression Parse(params Expression[] arguments) => 
        Expression.Not(Expression.TypeIs(arguments[0], typeof(IEnumerable<object?>)));
}

internal record Car() : Op("car", 1)
{
    public override Expression Parse(params Expression[] arguments) =>
        Expression.Call(
                _first,
                Expression.Convert(arguments[0], typeof(IEnumerable<object?>)),
                Expression.Constant(Lisp.NilResult));

    private static readonly MethodInfo _first = 
        GetMethod((IEnumerable<object?> items) => items.FirstOrDefault(Lisp.NilResult));
}

internal record Cdr() : Op("cdr", 1)
{
    public override Expression Parse(params Expression[] arguments) =>
        Expression.Call(
                    _skip,
                    Expression.Convert(arguments[0], typeof(IEnumerable<object?>)),
                    Expression.Constant(1));

    private static readonly MethodInfo _skip =
        GetMethod((IEnumerable<object?> items) => items.Skip(1));
}

internal record Cons() : Op("cons", 2)
{
    public override Expression Parse(params Expression[] arguments) =>
        Expression.Call(
            _prepend,
            Expression.Convert(arguments[1], typeof(IEnumerable<object?>)),
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

    protected override Func<object?[], object?> GetRun()
    {
        var changeType = GetMethod((object value, Type type) => Convert.ChangeType(value, type));
        var typed = Types.Numeric
            .ToDictionary(
                type => type,
                type =>
                {
                    var item = Expression.Parameter(typeof(object));
                    var result = Expression.Parameter(typeof(object));
                    return Expression.Lambda<Func<object, object, object>>(
                        Expression.Convert(
                            Expression.MakeBinary(
                                Type, 
                                Expression.Convert(Expression.Call(changeType, result, Expression.Constant(type)), type), 
                                Expression.Convert(Expression.Call(changeType, item, Expression.Constant(type)), type)),
                            typeof(object)),
                        true,
                        result,
                        item).Compile();
                });

        var binary = (object? left, object? right) =>
            typed[Types.PromoteNumeric(
                left?.GetType() ?? throw new ArgumentNullException(),
                right?.GetType() ?? throw new ArgumentNullException())](left, right);

        return (object?[] args) => args.Length > 1 ? args.Aggregate(binary) : binary(Identity, args[0]);
    }

    private Expression MakeBinary(Expression left, Expression right)
    {
        var type = Types.PromoteNumeric(left.Type, right.Type);
        return Expression.MakeBinary(Type, Types.CoerceExpr(type, left), Types.CoerceExpr(type, right));
    }
}

internal record Add(): NumericOp("+", Expression.Constant(0), ExpressionType.Add);
internal record Sub(): NumericOp("-", Expression.Constant(0), ExpressionType.Subtract);
internal record Mul(): NumericOp("*", Expression.Constant(1), ExpressionType.Multiply);
internal record Div(): NumericOp("/", Expression.Constant(1), ExpressionType.Divide);

internal record LogicalBinaryOp(string Name, ConstantExpression Identity, ExpressionType Type) : Op(Name, -1)
{
    public override Expression Parse(params Expression[] arguments) => arguments.Length switch
    {
        0 => Identity,
        1 => MakeBinary(Identity, arguments[0]),
        _ => arguments.Aggregate(MakeBinary)
    };

    protected override Func<object?[], object?> GetRun()
    {
        var left = Expression.Parameter(typeof(object));
        var right = Expression.Parameter(typeof(object));
        var binary = Expression.Lambda<Func<object?, object?, object>>(
            Types.BoxExpr(
                Expression.MakeBinary(
                    Type,
                    Expression.Call(_isTrue, left),
                    Expression.Call(_isTrue, right))),
            true,
            left,
            right
            ).Compile();

        return (object?[] arguments) => arguments.Length switch
        {
            0 => Identity.Value,
            1 => binary(Identity.Value, arguments[0]),
            _ => arguments.Aggregate(binary)
        };
    }

    private Expression MakeBinary(Expression left, Expression right) =>
        Expression.MakeBinary(
            Type, 
            left.Type == typeof(bool) ? left : Expression.Call(_isTrue, Types.BoxExpr(left)), 
            right.Type == typeof(bool) ? right : Expression.Call(_isTrue, Types.BoxExpr(right)));

    private static readonly MethodInfo _isTrue = GetMethod((object item) => Types.IsTrue(item));
}

internal record And(): LogicalBinaryOp("and", Expression.Constant(true), ExpressionType.And);
internal record Or(): LogicalBinaryOp("or", Expression.Constant(false), ExpressionType.Or);

internal record Not() : Op("not", 1)
{
    public override Expression Parse(params Expression[] arguments) => 
        Expression.Call(_isFalse, Expression.Convert(arguments[0], typeof(object)));

    private static readonly MethodInfo _isFalse = GetMethod((object item) => Types.IsFalse(item));
}

internal record Comparison(string Name, ExpressionType Type): Op(Name, -1)
{
    public override Expression Parse(params Expression[] arguments) => 
        arguments.Aggregate(
            (Result: (Expression)Expression.Constant(true), Previous: default(Expression)),
            (result, item) =>
            {
                if(result.Previous is null)
                {
                    return (result.Result, Previous: item);
                }

                var numeric = Types.PromoteNumeric(result.Previous.Type, item.Type);
                return 
                (
                    Result: Expression.MakeBinary(ExpressionType.And, result.Result, MakeBinary(result.Previous, item)),
                    Previous: item
                );
            }).Result;

    protected override Func<object?[], object?> GetRun()
    {
        var changeType = GetMethod((object value, Type type) => Convert.ChangeType(value, type));

        var typed = Types.Numeric
            .ToDictionary(
                type => type,
                type =>
                {
                    var item = Expression.Parameter(typeof(object));
                    var result = Expression.Parameter(typeof(object));
                    return Expression.Lambda<Func<object?, object?, bool>>(
                            Expression.MakeBinary(
                                Type, 
                                Expression.Convert(Expression.Call(changeType, result, Expression.Constant(type)), type), 
                                Expression.Convert(Expression.Call(changeType, item, Expression.Constant(type)), type)),
                        true,
                        result,
                        item).Compile();
                });

        var compare = (object? left, object? right) =>
            typed[Types.PromoteNumeric(
                left?.GetType() ?? throw new ArgumentNullException(),
                right?.GetType() ?? throw new ArgumentNullException())](left, right);

        return (object?[] args) =>
        {
            object? previous = null;
            var first = false;
            foreach(var item in args)
            {
                if(!first)
                {
                    first = true;
                }
                else if(!compare(previous, item))
                {
                    return false;
                }
                
                previous = item;
            }
            return true;
        };
    }

    private Expression MakeBinary(Expression left, Expression right)
    {
        var type = Types.PromoteNumeric(left.Type, right.Type);
        return Expression.MakeBinary(Type, Types.CoerceExpr(type, left), Types.CoerceExpr(type, right));
    }
}

internal record Lt(): Comparison("<", ExpressionType.LessThan);
internal record Gt(): Comparison(">", ExpressionType.GreaterThan);
internal record Le(): Comparison("<=", ExpressionType.LessThanOrEqual);
internal record Ge(): Comparison(">=", ExpressionType.GreaterThanOrEqual);