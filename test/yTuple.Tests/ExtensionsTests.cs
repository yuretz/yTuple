namespace yTuple.Tests;

public class ExtensionsTests
{
    [Fact]
    public void Empty()
    {
        Assert.Empty(Extensions.Empty);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToFromEmpty(bool recurse)
    {
        Assert.Equal(default(ValueTuple), Extensions.Empty.ToTuple(recurse));
        Assert.Empty(default(ValueTuple).ToEnumerable(recurse));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToFromSingle(bool recurse)
    {
        var tuple = new ValueTuple<int>(42);
        var items = new object?[] { 42 };
        Assert.Equal(tuple, items.ToTuple(recurse));
        AssertEnumerable(items, tuple.ToEnumerable(recurse));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToFromFew(bool recurse)
    {
        var tuple = (1, 2, 3);
        var items = new object?[] { 1, 2, 3 };
        Assert.Equal(tuple, items.ToTuple(recurse));
        AssertEnumerable(items, tuple.ToEnumerable(recurse));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToFromMany(bool recurse)
    {
        var tuple = (1, 2, true, 3, 4, 5, "foo", 6, 7, 8, 9, 0, "bar");
        var items = new object?[] { 1, 2, true, 3, 4, 5, "foo", 6, 7, 8, 9, 0, "bar" };
        Assert.Equal(tuple, items.ToTuple(recurse));
        AssertEnumerable(items, tuple.ToEnumerable(recurse));
    }

    [Fact]
    public void ToFromShallow()
    {
        var tuple1 = ((1, 2), (true, (3, 4, 5)), ("foo", 6, 7, 8, 9, 0, "bar"));
        var items1 = new object?[] { (1, 2), (true, (3, 4, 5)), ("foo", 6, 7, 8, 9, 0, "bar")};
        Assert.Equal(tuple1, items1.ToTuple(false));
        AssertEnumerable(items1, tuple1.ToEnumerable(false));
    }

    [Fact]
    public void ToFromDeep()
    {
        var tuple1 = ((1, 2), (true, (3, 4, 5)), ("foo", 6, 7, 8, 9, 0, "bar"));
        var items1 = new object?[] 
        { 
            new object[] { 1, 2 }, 
            new object[] { true, new object[] { 3, 4, 5 } }, 
            new object[] { "foo", 6, 7, 8, 9, 0, "bar" } 
        };
        Assert.Equal(tuple1, items1.ToTuple(true));
        AssertEnumerable(items1, tuple1.ToEnumerable(true));
    }

    private void AssertEnumerable(IEnumerable<object?> expected, IEnumerable<object?> actual)
    {
        var expectedList = expected.ToList();
        var actualList = actual.ToList();
        Assert.Equal(expectedList.Count, actualList.Count);
        Assert.All(
            Enumerable.Zip(expectedList, actualList),
            pair =>
            {
                if (pair is { First: IEnumerable<object?> first, Second: IEnumerable<object?> second })
                {
                    AssertEnumerable(first, second);
                }
                else 
                { 
                    Assert.Equal(pair.First, pair.Second); 
                }
            });
    }
}
