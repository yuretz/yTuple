# λTuple
![GitHub](https://img.shields.io/github/license/yuretz/FreeAwait)

_Scheme_ for your tuples

## What is it?
This small library implements a Scheme-inspired Lisp dialect allowing you to write some lisp directly in your C# code. 

In other words, by exploiting certain C# language features _λTuple_ aims to to provide Lisp's glorious syntax and semantics from within the C#/.NET environment.

Among other things, it is:

* :racing_car: Highly dynamic, as any Lisp shall be: data and code are the same.
* :zap: Pretty fast, because it's not an interpreter: all your code is compiled using `System.Linq.Expressions`.
* :handshake: Well integarted with C# and .NET: mix and match your normal C# and Lisp code as you like
* :cyclone: Optimizing tail calls  when possible.
* :question: Work in progress, more goodies to come...

## Show me the code
First things first, let's add _λTuple_ to the usings section of your program.
```csharp
using yTuple;
using static yTuple.Elementary;
```

This should make it possible to write the following code.
```csharp
var fib = Lisp.Parse(n =>
    (begin,
        (define, _.loop, (lambda, (_.i, _.curr, _.prev),
            (cond,
                ((eq, _.i, 0), 0),
                ((eq, _.i, 1), _.curr),
                (@else, 
                    (_.loop, (sub, _.i, 1), (add, _.curr, _.prev), _.curr))))),
        (_.loop, n, 1, 0))
).Compile();

Console.WriteLine(fib(42));

```
This creates a tail-recursive lambda function `fib()`, that computes n-th number in the Fibonacci sequence and then uses it to output the result for `n = 42`. 


Another good place to get a look and feel of it would be in the [test code](./test/yTuple.Tests/LispTests.cs). More examples are coming soon.

## Future features & ideas
* More C# interop features
* Better static typing to avoid unnecessary boxing and dynamic type casts
* Serialization, so that it's possible to load code from external source
* Support macros
* Use [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) for even better performance

## Futher info
If you have a question, or think you found a bug, or have a good idea and don't mind sharing it, please open an issue and I would be happy to discuss it.
