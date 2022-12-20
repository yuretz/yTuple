using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using static yTuple.Elementary;

namespace yTuple;

public static partial class Lisp
{
    public static Expression<Func<object?>> Parse(Func<ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));
        return Parse(program());
    }
        

    public static Expression<Func<object?, object?>> Parse(Func<Symbol, ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var v0 = Declare()[0];
        var p0 = Expression.Parameter(typeof(object));
        
        return Expression.Lambda<Func<object?, object?>>(
            Types.BoxExpr(ParseExpr(program(v0), new() { { v0, p0 } })),
            true,
            p0);
    }

    public static Expression<Func<object?, object?, object?>> Parse(Func<Symbol, Symbol, ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var (v0, v1) = Declare();
        var p0 = Expression.Parameter(typeof(object));
        var p1 = Expression.Parameter(typeof(object));
        
        return Expression.Lambda<Func<object?, object?, object?>>(
            Types.BoxExpr(ParseExpr(
                program(v0, v1), 
                new() 
                {
                    { v0, p0 },
                    { v1, p1 },
                })),
            true,
            p0, p1);
    }

    public static Expression<Func<object?, object?, object?, object?>> Parse(Func<Symbol, Symbol, Symbol, ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var (v0, v1, v2) = Declare();
        var p0 = Expression.Parameter(typeof(object));
        var p1 = Expression.Parameter(typeof(object));
        var p2 = Expression.Parameter(typeof(object));

        return Expression.Lambda<Func<object?, object?, object?, object?>>(
            Types.BoxExpr(ParseExpr(
                program(v0, v1, v2),
                new()
                {
                    { v0, p0 },
                    { v1, p1 },
                    { v2, p2 },
                })),
            true,
            p0, p1, p2);
    }

    public static Expression<Func<object?, object?, object?, object?, object?>> Parse(
        Func<Symbol, Symbol, Symbol, Symbol, ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var (v0, v1, v2, v3) = Declare();
        var p0 = Expression.Parameter(typeof(object));
        var p1 = Expression.Parameter(typeof(object));
        var p2 = Expression.Parameter(typeof(object));
        var p3 = Expression.Parameter(typeof(object));

        return Expression.Lambda<Func<object?, object?, object?, object?, object?>>(
            Types.BoxExpr(ParseExpr(
                program(v0, v1, v2, v3),
                new()
                {
                    { v0, p0 },
                    { v1, p1 },
                    { v2, p2 },
                    { v3, p3 },
                })),
            true,
            p0, p1, p2, p3);
    }

    public static Expression<Func<object?, object?, object?, object?, object?, object?>> Parse(
        Func<Symbol, Symbol, Symbol, Symbol, Symbol, ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var (v0, v1, v2, v3, v4) = Declare();
        var p0 = Expression.Parameter(typeof(object));
        var p1 = Expression.Parameter(typeof(object));
        var p2 = Expression.Parameter(typeof(object));
        var p3 = Expression.Parameter(typeof(object));
        var p4 = Expression.Parameter(typeof(object));

        return Expression.Lambda<Func<object?, object?, object?, object?, object?, object?>>(
            Types.BoxExpr(ParseExpr(
                program(v0, v1, v2, v3, v4),
                new()
                {
                    { v0, p0 },
                    { v1, p1 },
                    { v2, p2 },
                    { v3, p3 },
                    { v4, p4 },
                })),
            true,
            p0, p1, p2, p3, p4);
    }

    public static Expression<Func<object?, object?, object?, object?, object?, object?, object?>> Parse(
        Func<Symbol, Symbol, Symbol, Symbol, Symbol, Symbol, ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var (v0, v1, v2, v3, v4, v5) = Declare();
        var p0 = Expression.Parameter(typeof(object));
        var p1 = Expression.Parameter(typeof(object));
        var p2 = Expression.Parameter(typeof(object));
        var p3 = Expression.Parameter(typeof(object));
        var p4 = Expression.Parameter(typeof(object));
        var p5 = Expression.Parameter(typeof(object));

        return Expression.Lambda<Func<object?, object?, object?, object?, object?, object?, object?>>(
            Types.BoxExpr(ParseExpr(
                program(v0, v1, v2, v3, v4, v5),
                new()
                {
                    { v0, p0 },
                    { v1, p1 },
                    { v2, p2 },
                    { v3, p3 },
                    { v4, p4 },
                    { v5, p5 },
                })),
            true,
            p0, p1, p2, p3, p4, p5);
    }

    public static Expression<Func<object?, object?, object?, object?, object?, object?, object?, object?>> Parse(
        Func<Symbol, Symbol, Symbol, Symbol, Symbol, Symbol, Symbol, ITuple> program)
    {
        ArgumentNullException.ThrowIfNull(program, nameof(program));

        var (v0, v1, v2, v3, v4, v5, v6) = Declare();
        var p0 = Expression.Parameter(typeof(object));
        var p1 = Expression.Parameter(typeof(object));
        var p2 = Expression.Parameter(typeof(object));
        var p3 = Expression.Parameter(typeof(object));
        var p4 = Expression.Parameter(typeof(object));
        var p5 = Expression.Parameter(typeof(object));
        var p6 = Expression.Parameter(typeof(object));

        return Expression.Lambda<Func<object?, object?, object?, object?, object?, object?, object?, object?>>(
            Types.BoxExpr(ParseExpr(
                program(v0, v1, v2, v3, v4, v5, v6),
                new()
                {
                    { v0, p0 },
                    { v1, p1 },
                    { v2, p2 },
                    { v3, p3 },
                    { v4, p4 },
                    { v5, p5 },
                    { v6, p6 },
                })),
            true,
            p0, p1, p2, p3, p4, p5, p6);
    }
}

