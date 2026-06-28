using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
public static class JsonWriteHelpers
{
    public static string ConvertName(string name, JsonSerializerOptions options)
        => options.PropertyNamingPolicy?.ConvertName(name) ?? name;

    public static void WriteProperty<T>(Utf8JsonWriter writer, JsonSerializerOptions options, string name, T value)
    {
        if (ShouldIgnore(value, options, options.DefaultIgnoreCondition))
        {
            return;
        }
        writer.WritePropertyName(ConvertName(name, options));
        JsonSerializer.Serialize(writer, value, options);
    }

    internal static bool ShouldIgnore<T>(T value, JsonSerializerOptions options, JsonIgnoreCondition condition)
        => condition switch
        {
            JsonIgnoreCondition.Never => false,
            JsonIgnoreCondition.Always => true,  // This one seams strange, but you can't create a JsonSerializerOptions with this value 🤷
            JsonIgnoreCondition.WhenWritingNull => value is null,
            JsonIgnoreCondition.WhenWritingDefault => IsDefault(options, value),
#if NET10_0_OR_GREATER
            JsonIgnoreCondition.WhenWriting => true,
            JsonIgnoreCondition.WhenReading => false,
#endif
            _ => throw new InvalidOperationException($"Unexpected condition {condition}"),
        };

    internal static bool IsDefault<T>(JsonSerializerOptions options, T value)
    {
        if (value == null)
        {
            return true;
        }

        foreach (var converter in options.Converters)
        {
            if (converter is IDefaultIdentifier defaultIdentifier)
            {
                if (defaultIdentifier.IsDefault(value) is bool result)
                {
                    return result;
                }
            }
        }

        return value switch
        {
            string str => string.IsNullOrEmpty(str),
            _ => EqualityComparer<T>.Default.Equals(value, default),
        };
    }
}
