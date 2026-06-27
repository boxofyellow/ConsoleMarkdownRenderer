using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter
{
    [SourceFile]
    internal sealed class TextStyleJsonConverter : JsonConverter<TextStyle>
    {
        public override TextStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var element = doc.RootElement;

            List<JsonProperty> properties = element.EnumerateObject().ToList();
            return new TextStyle(properties, options);
        }

        public override void Write(Utf8JsonWriter writer, TextStyle value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            value.Write(writer, options);
            writer.WriteEndObject();
        }
    }
}
