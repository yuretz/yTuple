namespace yTuple;

public static class Elementary
{
    public static readonly ValueTuple nil = default;

    // special operators
    public static readonly Symbol quote = new Quote();
    public static readonly Symbol cond = new Cond();

    public static readonly Operator atom = new Atom();
    public static readonly Operator car = new Car();
    public static readonly Operator cdr = new Cdr();
    public static readonly Operator cons = new Cons();
    

    
    public const string lambda = "lambda";
    public const string label = "label";

    public static readonly Operator eq = new Eq();
    public const string and = "and";
    public const string or = "or";
    public const string not = "not";

    public const string plus = "+";
    public const string minus = "-";
    public const string mul = "*";
    public const string div = "/";
}
