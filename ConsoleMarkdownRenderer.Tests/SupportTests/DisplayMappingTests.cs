using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

[TestClass]
public class DisplayMappingTests : TestBase
{
    [TestMethod]
    public void Assert_Options_Properties_Match()
    {
        var mappedTypes = new Dictionary<Type, Type>
        {
            { typeof(Color), typeof(TextColor) },
            { typeof(Decoration), typeof(TextDecoration) },
            { typeof(Justify), typeof(TextJustification) },
            { typeof(BoxBorder), typeof(RuleBorder) },
            { typeof(TableBorder), typeof(TextTableBorder) },
            { typeof(Style), typeof(TextStyle) },
            { typeof(List<ISpectreHeaderStyle>), typeof(List<IHeaderStyle>) },
            { typeof(ISpectreHeaderStyle), typeof(IHeaderStyle) }
        };

        Assert.HasCount(Mappings.SpectreDisplayOptionsProperties.Count, DisplayMappings.DisplayOptionsProperties);
        foreach (var kvp in Mappings.SpectreDisplayOptionsProperties)
        {
            Assert.IsTrue(DisplayMappings.DisplayOptionsProperties.ContainsKey(kvp.Key), $"Property {kvp.Key} is missing from DisplayMappings.DisplayOptionsProperties");
            var displayMappingProperty = DisplayMappings.DisplayOptionsProperties[kvp.Key];
            var mappedType = mappedTypes.GetValueOrDefault(kvp.Value.Type) ?? kvp.Value.Type;
            Assert.AreEqual(mappedType, displayMappingProperty.Type, $"Property {kvp.Key} has type {displayMappingProperty.Type} in DisplayMappings but {kvp.Value.Type} in Mappings");
        }
    }

    [TestMethod]
    public void Assert_Decorations_Matches()
        => Assert.HasCount(Mappings.DecorationByName.Count, DisplayMappings.DecorationMap.Forward);

    [TestMethod]
    public void Assert_BoxBorders_Matches()
        => Assert.HasCount(Mappings.BoxBorders.NameMap.Forward.Count, DisplayMappings.RuleBoxBorderMap.Forward);

    [TestMethod]
    public void Assert_TableBorders_Matches()
        => Assert.HasCount(Mappings.TableBorders.NameMap.Forward.Count, DisplayMappings.TableBoxBorderMap.Forward);
}