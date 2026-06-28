namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class JsonPropertyCollectorTest
{
    [TestMethod]
    public void Should_Collect_All_Property_Names()
    {
        var json = """
            {
                "name": "Alice",
                "age": 30,
                "address": {
                    "street": "123 Main St",
                    "city": "Anytown"
                },
                "hobbies": ["reading", "hiking"]
            }
            """;

        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "name", "age", "address", "street", "city", "hobbies"
        };

        var actual = JsonPropertyCollector.GetAllPropertyNames(json);
        CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
    }

    [TestMethod]
    public void Should_Find_Null_Properties()
    {
        var json = """
            {
                "name": "Bob",
                "age": null,
                "address": {
                    "street": null,
                    "city": "Othertown"
                },
                "hobbies": null
            }
            """;

        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "age", "street", "hobbies"
        };

        var actual = JsonPropertyCollector.FindNullProperties(json);
        CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
    }

    [TestMethod]
    public void Should_Find_Default_Properties()
    {
        var json = """
            {
                "integer": 0,
                "integerWithValue": 42,
                "string": "",
                "stringWithValue": "hello",
                "bool": false,
                "boolWithValue": true,
                "list": [],
                "listWithValue": [1, 2, 3],
                "object": {
                    "nestedInteger": 0,
                    "nestedIntegerWithValue": 1
                },
                "listWithObjects": [
                    { "listInteger": 0 },
                    { "listIntegerWithValue": 2 }
                ],
                "null": null
            }
            """;

        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "integer", "string", "bool", "list", "nestedInteger", "listInteger", "null"
        };

        var actual = JsonPropertyCollector.FindDefaultProperties(json);
        CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
    }
}
