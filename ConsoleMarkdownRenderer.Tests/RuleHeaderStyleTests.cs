using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="RuleHeaderStyle"/> and the shared <see cref="IHeaderStyle"/> contract.
    /// </summary>
    [TestClass]
    public class RuleHeaderStyleTests
    {
        [TestMethod]
        public void RuleHeaderStyle_DefaultsAreNull()
        {
            var style = RuleHeaderStyle.Create();
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Border);
        }

        [TestMethod]
        public void RuleHeaderStyle_Create_PreservesProperties()
        {
            var created = RuleHeaderStyle.Create(
                justification: TextJustification.Center,
                foreground: TextColor.Red,
                border: RuleBorder.Heavy);

            Assert.AreEqual(TextJustification.Center, created.Justification);
            Assert.AreEqual(TextColor.Red,            created.Foreground);
            Assert.AreEqual(RuleBorder.Heavy,         created.Border);
            Assert.AreEqual(
                RuleHeaderStyle.Create(TextJustification.Center, TextColor.Red, RuleBorder.Heavy),
                created);
        }

        [TestMethod]
        [DataRow(TextJustification.Left,   "red",   RuleBorder.Square,  TextJustification.Left,   "red",   RuleBorder.Square,  true )]
        [DataRow(TextJustification.Left,   "red",   RuleBorder.Square,  TextJustification.Right,  "red",   RuleBorder.Square,  false)]
        [DataRow(TextJustification.Left,   "red",   RuleBorder.Square,  TextJustification.Left,   "blue",  RuleBorder.Square,  false)]
        [DataRow(TextJustification.Left,   "red",   RuleBorder.Square,  TextJustification.Left,   "red",   RuleBorder.Rounded, false)]
        public void RuleHeaderStyle_Equality(
            TextJustification justificationA, string foregroundA, RuleBorder borderA,
            TextJustification justificationB, string foregroundB, RuleBorder borderB,
            bool expectedEqual)
        {
            var a = RuleHeaderStyle.Create(justificationA, TextStyle.FromMarkup(foregroundA).Foreground, borderA);
            var b = RuleHeaderStyle.Create(justificationB, TextStyle.FromMarkup(foregroundB).Foreground, borderB);

            Assert.AreEqual(expectedEqual, a.Equals(b));
            if (expectedEqual)
            {
                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            }
        }

        [TestMethod]
        public void RuleHeaderStyle_BackgroundAndDecoration_HardCodedViaIHeaderStyle()
        {
            // Rule does not support a background colour or text decoration on its title, so
            // those members on the IHeaderStyle interface are hard-coded.
            IHeaderStyle rule = RuleHeaderStyle.Create(foreground: TextColor.Green);

            Assert.AreEqual(TextColor.Green,     rule.Foreground);
            Assert.IsNull(rule.Background);
            Assert.AreEqual(TextDecoration.None, rule.Decoration);
        }

        [TestMethod]
        public async Task RuleHeaderStyle_JsonRoundTrip_ViaDisplayOptions()
        {
            var json = $$"""
                {
                    "headers": [
                        {
                            "$type": "{{nameof(RuleHeaderStyle)}}",
                            "justification": "{{TextJustification.Center}}",
                            "foreground": { "named": "{{NamedColor.Red}}" },
                            "border": "{{RuleBorder.Heavy}}"
                        }
                    ]
                }
                """;

            var options = await DisplayOptions.DeserializeAsync(json).ConfigureAwait(false);

            Assert.IsTrue(options.Headers.Count >= 1, "Expected at least one header entry.");
            Assert.IsInstanceOfType<RuleHeaderStyle>(options.Headers[0]);
            var style = (RuleHeaderStyle)options.Headers[0];
            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Red,            style.Foreground);
            Assert.AreEqual(RuleBorder.Heavy,         style.Border);
        }

        [TestMethod]
        public void RuleHeaderStyle_JsonRoundTrip_NullsArePreserved()
        {
            // Round-trip a RuleHeaderStyle whose fields are all null through DisplayOptions
            // serialization, ensuring the converter emits a $type discriminator even when
            // every other field is omitted.
            var original = new DisplayOptions
            {
                Headers = new() { RuleHeaderStyle.Create() },
            };

            var serialized = original.Serialize();
            StringAssert.Contains(serialized, $"\"$type\":\"{nameof(RuleHeaderStyle)}\"");

            var roundTripped = DisplayOptions.DeserializeAsync(serialized).GetAwaiter().GetResult();
            Assert.IsInstanceOfType<RuleHeaderStyle>(roundTripped.Headers[0]);
            var style = (RuleHeaderStyle)roundTripped.Headers[0];
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Border);
        }
    }
}
