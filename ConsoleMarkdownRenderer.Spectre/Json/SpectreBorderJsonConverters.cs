using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Json
{
    /// <summary>
    /// JSON converter for Spectre.Console <see cref="TableBorder"/>. Serializes and deserializes
    /// by the border's class name (e.g., <c>"Square"</c>, <c>"Rounded"</c>).
    /// </summary>
    internal sealed class SpectreTableBorderJsonConverter : JsonConverter<TableBorder>
    {
        private static readonly Dictionary<string, TableBorder> s_namedBorders =
            typeof(TableBorder)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(TableBorder))
                .ToDictionary(p => p.Name, p => (TableBorder)p.GetValue(null)!, StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<TableBorder, string> s_borderNames =
            s_namedBorders
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

        public override TableBorder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.None && !reader.Read())
            {
                throw new JsonException("Expected a table border token.");
            }

            var name = reader.GetString();
            if (!string.IsNullOrEmpty(name) && s_namedBorders.TryGetValue(name, out var border))
            {
                return border;
            }
            return TableBorder.Square;
        }

        public override void Write(Utf8JsonWriter writer, TableBorder value, JsonSerializerOptions options)
        {
            if (s_borderNames.TryGetValue(value, out var name))
            {
                writer.WriteStringValue(SpectreJsonWriteHelpers.ConvertName(name, options));
            }
            else
            {
                writer.WriteStringValue(SpectreJsonWriteHelpers.ConvertName("Square", options));
            }
        }
    }

    /// <summary>
    /// JSON converter for Spectre.Console <see cref="BoxBorder"/>. Serializes and deserializes
    /// by the border's class name (e.g., <c>"Square"</c>, <c>"Rounded"</c>).
    /// </summary>
    internal sealed class SpectreBoxBorderJsonConverter : JsonConverter<BoxBorder?>
    {
        private static readonly Dictionary<string, BoxBorder> s_namedBorders =
            typeof(BoxBorder)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(BoxBorder))
                .ToDictionary(p => p.Name, p => (BoxBorder)p.GetValue(null)!, StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<BoxBorder, string> s_borderNames =
            s_namedBorders
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

        public override BoxBorder? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.None && !reader.Read())
            {
                throw new JsonException("Expected a box border token.");
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            var name = reader.GetString();
            if (!string.IsNullOrEmpty(name) && s_namedBorders.TryGetValue(name, out var border))
            {
                return border;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, BoxBorder? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }
            if (s_borderNames.TryGetValue(value, out var name))
            {
                writer.WriteStringValue(SpectreJsonWriteHelpers.ConvertName(name, options));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
