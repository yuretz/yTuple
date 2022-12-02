using System.Runtime.CompilerServices;

namespace yTuple;

public static class Elementary
{

    public static readonly ValueTuple nil;

    // special forms and operators
    public static readonly Symbol quote = new Quote();
    public static readonly Symbol cond = new Cond();
    public static readonly Symbol lambda = new Lambda();
    public static readonly Symbol define = new Define();
    public static readonly Symbol @else = new Else();
    public static readonly Symbol begin = new Begin();

    public static readonly Op atom = new Atom();
    public static readonly Op car = new Car();
    public static readonly Op cdr = new Cdr();
    public static readonly Op cons = new Cons();

    public static readonly Op eq = new Eq();
    public static readonly Op and = new And();
    public static readonly Op or = new Or();
    public static readonly Op not = new Not();

    public static readonly Op add = new Add();
    public static readonly Op sub = new Sub();
    public static readonly Op mul = new Mul();
    public static readonly Op div = new Div();

    public static readonly dynamic _ = new DynamicDeclaration("_");

    public static ValueTuple<T> Single<T>(T value) => new(value);

    public static Declaration Declare() => new Declaration();

    public static Symbol Declare(string name) => new Var(name);
    
    public static (Symbol, Symbol) Declare(string v1, string v2) => 
        (new Var(v1), new Var(v2));

    public static (Symbol, Symbol, Symbol) Declare(string v1, string v2, string v3) => 
        (new Var(v1), new Var(v2), new Var(v3));

    public static (Symbol, Symbol, Symbol, Symbol) Declare(string v1, string v2, string v3, string v4) => 
        (new Var(v1), new Var(v2), new Var(v3), new Var(v4));

    public static (Symbol, Symbol, Symbol, Symbol, Symbol) Declare(string v1, string v2, string v3, string v4, string v5) =>
        (new Var(v1), new Var(v2), new Var(v3), new Var(v4), new Var(v5));

}
