using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
public static class Mappings
{
    private static readonly IReadOnlyDictionary<string, ConsoleColor> s_consoleColorsByName 
        = EnumMappingByName<ConsoleColor>();

    public static readonly NamedTypeCollection<BoxBorder> BoxBorders 
        = new(GetPropertyValues<BoxBorder>([typeof(BoxBorder)]));

    public static readonly NamedTypeCollection<TableBorder> TableBorders 
        = new(GetPropertyValues<TableBorder>([typeof(TableBorder), typeof(DefaultTableBorder)]));

    public static readonly BidirectionalMap<string, Color> Colors = new(
        GetPropertyValues<Color>([typeof(Color)])
            // Default gets its own special handling 
            .Where(kvp => kvp.Name != nameof(Color.Default))
            .OrderBy(kvp => SortColorNames(kvp.Name))
            .ThenBy(kvp => kvp.Name, StringComparer.OrdinalIgnoreCase),
        keyComparer: StringComparer.OrdinalIgnoreCase,
        allowDuplicateValues: true);

    public static IEnumerable<(string Name, T Value)> GetPropertyValues<T>(IEnumerable<Type> types)
        => types
            .SelectMany(t => t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            .Where(p => p.PropertyType == typeof(T))
            .Select(p => (p.Name, (T)p.GetValue(null)!));

    public static readonly IReadOnlyDictionary<string, Decoration> DecorationByName 
        = EnumMappingByName<Decoration>();

    internal static readonly IReadOnlyDictionary<string, (Type Type, Action<SpectreDisplayOptions, object> Setter, Func<SpectreDisplayOptions, object> Getter)> SpectreDisplayOptionsProperties
        = SettersAndGettersByName<SpectreDisplayOptions>();

    public static Dictionary<string, T> EnumMappingByName<T>() 
        where T : struct, Enum
        => Enum.GetValues<T>()
            .ToDictionary(e => e.ToString(), e => e, StringComparer.OrdinalIgnoreCase);

    public static Dictionary<string, (Type Type, Action<T, object> Setter, Func<T, object> Getter)> SettersAndGettersByName<T>()
         => typeof(T)
            .GetProperties(
                System.Reflection.BindingFlags.Public 
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToDictionary(
                p => p.Name, 
                p => (
                    p.PropertyType,
                    (Action<T, object>)((options, value) => p.SetValue(options, value)),
                    (Func<T, object>)(options => p.GetValue(options)!)
                ),
                StringComparer.OrdinalIgnoreCase);

    public static int SortColorNames(string name)
        => s_consoleColorsByName.TryGetValue(name, out var value)
        ? (int)value
        : int.MaxValue; // Non-console colors go at the end
}
