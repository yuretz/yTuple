using System.Runtime.CompilerServices;

namespace yTuple;

public static class Elementary
{

    public static readonly ValueTuple nil;

    // special forms and operators
    public static readonly Symbol quote = new Quote();
    public static readonly Symbol cond = new Cond();
    public static readonly Symbol lambda = new Lambda();
    public static readonly Symbol label = new Label();

    public static readonly Op atom = new Atom();
    public static readonly Op car = new Car();
    public static readonly Op cdr = new Cdr();
    public static readonly Op cons = new Cons();

    public static readonly Op eq = new Eq();
    public const string and = "and";
    public const string or = "or";
    public const string not = "not";

    public const string plus = "+";
    public const string minus = "-";
    public const string mul = "*";
    public const string div = "/";

    public static ValueTuple<T> Single<T>(T value) => new(value);

    public static Symbol Declare(string name) => new Var(name);
    public static ITuple Declare(params string[] names) => names.Select(name => new Var(name)).ToTuple(false);



}
