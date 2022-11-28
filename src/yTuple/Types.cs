﻿namespace yTuple;

internal static class Types
{
    public static readonly IEnumerable<object?> Empty = Enumerable.Empty<object?>();

    public static readonly IReadOnlyList<Type> Numeric = new List<Type>()
    {
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(ulong),
        typeof(long),
        typeof(uint),
        typeof(int)
    }.AsReadOnly();
    
    // As described in the following document
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions#11473-binary-numeric-promotions
    public static Type PromoteNumeric(Type left, Type right)
    {
        if (left == typeof(decimal) || right == typeof(decimal))
        {
            return typeof(decimal);
        }
        else if (left == typeof(double) || right == typeof(double))
        {
            return typeof(double);
        }
        else if (left == typeof(float) || right == typeof(float))
        {
            return typeof(float);
        }
        else if (left == typeof(ulong) || right == typeof(ulong))
        {
            return typeof(ulong);
        }
        else if (left == typeof(uint) && (right == typeof(int) || right == typeof(short) || right == typeof(sbyte))
            || right == typeof(uint) && (left == typeof(int) || left == typeof(short) || left == typeof(sbyte)))
        {
            return typeof(long);
        }
        else if (left == typeof(uint) || right == typeof(uint))
        {
            return typeof(uint);
        }
        else
        {
            return typeof(int);
        }
    }

    public static bool IsFalse(object? value) => value is bool flag && !flag || value == Empty;

    public static bool IsTrue(object? value) => !IsFalse(value);
}