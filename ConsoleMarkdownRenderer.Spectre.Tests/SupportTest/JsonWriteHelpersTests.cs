using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class JsonWriteHelpersTests
    {
        [TestMethod]
        public void ShouldIgnoreCovered()
        {
            var options = new JsonSerializerOptions();
            foreach (var condition in Enum.GetValues<JsonIgnoreCondition>())
            {
                // Just checking to make sure we have a case for this and we don't throw
                JsonWriteHelpers.ShouldIgnore<object?>(value: null, options, condition);
            }
        }

        [TestMethod]
        public void JsonWriteHelpers_ShouldIgnore_Always_ReturnsTrue()
        {
            var options = new JsonSerializerOptions();
            // DefaultIgnoreCondition.Always cannot be set on JsonSerializerOptions directly
            // (the property setter rejects it), so ShouldIgnore is exercised directly to
            // cover the Always / Never arms.
            Assert.IsTrue(JsonWriteHelpers.ShouldIgnore("anything", options, JsonIgnoreCondition.Always));
            Assert.IsTrue(JsonWriteHelpers.ShouldIgnore<string?>(null, options, JsonIgnoreCondition.Always));
            Assert.IsFalse(JsonWriteHelpers.ShouldIgnore("anything", options, JsonIgnoreCondition.Never));
        }

        [TestMethod]
        public void JsonWriteHelpers_ShouldIgnore_UnexpectedCondition_Throws()
        {
            var options = new JsonSerializerOptions();
            // An out-of-range enum value falls through to the default arm of the switch.
            var ex = Assert.ThrowsExactly<InvalidOperationException>(
                () => JsonWriteHelpers.ShouldIgnore("anything", options, (JsonIgnoreCondition)int.MaxValue));
            Assert.Contains(int.MaxValue.ToString(), ex.Message);
        }
    }
}