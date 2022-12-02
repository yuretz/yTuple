using System.Runtime.CompilerServices;

namespace yTuple.Tests;

public class LispTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData(42)]
    [InlineData("foo")]
    public void QuoteAtom(object? value) =>
        AssertResult(value, Run((quote, value)));

    [Fact]
    public void QuoteTuple1()
    {
        var value = default(ValueTuple);
        AssertResult(value, Run((quote, value)));
    }

    [Fact]
    public void QuoteTuple2()
    {
        var value = (1, 2, 3);
        AssertResult(value, Run((quote, value)));
    }

    [Fact]
    public void QuoteTuple3()
    {
        var value = ((1, "foo"), 3, (nil, 4, 5, ValueTuple.Create(true)));
        AssertResult(value, Run((quote, value)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(42)]
    [InlineData("foo")]
    public void CarAtom(object value)
    {
        Assert.ThrowsAny<Exception>(() => Run((car, value)));
    }

    [Fact]
    public void CarNil1()
    {
        AssertResult(nil, Run((car, nil)));
    }

    [Fact]
    public void CarNil2()
    {
        AssertResult(nil, Run((car, (quote, nil))));
    }

    [Fact]
    public void CarList1()
    {
        var value = (1, 2, 3);
        AssertResult(value.Item1, Run((car, (quote, value))));
    }

    [Fact]
    public void CarList2()
    {
        var value = ("foo", "bar", "baz");
        AssertResult(value.Item1, Run((car, (quote, value))));
    }

    [Fact]
    public void CarList3()
    {
        var value = (("foo", "bar"), "baz");
        AssertResult(value.Item1, Run((car, (quote, value))));
    }

    [Fact]
    public void CarList4()
    {
        var value = Tuple.Create(42);
        AssertResult(value.Item1, Run((car, (quote, value))));
    }

    [Fact]
    public void CdrNil1()
    {
        AssertResult(nil, Run((cdr, nil)));
    }

    [Fact]
    public void CdrNil2()
    {
        AssertResult(nil, Run((cdr, (quote, nil))));
    }
    [Fact]
    public void CdrList1()
    {
        var value = (1, 2, 3);
        AssertResult(value.ToEnumerable(true).Skip(1), Run((cdr, (quote, value))));
    }

    [Fact]
    public void CdrList2()
    {
        var value = ("foo", "bar", "baz");
        AssertResult(value.ToEnumerable(true).Skip(1), Run((cdr, (quote, value))));
    }

    [Fact]
    public void CdrList3()
    {
        var value = (("foo", "bar"), "baz");
        AssertResult(value.ToEnumerable(true).Skip(1), Run((cdr, (quote, value))));
    }

    [Fact]
    public void CdrList4()
    {
        var value = Tuple.Create(42);
        AssertResult(value.ToEnumerable(true).Skip(1), Run((cdr, (quote, value))));
    }

    [Fact]
    public void ConsNil()
    {
        var value = 42;
        AssertResult(ValueTuple.Create(value), Run((cons, value, nil)));
    }

    [Fact]
    public void ConsMany()
    {
        AssertResult((1, 2, 3), Run((cons, 1, (cons, 2, (cons, 3, nil)))));
    }

    [Fact]
    public void ConsQuote()
    {
        AssertResult((1, 2, 3, 4), Run((cons, 1, (quote, (2, 3, 4)))));
    }

    [Fact]
    public void ConsComplex()
    {
        AssertResult(((1, 2), 3, (4, 5)), Run((cons, (quote, (1, 2)), (cons, 3, (cons, (quote, (4, 5)), nil)))));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData(42)]
    [InlineData("foo")]
    public void EqAtom(object? value)
    {
        AssertResult(true, Run((eq, value, value)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData(42)]
    [InlineData("foo")]
    public void EqQuoteAtom(object? value)
    {
        AssertResult(true, Run((eq, value, (quote, value))));
    }

    [Fact]
    public void EqNil()
    {
        AssertResult(true, Run((eq, nil, nil)));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, 0)]
    [InlineData(null, "foo")]
    [InlineData(42, 41)]
    [InlineData("foo", "bar")]
    [InlineData("42", 42)]
    public void NotEqAtom(object? left, object? right)
    {
        AssertResult(false, Run((eq, left, right)));
    }

    [Fact]
    public void CondSimple()
    {
        for (var i = 0; i < 3; i++)
        {
            AssertResult(i, Run((cond, (i == 0, 0), (i == 1, 1), (i == 2, 2))));
        }
    }


    [Theory]
    [InlineData(true, "yes")]
    [InlineData(false, "no")]
    [InlineData(null, "yes")]
    [InlineData(42, "yes")]
    [InlineData("foo", "yes")]
    public void CondTruthyTests(object value, string result)
    {
        AssertResult(result, Run((cond, (value, "yes"), (@else, "no"))));
    }

    [Fact]
    public void CondNilTest1()
    {
        AssertResult("no", Run((cond, (nil, "yes"), (@else, "no"))));
    }

    [Fact]
    public void DynamicCall1()
    {
        AssertResult(1, Run(((cond, (1, car), (2, cdr)), (quote, (1, 2, 3)))));
    }

    [Fact]
    public void DynamicCall2()
    {
        AssertResult((2, 3), Run(((cond, ((eq, 42, 43), car), (@else, cdr)), (quote, (1, 2, 3)))));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData(42)]
    [InlineData("foo")]
    public void LambdaIdentity(object value)
    {
        var x = Declare("x");
        AssertResult(value, Run(((lambda, Single(x), x), value)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    [InlineData(42)]
    [InlineData("foo")]
    public void LambdaValue(object value)
    {
        AssertResult(value, Run(Single((lambda, nil, value))));
    }

    [Theory]
    [InlineData(42, 42, "yes")]
    [InlineData(42, 43, "no")]
    [InlineData(true, true, "yes")]
    [InlineData("foo", "bar", "no")]
    public void LambdaWithArgs(object left, object right, string result)
    {
        var (x, y) = Declare("x", "y");
        AssertResult(result, Run(
            ((lambda, (x, y),
                (cond,
                    ((eq, x, y), "yes"),
                    (@else, "no"))),
            left, right)));
    }

    [Theory]
    [InlineData(42)]
    [InlineData(42.5)]
    [InlineData(-20)]
    public void AddOne(object value)
    {
        AssertResult(value, Run((add, value)));
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1.5, 2, 3.5)]
    [InlineData(-1, 2.5, 1.5)]
    public void AddTwo(object left, object right, object result)
    {
        AssertResult(result, Run((add, left, right)));
    }

    [Fact]
    public void AddMany()
    {
        var values = new[] { 1, 2, 3, 4, 5, 15, -3, -7, 0, 42 };
        var program = values.Cast<object>().Prepend(add).ToTuple(false);
        AssertResult(values.Sum(), Run(program));
    }

    [Theory]
    [InlineData(42, -42)]
    [InlineData(42.5, -42.5)]
    [InlineData(-20, 20)]
    public void SubOne(object value, object result)
    {
        AssertResult(result, Run((sub, value)));
    }

    [Theory]
    [InlineData(1, 2, -1)]
    [InlineData(2, 1.5, 0.5)]
    [InlineData(-1, 2.5, -3.5)]
    public void SubTwo(object left, object right, object result)
    {
        AssertResult(result, Run((sub, left, right)));
    }

    [Fact]
    public void SubMany()
    {
        var values = new[] { 1, 2, 3, 4, 5, 15, -3, -7, 0, 42 };
        var program = values.Cast<object>().Prepend(sub).ToTuple(false);
        AssertResult(values[0] + values.Skip(1).Select(item => -item).Sum(), Run(program));
    }

    [Theory]
    [InlineData(42)]
    [InlineData(42.5)]
    [InlineData(-20)]
    public void MulOne(object value)
    {
        AssertResult(value, Run((mul, value)));
    }

    [Theory]
    [InlineData(1, 2, 2)]
    [InlineData(1.5, 2, 3.0)]
    [InlineData(-1, 2.5, -2.5)]
    public void MulTwo(object left, object right, object result)
    {
        AssertResult(result, Run((mul, left, right)));
    }

    [Fact]
    public void MulMany()
    {
        var values = new[] { 1, 2, 3, 4, 5, 15, -3, -7, 2, 42 };
        var program = values.Cast<object>().Prepend(mul).ToTuple(false);
        AssertResult(values.Aggregate((result, item) => result * item), Run(program));
    }

    [Theory]
    [InlineData(42, 1 / 42)]
    [InlineData(42.5, 1 / 42.5)]
    [InlineData(-20, -1 / 20)]
    public void DivOne(object value, object result)
    {
        AssertResult(result, Run((div, value)));
    }

    [Theory]
    [InlineData(1, 2, 1 / 2)]
    [InlineData(2, 1.5, 2 / 1.5)]
    [InlineData(-1, 2.5, -1 / 2.5)]
    public void DivTwo(object left, object right, object result)
    {
        AssertResult(result, Run((div, left, right)));
    }

    [Fact]
    public void DivMany()
    {
        var values = new[] { 1, 2, 3, 4, 5, 15, -3, -7, 0.1, 42 };
        var program = values.Cast<object>().Prepend(div).ToTuple(false);
        AssertResult(values.Aggregate((result, item) => result / item), Run(program));
    }

    [Theory]
    [InlineData(false, 9)]
    [InlineData(true, 24)]
    public void AddOrMulDynamic(bool flag, int result)
    {
        AssertResult(result, Run(((cond, (flag, mul), (!flag, add)), 2, 3, 4)));
    }

    [Theory]
    [InlineData(false, 6)]
    [InlineData(true, 3)]
    public void SubOrDivDynamic(bool flag, int result)
    {
        var x = Declare("x");
        AssertResult(result, Run(
            ((lambda, Single(x), (x, 9, 3)),
            (cond, (flag, div), (!flag, sub)))));
    }

    [Fact]
    public void DefineValue()
    {
        var (x, y) = Declare("x", "y");
        AssertResult(42, Run(((lambda, Single(x), (define, y, 1), (add, x, y)), 41)));
    }

    [Fact]
    public void DefineExpression()
    {
        var (x, y) = Declare("x", "y");
        AssertResult(42, Run(((lambda, Single(x), (define, y, (add, x, 1)), y), 41)));
    }

    [Fact]
    public void DefineExpressionLambda()
    {
        var (x, y, inc) = Declare("x", "y", "inc");
        AssertResult(
            42,
            Run(
                ((lambda, Single(x),
                    (define, inc, (lambda, Single(y), (add, y, 1))),
                    (inc, x)),
                41)
            ));

    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(42, false)]
    [InlineData(null, true)]
    public void NotValue(object value, bool result)
    {
        value ??= nil;
        AssertResult(result, Run((not, value)));
    }


    [Theory]
    [InlineData(new object?[] { }, true)]
    [InlineData(new object?[] { false }, false)]
    [InlineData(new object?[] { true }, true)]
    [InlineData(new object?[] { 42 }, true)]
    [InlineData(new object?[] { null }, false)]
    [InlineData(new object?[] { false, false }, false)]
    [InlineData(new object?[] { false, true }, false)]
    [InlineData(new object?[] { true, false }, false)]
    [InlineData(new object?[] { true, true }, true)]
    [InlineData(new object?[] { 1, 2 }, true)]
    [InlineData(new object?[] { 1, null }, false)]
    [InlineData(new object?[] { 1, 2, 3, 4, 5 }, true)]
    [InlineData(new object?[] { true, true, false, true, true }, false)]
    [InlineData(new object?[] { false, false, false, false }, false)]
    [InlineData(new object?[] { true, true, true, true }, true)]
    [InlineData(new object?[] { 1, 2, null }, false)]
    public void And(object?[] args, bool result)
    {
        var program = args.Select(item => item ?? nil).Prepend(and).ToTuple(false);

        AssertResult(result, Run(program));
    }

    [Theory]
    [InlineData(new object?[] { }, false)]
    [InlineData(new object?[] { false }, false)]
    [InlineData(new object?[] { true }, true)]
    [InlineData(new object?[] { 42 }, true)]
    [InlineData(new object?[] { null }, false)]
    [InlineData(new object?[] { false, false }, false)]
    [InlineData(new object?[] { false, true }, true)]
    [InlineData(new object?[] { true, false }, true)]
    [InlineData(new object?[] { true, true }, true)]
    [InlineData(new object?[] { 1, 2 }, true)]
    [InlineData(new object?[] { 1, null }, true)]
    [InlineData(new object?[] { 1, 2, 3, 4, 5 }, true)]
    [InlineData(new object?[] { true, true, false, true, true }, true)]
    [InlineData(new object?[] { false, false, false, false }, false)]
    [InlineData(new object?[] { true, true, true, true }, true)]
    [InlineData(new object?[] { 1, 2, null }, true)]
    public void Or(object?[] args, bool result)
    {
        var program = args.Select(item => item ?? nil).Prepend(or).ToTuple(false);

        AssertResult(result, Run(program));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AndOrOrDynamic(bool flag, bool result)
    {
        AssertResult(result, Run(((cond, (flag, and), (!flag, or)), true, false, true)));
    }

    [Theory]
    [InlineData(10, 55)]
    [InlineData(1000, 500500)]
    public void SimpleRecursion(int value, int result)
    {
        var (x, y, sum) = Declare("x", "y", "sum");

        AssertResult(result, Run(
            ((lambda, Single(y),
                (define, sum,
                    (lambda, Single(x),
                        (cond, ((eq, x, 0), 0),
                            (@else, (add, x, (sum, (sub, x, 1))))))),
                (sum, y)),
             value)
        ));
    }

    [Theory]
    [InlineData(10, 55)]
    public void TailRecursion1(int value, int result)
    {
        var (n, i, curr, prev, fib) = Declare("n", "i", "curr", "prev", "fib");
        AssertResult(result, Run(
            ((lambda, Single(n),
                (define, fib,
                    (lambda, (i, curr, prev),
                        (cond,
                            ((eq, i, 0), curr),
                            ((eq, i, 1), curr),
                            (@else, (fib, (sub, i, 1), (add, curr, prev), curr))))),
                (fib, n, 1, 0)),
            value)
        ));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    [InlineData(1000000)]
    public void TailRecursion2(int value)
    {
        var (x, y, i, count) = Declare("x", "y", "i", "count");

        AssertResult(value, Run(
            ((lambda, Single(y),
                (define, count,
                    (lambda, (x, i),
                        (cond,
                            ((eq, i, 0), x),
                            (@else, (count, (add, x, 1), (sub, i, 1)))))),
                (count, 0, y)),
            value)
        ));
    }

    [Fact]
    public void UnnamedDeclaration()
    {
        var (a, b) = Declare();
        var (c, d, e) = Declare();
        var (f, g, h, i) = Declare();
        var (j, k, l, m, n) = Declare();
        var (o, p, q, r, s, t) = Declare();
        var (u, v, w, x, y, z, a1) = Declare();
        var (b1, c1, d1, e1, f1, g1, h1, i1) = Declare();
        var (j1, k1, l1, m1, n1, o1, p1, q1, r1) = Declare();
        var (s1, t1, u1, v1, w1, x1, y1, z1, a2, b2) = Declare();

        var all = new[] {a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z, a1, b1, c1,
            d1, e1, f1, g1, h1, i1, j1, k1, l1, m1, n1, o1, p1, q1, r1, s1, t1, u1, v1, w1, x1, y1, z1, a2, b2};

        Assert.Equal(all.Length, all.Distinct().Count());
    }

    [Fact]
    public void ImplicitOne()
    {
        Assert.True(_.foo is Symbol { Name: "_foo" });
    }

    [Fact]
    public void ImplicitSame()
    {
        Assert.True(_.bar == _.bar);
    }

    [Fact]
    public void ImplicitDifferent()
    {
        Assert.True(_.foo != _.bar);
    }

    [Theory]
    [InlineData((short)1, 2, 3)]
    [InlineData((short)1, (short)2, 3)]
    [InlineData(1L, (short)2, 3L)]
    [InlineData(1L, 2L, 3L)]
    [InlineData(1, 2D, 3D)]
    [InlineData(1D, 2F, 3D)]
    [InlineData(1F, 2F, 3F)]
    [InlineData(1UL, 2U, 3UL)]
    public void NumericPromotion(object x, object y, object expected)
    {
        var actual = Run((add, x, y));
        Assert.Equal(expected, actual);
        Assert.Equal(expected.GetType(), actual?.GetType());
    }

    [Fact]
    public void BeginBlock()
    {
        AssertResult(3, Run(
            (begin, 
                (define, _.x, 1), 
                (define, _.y, 2), 
                (add, _.x, _.y))));
    }

    private void AssertResult(object? expected, object? actual)
    {
        if(expected is ITuple tuple)
        {
            expected = tuple.ToEnumerable(true);
        }

        if(expected is IEnumerable<object?> expectedEnumrable
            && actual is IEnumerable<object?> actualEnumerable)
        {
            var expectedList = expectedEnumrable.ToList();
            var actualList = actualEnumerable.ToList();
            Assert.Equal(expectedList.Count, actualList.Count);
            Assert.All(
                Enumerable.Zip(expectedList, actualList), 
                (pair) => AssertResult(pair.First, pair.Second));
        }
        else 
        { 
            Assert.Equal(expected, actual); 
        }
    }
    
    private static object? Run(ITuple program) =>
        Lisp.Parse(program).Compile()();

}