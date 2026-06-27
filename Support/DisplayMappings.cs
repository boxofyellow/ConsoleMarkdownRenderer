using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Support
{
    [SourceFile]
    internal static class DisplayMappings
    {
        public static readonly BidirectionalMap<string, TextColor> Colors = new(
            Mappings.GetPropertyValues<TextColor>([typeof(TextColor)])
                // Default gets its own special handling 
                .Where(kvp => kvp.Name != nameof(TextColor.Default))
                .OrderBy(kvp => Mappings.SortColorNames(kvp.Name))
                .ThenBy(kvp => kvp.Name, StringComparer.OrdinalIgnoreCase),
            keyComparer: StringComparer.OrdinalIgnoreCase,
            allowDuplicateValues: false);  // TextColors unlike Spectre's Colors are unique, so we can disallow duplicate values here

        public static readonly BidirectionalMap<TextColor, Color> SpectreColorMap = new(
            PairColors(),
            allowDuplicateValues: true);

        public static readonly BidirectionalMap<TextJustification, Justify> JustificationMap = new(
            PairEnums<TextJustification, Justify>(),
            allowDuplicateValues: false);

        public static readonly BidirectionalMap<TextDecoration, Decoration> DecorationMap = new(
            PairEnums<TextDecoration, Decoration>(),
            allowDuplicateValues: false);

        public static readonly BidirectionalMap<RuleBorder, BoxBorder> RuleBoxBorderMap = new(
            PairEnumWithClass<RuleBorder, BoxBorder>(Mappings.BoxBorders),
            allowDuplicateValues: false);

        public static readonly BidirectionalMap<TextTableBorder, TableBorder> TableBoxBorderMap = new(
            PairEnumWithClass<TextTableBorder, TableBorder>(Mappings.TableBorders),
            allowDuplicateValues: false);

        private static List<(TextColor, Color)> PairColors()
        {
            List<(TextColor, Color)> pairs = [];
            pairs.Add((TextColor.Default, Color.Default));
            foreach (var (name, textColor) in Colors.Forward)
            {
                if (Mappings.Colors.Forward.TryGetValue(name, out var spectreColor))
                {
                    pairs.Add((textColor, spectreColor));
                }
            }
            return pairs;
        }

        private static List<(TKey1, TKey2)> PairEnums<TKey1, TKey2>()
            where TKey1 : struct, Enum
            where TKey2 : struct, Enum
        {
            var items1 = Mappings.EnumMappingByName<TKey1>();
            var items2 = Mappings.EnumMappingByName<TKey2>();
            List<(TKey1, TKey2)> pairs = [];
            foreach (var value in items1)
            {
                if (items2.TryGetValue(value.Key, out var correspondingValue))
                {
                    pairs.Add((value.Value, correspondingValue));
                }
            }
            return pairs;
        }

        public static List<(TEnum, TClass)> PairEnumWithClass<TEnum, TClass>(NamedTypeCollection<TClass> classValues)
            where TClass : notnull
            where TEnum : struct, Enum
        {
            var enumItems = Mappings.EnumMappingByName<TEnum>();
            List<(TEnum, TClass)> pairs = [];
            foreach (var value in classValues.NameMap.Forward)
            {
                if (enumItems.TryGetValue(value.Key, out var correspondingValue))
                {
                    pairs.Add((correspondingValue, value.Value));
                }
            }
            return pairs;
        }

        internal static readonly IReadOnlyDictionary<string, (Type Type, Action<DisplayOptions, object> Setter, Func<DisplayOptions, object> Getter)> DisplayOptionsProperties
            = Mappings.SettersAndGettersByName<DisplayOptions>();
    }
}