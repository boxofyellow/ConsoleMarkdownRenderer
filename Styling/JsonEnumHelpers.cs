using System.Text.Json;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Internal helpers that read enum values from <see cref="JsonElement"/> without relying
    /// on a <see cref="System.Text.Json.Serialization.JsonStringEnumConverter"/> being present
    /// in the surrounding <see cref="JsonSerializerOptions"/>. Both string ("Bold, Underline")
    /// and numeric (the underlying integer) forms are accepted.
    /// </summary>
    internal static class JsonEnumHelpers
    {
        internal static T Read<T>(JsonElement element) where T : struct, Enum
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    var text = element.GetString();
                    if (Enum.TryParse<T>(text, ignoreCase: true, out var parsed))
                    {
                        return parsed;
                    }
                    throw new JsonException($"Unknown {typeof(T).Name} value '{text}'.");
                case JsonValueKind.Number:
                    return (T)Enum.ToObject(typeof(T), element.GetInt64());
                default:
                    throw new JsonException($"Unexpected token {element.ValueKind} for {typeof(T).Name}.");
            }
        }

        internal static T? ReadNullable<T>(JsonElement element) where T : struct, Enum
            => element.ValueKind == JsonValueKind.Null ? null : Read<T>(element);
    }
}
