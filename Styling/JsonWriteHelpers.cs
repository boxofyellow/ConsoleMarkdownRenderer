using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Internal helpers shared by the styling JSON converters for honouring the
    /// caller-supplied <see cref="JsonSerializerOptions"/> when writing. Currently respects:
    /// <list type="bullet">
    /// <item><see cref="JsonSerializerOptions.PropertyNamingPolicy"/> — applied to the
    /// property names the converters emit literally (e.g. <c>"justification"</c>,
    /// <c>"foreground"</c>, <c>"named"</c>, <c>"r"</c>/<c>"g"</c>/<c>"b"</c>).</item>
    /// <item><see cref="JsonSerializerOptions.DefaultIgnoreCondition"/> — values that
    /// match <see cref="JsonIgnoreCondition.WhenWritingNull"/> /
    /// <see cref="JsonIgnoreCondition.WhenWritingDefault"/> are skipped when written
    /// through <see cref="WriteProperty{T}"/>.</item>
    /// </list>
    /// Other options that affect output (<see cref="JsonSerializerOptions.WriteIndented"/>
    /// and <see cref="JsonSerializerOptions.Encoder"/>) are honoured automatically by the
    /// <see cref="Utf8JsonWriter"/> that <see cref="JsonSerializer"/> hands the converter.
    /// </summary>
    internal static class JsonWriteHelpers
    {
        /// <summary>
        /// Converts the literal property name the converter would emit into the form
        /// requested by <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>. When no
        /// naming policy is set the name is returned unchanged.
        /// </summary>
        internal static string ConvertName(string name, JsonSerializerOptions options)
            => options.PropertyNamingPolicy?.ConvertName(name) ?? name;

        /// <summary>
        /// Writes a property to <paramref name="writer"/> while honouring
        /// <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> and
        /// <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/>. The value itself is
        /// serialized through <see cref="JsonSerializer.Serialize{TValue}(Utf8JsonWriter, TValue, JsonSerializerOptions)"/>
        /// so any converters / enum policies the caller configured are honoured too.
        /// </summary>
        internal static void WriteProperty<T>(Utf8JsonWriter writer, JsonSerializerOptions options, string name, T value)
        {
            if (ShouldIgnore(value, options.DefaultIgnoreCondition))
            {
                return;
            }
            writer.WritePropertyName(ConvertName(name, options));
            JsonSerializer.Serialize(writer, value, options);
        }

        private static bool ShouldIgnore<T>(T value, JsonIgnoreCondition condition)
            => condition switch
            {
                JsonIgnorecondition.Never => false,
                JsonIgnoreCondition.Always => true,
                JsonIgnoreCondition.WhenWritingNull => value is null,
                JsonIgnoreCondition.WhenWritingDefault => EqualityComparer<T>.Default.Equals(value, default!),
                JsonIgnoreCondition.WhenWriting => true,
                JsonIgnoreCondition.WhenReading => false,
                _ => throw new InvalidOperationException($"Unexpected condition {condition}"),
            };
    }
}
