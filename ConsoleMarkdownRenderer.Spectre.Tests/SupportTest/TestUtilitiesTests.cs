using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class TestUtilitiesTests
    {
        [TestMethod]
        [DataRow(true, true, true)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        public void AssertTheseMatch_Should_Work_Correctly_Bool(bool expected, bool actual, bool shouldMatch) 
            => TestUtilities.AssertTheseMatch(expected, actual, shouldMatch);

        [TestMethod]
        [DataRow("Hello", "Hello", true)]
        [DataRow("Hello", "World", false)]
        [DataRow("hello", "HELLO", false)]
        [DataRow("", "", true)]
        public void AssertTheseMatch_Should_Work_Correctly_String(string expected, string actual, bool shouldMatch) 
            => TestUtilities.AssertTheseMatch(expected, actual, shouldMatch);

        [TestMethod]
        [DataRow(1, 1, true)]
        [DataRow(1, 2, false)]
        public void AssertTheseMatch_Should_Work_Correctly_Int(int expected, int actual, bool shouldMatch) 
            => TestUtilities.AssertTheseMatch(expected, actual, shouldMatch);

        [TestMethod]
        [DataRow("", "", true)]
        [DataRow("0,0", "0,0", true)]
        [DataRow("0,0", "0,1", false)]
        [DataRow("0,0", "0,0,0", false)]
        public void AssertTheseMatch_Should_Work_Correctly_List(string expected, string actual, bool shouldMatch)
            => TestUtilities.AssertTheseMatch(ListFromString(expected), ListFromString(actual), shouldMatch);

        [TestMethod]
        [DataRow("", "", true)]
        [DataRow("0,0", "0,0", true)]
        [DataRow("0,0", "0,1", false)]
        [DataRow("0,0", "0,0,0", false)]
        public void AssertTheseMatch_Should_Work_Correctly_NullableList(string expected, string actual, bool shouldMatch)
        {
            List<int>? expectedList = string.IsNullOrEmpty(expected) ? null : ListFromString(expected);
            List<int>? actualList = string.IsNullOrEmpty(actual) ? null : ListFromString(actual);
            TestUtilities.AssertTheseMatch(expectedList, actualList, shouldMatch);
        }

        [TestMethod]
        public void DifferenceFinder_Should_Return_Non_Empty_String_When_Values_Differ() 
            => Assert.IsFalse(string.IsNullOrEmpty(TestOptions.GetOptions().FindDifference(
                typeof(SpectreDisplayOptions),
                new SpectreDisplayOptions(),
                TestUtilities.Crazy)));

        [TestMethod]
        public void DifferenceFinder_Should_Return_Empty_String_When_Unregistered_Type() 
            => Assert.IsTrue(string.IsNullOrEmpty(TestOptions.GetOptions().FindDifference(
                typeof(Color),
                Color.Red,
                Color.Blue)));

        [TestMethod]
        public void Registered_DifferenceFinder_Should_Be_Used()
        {
            const string magicString = nameof(magicString);

            // Check to make sure our test is valid
            Assert.DoesNotContain(magicString, Assert.Throws<AssertFailedException>(() =>
            {
                TestUtilities.AssertTheseMatch(0, 1, shouldMatch: true);
            }).Message);

            var expectedList = new List<int> { 0, 0 };
            var actualList = new List<int> { 0, 1 };
            Assert.DoesNotContain(magicString, Assert.Throws<AssertFailedException>(() =>
            {
                TestUtilities.AssertTheseMatch(expectedList, actualList, shouldMatch: true);
            }).Message);

            // Register our finder
            TestOptions.GetOptions().RegisterDifferenceFinder<int>((_, _) => magicString);

            Assert.Contains(magicString, Assert.Throws<AssertFailedException>(() =>
            {
                TestUtilities.AssertTheseMatch(0, 1, shouldMatch: true);
            }).Message);

            Assert.Contains(magicString, Assert.Throws<AssertFailedException>(() =>
            {
                TestUtilities.AssertTheseMatch(expectedList, actualList, shouldMatch: true);
            }).Message);
        }

        [TestMethod]
        public void Bad_Implementation_Of_IsDefault_Fail()
        {
            // Sanity Check - these don't match, and this should pass
            TestUtilities.AssertTheseMatch(0, 1, shouldMatch: false);

            var expectedList = new List<int> { 0, 0 };
            var actualList = new List<int> { 0, 1 };
            TestUtilities.AssertTheseMatch(expectedList, actualList, shouldMatch: false);

            TestOptions.GetOptions().SerializerOptions.Add(new JsonSerializerOptions
            {
                Converters = { new BadIsDefaultIdentifierJsonConverter() }
            });

            // Now this should throw once we add our bad implementation
            Assert.Throws<AssertFailedException>(() =>
            {
                TestUtilities.AssertTheseMatch(0, 1, shouldMatch: false);
            });

            Assert.Throws<AssertFailedException>(() =>
            {
                TestUtilities.AssertTheseMatch(expectedList, actualList, shouldMatch: false);
            });
        }

        private class BadIsDefaultIdentifierJsonConverter : JsonConverter<int>, IDefaultIdentifier
        {
            public bool? IsDefault(object obj) => obj switch
            {
                // This is a bad implementation because it return true for some ints, and null for others, A good implementation return true or false for all things of the same type
                int i when i == 0 => true,
                _ => null,
            };

            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
                => throw new NotImplementedException();
        }

        private static List<int> ListFromString(string s) => [.. s.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse)];
    }
}