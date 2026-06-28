using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Support;

[SourceFile]
internal static class Extensions
{
    internal static Style ToSpectreStyle(this TextStyle headerStyle) => new(
        headerStyle.Foreground?.ToSpectreColor(),
        headerStyle.Background?.ToSpectreColor(),
        headerStyle.Decoration.ToSpectreDecoration());

    internal static TextStyle ToTextStyle(this Style spectreStyle, bool preferNullColors = false) => new(
        spectreStyle.Decoration.ToTextDecoration(),
        ManageNullColor(spectreStyle.Foreground.ToTextColor(), preferNullColors),
        ManageNullColor(spectreStyle.Background.ToTextColor(), preferNullColors));

    private static TextColor? ManageNullColor(TextColor color, bool preferNullColors)
        => preferNullColors && color.IsDefault() ? null : color;

    internal static ISpectreHeaderStyle ToSpectreHeaderStyle(this IHeaderStyle headerStyle)
         => headerStyle switch
        {
            TextStyle t => new SpectreTextStyle(ToSpectreDecoration(t.Decoration), t.Foreground?.ToSpectreColor(), t.Background?.ToSpectreColor()),
            RuleHeaderStyle r => new SpectreRuleHeaderStyle(r.Justification?.ToSpectreJustify(), r.Foreground?.ToSpectreColor(), r.Border?.ToSpectreBoxBorder()),
            FigletTextStyle f => SpectreFigletTextStyle.Create(f.Justification?.ToSpectreJustify(), f.Foreground?.ToSpectreColor(), f.FontPath, f.Font),
            _ => throw new ArgumentException($"Unknown {nameof(IHeaderStyle)} implementation type {headerStyle.GetType().FullName}"),
        };

    internal static IHeaderStyle ToHeaderStyle(this ISpectreHeaderStyle spectreHeaderStyle)
        => spectreHeaderStyle switch
        {
            SpectreTextStyle t => new TextStyle(ToTextDecoration(t.Decoration), t.Foreground?.ToTextColor(), t.Background?.ToTextColor()),
            SpectreRuleHeaderStyle r => new RuleHeaderStyle(r.Justification?.ToTextJustification(), r.Foreground?.ToTextColor(), r.Border?.ToRuleBorder()),
            SpectreFigletTextStyle f => FigletTextStyle.Create(f.Justification?.ToTextJustification(), f.Foreground?.ToTextColor(), f.FontPath, f.Font),
            _ => throw new ArgumentException($"Unknown {nameof(ISpectreHeaderStyle)} implementation type {spectreHeaderStyle.GetType().FullName}"),
        };

    internal static Color ToSpectreColor(this TextColor textColor) 
        => DisplayMappings.SpectreColorMap.GetForward(textColor, new Color(textColor.R, textColor.G, textColor.B));

    internal static TextColor ToTextColor(this Color spectreColor)
        => DisplayMappings.SpectreColorMap.GetReverse(spectreColor, TextColor.FromRgb(spectreColor.R, spectreColor.G, spectreColor.B));

    internal static TableBorder ToSpectreTableBorder(this TextTableBorder border)
        => DisplayMappings.TableBoxBorderMap.GetForward(border, TableBorder.None);

    internal static TextTableBorder ToTextTableBorder(this TableBorder border)
        => DisplayMappings.TableBoxBorderMap.GetReverse(border, TextTableBorder.None);

    internal static Justify ToSpectreJustify(this TextJustification justification) 
        => DisplayMappings.JustificationMap.GetForward(justification, Justify.Left);

    internal static TextJustification ToTextJustification(this Justify justification) 
        => DisplayMappings.JustificationMap.GetReverse(justification, TextJustification.Left);

    internal static BoxBorder ToSpectreBoxBorder(this RuleBorder border)
        => DisplayMappings.RuleBoxBorderMap.GetForward(border, BoxBorder.None);

    internal static RuleBorder ToRuleBorder(this BoxBorder border)
        => DisplayMappings.RuleBoxBorderMap.GetReverse(border, RuleBorder.None);

    private static Decoration ToSpectreDecoration(this TextDecoration decoration) 
        => ToEnumFlags(decoration, DisplayMappings.DecorationMap.Forward, Decoration.None);

    internal static TextDecoration ToTextDecoration(this Decoration spectreDecoration)
        => ToEnumFlags(spectreDecoration, DisplayMappings.DecorationMap.Reverse, TextDecoration.None);

    internal static T AssertDeserializationIsNotNull<T>(this T? value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new JsonException($"Expected non-null value for {paramName}");
        }
        return value;
    }

    private static TTo ToEnumFlags<TFrom, TTo>(TFrom from, IReadOnlyDictionary<TFrom, TTo> map, TTo start)
        where TFrom : struct, Enum
        where TTo : struct, Enum
    {
        var result = start;
        foreach (var kvp in map)
        {
            if (kvp.Value.Equals(start))
            {
                continue; // Skip the "None" value
            }

            if (from.HasFlag(kvp.Key))
            {
                result = (TTo)(object)(((int)(object)result) | ((int)(object)kvp.Value));
            }
        }
        return result;
    }
}
