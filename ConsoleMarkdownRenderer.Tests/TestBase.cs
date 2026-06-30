using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

// All Tests in this project should inherit from this class
public abstract class TestBase
{
    [TestInitialize]
    public virtual void TestInitialize()
    {
        var options = TestOptions.GetOptions();
        options.SerializerOptions.Add(DisplayOptions.BuildEffectiveOptions(caller: null));
        options.RegisterDifferenceFinder<DisplayOptions>((e, a) => FindFirstDifferenceDisplayOptions(e, a, details: true));
    }

    public static string FindFirstDifferenceDisplayOptions(DisplayOptions expected, DisplayOptions actual, bool details)
    {
        foreach (var property in DisplayMappings.DisplayOptionsProperties)
        {
            var type = property.Value.Type;
            if (type == typeof(TextStyle) 
             || type == typeof(int)
             || type == typeof(IHeaderStyle)
             || type == typeof(bool)
             || type == typeof(string)
             || type == typeof(TextTableBorder)
             || type == typeof(RuleBorder))
            {
                var expectedValue = property.Value.Getter(expected);
                var actualValue = property.Value.Getter(actual);
                if (!Equals(expectedValue, actualValue))
                {
                    if (details)
                    {
                        return $"{property.Key} (expected: {expectedValue}, actual: {actualValue})";
                    }
                    return property.Key;
                }
            }
            else if (type == typeof(List<IHeaderStyle>))
            {
                var expectedList = (List<IHeaderStyle>)property.Value.Getter(expected);
                var actualList = (List<IHeaderStyle>)property.Value.Getter(actual);
                if (expectedList.Count != actualList.Count)
                {
                    if (details)
                    {
                        return $"{property.Key} (expected count: {expectedList.Count}, actual count: {actualList.Count})";
                    }
                    return property.Key;
                }
                for (int i = 0; i < expectedList.Count; i++)
                {
                    if (!Equals(expectedList[i], actualList[i]))
                    {
                        if (details)
                        {
                            return $"{property.Key}[{i}] (expected: {expectedList[i]}, actual: {actualList[i]})";
                        }
                        return $"{property.Key}[{i}]";
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Type {type} not supported for comparison in FindFirstDifference");
            }
        }
        return "";
    }
}