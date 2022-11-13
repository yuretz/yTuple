using System.Linq.Expressions;
using System.Reflection;

namespace yTuple;

public abstract record Symbol(string Name)
{
    public override string ToString() => Name;
}

internal record Var(string Name): Symbol(Name);

public abstract record Op(string Name, int Arity): Symbol(Name)
{
    public abstract Expression Parse(params Expression[] arguments);

    public virtual Delegate Run 
    {
        get 
        {
            if (_run is null)
            {
                var arguments = Enumerable
                    .Range(0, Arity)
                    .Select(_ => Expression.Parameter(typeof(object)))
                    .ToArray();

                _run = Expression.Lambda(Parse(arguments), arguments).Compile();
            }

            return _run;
        }
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

internal record Label(): Symbol("label");

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
