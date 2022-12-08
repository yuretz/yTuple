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

 For now, the best place to get a look and feel of it would be in the [test code](./test/yTuple.Tests/LispTests.cs). More examples are coming soon.

## Future features & ideas
* More C# interop features
* Serialization, so that it's possible to load code from external source
* Support macros
* Use [FastExpressionCompiler](dadhi/FastExpressionCompiler) for even better performance

## Futher info
If you have a question, or think you found a bug, or have a good idea and don't mind sharing it, please open an issue and I would be happy to discuss it.
