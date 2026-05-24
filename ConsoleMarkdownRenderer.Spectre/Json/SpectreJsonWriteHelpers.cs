using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Json
{
    /// <summary>
    /// Helpers shared by the Spectre JSON converters for honouring the
    /// caller-supplied <see cref="JsonSerializerOptions"/> when writing.
    /// </summary>
    public static class SpectreJsonWriteHelpers
    {
        public static string ConvertName(string name, JsonSerializerOptions options)
            => options.PropertyNamingPolicy?.ConvertName(name) ?? name;

        public static void WriteProperty<T>(Utf8JsonWriter writer, JsonSerializerOptions options, string name, T value)
        {
            if (ShouldIgnore(value, options.DefaultIgnoreCondition))
            {
                return;
            }
            writer.WritePropertyName(ConvertName(name, options));
            JsonSerializer.Serialize(writer, value, options);
        }

        public static bool ShouldIgnore<T>(T value, JsonIgnoreCondition condition)
            => condition switch
            {
                JsonIgnoreCondition.Never => false,
                JsonIgnoreCondition.Always => true,
                JsonIgnoreCondition.WhenWritingNull => value is null,
                JsonIgnoreCondition.WhenWritingDefault => EqualityComparer<T>.Default.Equals(value, default!),
#if NET10_0_OR_GREATER
                JsonIgnoreCondition.WhenWriting => true,
                JsonIgnoreCondition.WhenReading => false,
#endif
                _ => throw new InvalidOperationException($"Unexpected condition {condition}"),
            };
    }
}
