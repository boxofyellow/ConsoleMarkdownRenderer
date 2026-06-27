namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class JsonReverserTests
    {
        [TestMethod]
        public void Should_Reverse_Json_String()
        {
            var json = """
                {
                    "name": "Alice",
                    "age": 30,
                    "address": {
                        "street": "123 Main St",
                        "city": "Anytown"
                    },
                    "hobbies": [
                        {
                            "name": "reading",
                            "type": "leisure"
                        },
                        {
                            "name": "hiking",
                            "type": "leisure"
                        }
                    ]
                }
                """;

            var expected = """
                {
                    "hobbies": [
                        {
                            "type": "leisure",
                            "name": "reading"
                        },
                        {
                            "type": "leisure",
                            "name": "hiking"
                        }
                    ],
                    "address": {
                        "city": "Anytown",
                        "street": "123 Main St"
                    },
                    "age": 30,
                    "name": "Alice"
                }
                """;

            var actual = JsonReverser.ReverseJsonPropertyOrder(json);
            TestUtilities.AssertTheseMatch(NormalizeText(expected), NormalizeText(actual), shouldMatch: true);
        }

        private static string NormalizeText(string text) 
            => text.Replace("\r", "")
                   .Replace("\n", "")
                   .Replace("\t", "")
                   .Replace(" ", "");
    }
}