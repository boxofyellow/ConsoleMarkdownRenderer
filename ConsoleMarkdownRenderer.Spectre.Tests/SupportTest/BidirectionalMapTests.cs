using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class BidirectionalMapTests
{
    [TestMethod]
    public void Should_Map_Forward_And_Reverse()
    {
        var items = new[]
        {
            ("One", 1),
            ("Two", 2),
            ("Three", 3),
        };
        var map = new BidirectionalMap<string, int>(items);

        foreach (var (key, value) in items)
        {
            Assert.IsTrue(map.Forward.TryGetValue(key, out var forwardValue));
            TestUtilities.AssertTheseMatch(value, forwardValue, shouldMatch: true);

            Assert.IsTrue(map.Reverse.TryGetValue(value, out var reverseKey));
            TestUtilities.AssertTheseMatch(key, reverseKey, shouldMatch: true);
        }
    }

    [TestMethod]
    public void Should_Not_Allow_Duplicate_Keys()
    {
        var items = new[]
        {
            ("One", 1),
            ("Two", 2),
            ("One", 3),
        };
        Assert.Throws<ArgumentException>(() => new BidirectionalMap<string, int>(items));
    }

    [TestMethod]
    public void Should_Not_Allow_Values_By_Default()
    {
        var items = new[]
        {
            ("One", 1),
            ("Two", 2),
            ("Other One", 1),
        };
        Assert.Throws<ArgumentException>(() => new BidirectionalMap<string, int>(items));
    }


    [TestMethod]
    public void Should_Allow_Duplicate_Values_When_Requested()
    {
        var items = new[]
        {
            ("One", 1),
            ("Two", 2),
            ("Other One", 1),
        };
        var map = new BidirectionalMap<string, int>(items, allowDuplicateValues: true);

        foreach (var (key, value) in items)
        {
            Assert.IsTrue(map.Forward.TryGetValue(key, out var forwardValue));
            TestUtilities.AssertTheseMatch(value, forwardValue, shouldMatch: true);

            Assert.IsTrue(map.Reverse.TryGetValue(value, out var reverseKey));

            var expectedItem = items.First(x => x.Item2 == value).Item1; // The first one wins in the case of duplicates
            TestUtilities.AssertTheseMatch(expectedItem, reverseKey, shouldMatch: true);
        }
    }
}