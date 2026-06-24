using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support
{
    [SpectreSourceFile]
    public abstract class ColorJsonConverterBase<TColor> : JsonConverter<TColor>, IDefaultIdentifier
        where TColor : notnull
    {
        public const string IsDefaultDiscriminator = "isDefault";
        public const string NamedDiscriminator = "named";
        public const string ConsoleColorDiscriminator = "consoleColor";

        protected abstract TColor DefaultColor { get; }
        protected abstract TColor Create(ConsoleColor? consoleColor, byte r, byte g, byte b);
        protected abstract (byte R, byte G, byte B) ToRgb(TColor color);

        protected abstract BidirectionalMap<string, TColor> ColorMap { get; }

        public override TColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var element = doc.RootElement;

            byte r = 0, g = 0, b = 0;
            bool isDefault = false;
            string? named = null;
            ConsoleColor? consoleColor = null;

            var isDefaultPropertyName = JsonWriteHelpers.ConvertName(IsDefaultDiscriminator, options).ToLowerInvariant();
            var namedPropertyName = JsonWriteHelpers.ConvertName(NamedDiscriminator, options).ToLowerInvariant();
            var consoleColorPropertyName = JsonWriteHelpers.ConvertName(ConsoleColorDiscriminator, options).ToLowerInvariant();
            var rPropertyName = JsonWriteHelpers.ConvertName(nameof(Color.R), options).ToLowerInvariant();
            var gPropertyName = JsonWriteHelpers.ConvertName(nameof(Color.G), options).ToLowerInvariant();
            var bPropertyName = JsonWriteHelpers.ConvertName(nameof(Color.B), options).ToLowerInvariant();

            foreach (var prop in element.EnumerateObject())
            {
                var propName = prop.Name.ToLowerInvariant();
                if (propName == isDefaultPropertyName)
                {
                    isDefault = prop.Value.GetBoolean();
                    if (!isDefault)
                    {
                        throw new JsonException($"'{IsDefaultDiscriminator}' property must be true if present.");
                    }
                }
                else if (propName == namedPropertyName)
                {
                    named = prop.Value.GetString();
                }
                else if (propName == consoleColorPropertyName)
                {
                    consoleColor = prop.Value.Deserialize<ConsoleColor>(options);
                }
                else if (propName == rPropertyName)
                {
                    r = prop.Value.GetByte();
                }
                else if (propName == gPropertyName)
                {
                    g = prop.Value.GetByte();
                }
                else if (propName == bPropertyName)
                {
                    b = prop.Value.GetByte();
                }
                else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
                {
                    throw new JsonException($"Unrecognized property on Color: '{prop.Name}'.");
                }
            }

            if (isDefault)
            {
                if (named != null || consoleColor != null || r != 0 || g != 0 || b != 0)
                {
                    throw new JsonException($"'{IsDefaultDiscriminator}' property cannot be combined with other properties.");
                }
                return DefaultColor;
            }
            else if (!string.IsNullOrEmpty(named))
            {
                if (consoleColor != null || r != 0 || g != 0 || b != 0)
                {
                    throw new JsonException($"'{NamedDiscriminator}' property cannot be combined with other properties.");
                }
                if (ColorMap.Forward.TryGetValue(named, out var color))
                {
                    return color;
                }
                else
                {
                    throw new JsonException($"Unknown color name: '{named}'. known names are: {string.Join(", ", ColorMap.Forward.Keys)}");
                }
            }
            else
            {
                if (consoleColor != null && (r != 0 || g != 0 || b != 0))
                {
                    throw new JsonException($"'{ConsoleColorDiscriminator}' property cannot be combined with RGB properties.");
                }
                return Create(consoleColor, r, g, b);
            }
        }

        public override void Write(Utf8JsonWriter writer, TColor value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.Equals(DefaultColor))
            {
                JsonWriteHelpers.WriteProperty(writer, options, IsDefaultDiscriminator, true);
            }
            else
            {
                if (ColorMap.Reverse.TryGetValue(value, out var name))
                {
                    JsonWriteHelpers.WriteProperty(writer, options, NamedDiscriminator, name);
                }
                else
                {
                    var rgb = ToRgb(value);
                    JsonWriteHelpers.WriteProperty(writer, options, nameof(rgb.R), rgb.R);
                    JsonWriteHelpers.WriteProperty(writer, options, nameof(rgb.G), rgb.G);
                    JsonWriteHelpers.WriteProperty(writer, options, nameof(rgb.B), rgb.B);
                }
            }
            writer.WriteEndObject();
        }

        public bool? IsDefault(object value) 
            => value switch
            {
                TColor color => color.Equals(DefaultColor),
                _ => null
            };
    }
}