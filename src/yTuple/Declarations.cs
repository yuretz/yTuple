using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace yTuple;

public class Declaration : ITuple
{
    public Declaration() => _offset = _nextOffset++;

    object? ITuple.this[int index] => this[index];
    int ITuple.Length => _maxDeconstedLength;

    public Symbol this[int index] => index < _maxDeconstedLength  
        ? new Var($"_{10*_offset + index}")
        : throw new ArgumentOutOfRangeException(nameof(index), $"Max {_maxDeconstedLength} symbols per declaration");

    public void Deconstruct(
        out Symbol value0)
    {
        value0 = this[0];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1)
    {
        value0 = this[0];
        value1 = this[1];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1,
        out Symbol value2)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1,
        out Symbol value2,
        out Symbol value3)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
        value3 = this[3];
    }

    public void Deconstruct(
    out Symbol value0,
    out Symbol value1,
    out Symbol value2,
    out Symbol value3,
    out Symbol value4)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
        value3 = this[3];
        value4 = this[4];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1,
        out Symbol value2,
        out Symbol value3,
        out Symbol value4,
        out Symbol value5)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
        value3 = this[3];
        value4 = this[4];
        value5 = this[5];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1,
        out Symbol value2,
        out Symbol value3,
        out Symbol value4,
        out Symbol value5,
        out Symbol value6)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
        value3 = this[3];
        value4 = this[4];
        value5 = this[5];
        value6 = this[6];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1,
        out Symbol value2,
        out Symbol value3,
        out Symbol value4,
        out Symbol value5,
        out Symbol value6,
        out Symbol value7)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
        value3 = this[3];
        value4 = this[4];
        value5 = this[5];
        value6 = this[6];
        value7 = this[7];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1,
        out Symbol value2,
        out Symbol value3,
        out Symbol value4,
        out Symbol value5,
        out Symbol value6,
        out Symbol value7,
        out Symbol value8)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
        value3 = this[3];
        value4 = this[4];
        value5 = this[5];
        value6 = this[6];
        value7 = this[7];
        value8 = this[8];
    }

    public void Deconstruct(
        out Symbol value0,
        out Symbol value1,
        out Symbol value2,
        out Symbol value3,
        out Symbol value4,
        out Symbol value5,
        out Symbol value6,
        out Symbol value7,
        out Symbol value8,
        out Symbol value9)
    {
        value0 = this[0];
        value1 = this[1];
        value2 = this[2];
        value3 = this[3];
        value4 = this[4];
        value5 = this[5];
        value6 = this[6];
        value7 = this[7];
        value8 = this[8];
        value9 = this[9];
    }

    private readonly int _offset;
    private const int _maxDeconstedLength = 10;
    private static int _nextOffset = 0;
}