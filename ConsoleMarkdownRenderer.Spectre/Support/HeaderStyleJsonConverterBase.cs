using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
public abstract class HeaderStyleJsonConverterBase<THeaderStyle> : JsonConverter<THeaderStyle>, IDefaultIdentifier
{
    public interface ITypeConfig
    {
        THeaderStyle Create(List<JsonProperty> properties, JsonSerializerOptions options);
        void Write(THeaderStyle style, Utf8JsonWriter writer,JsonSerializerOptions options);
        string TypeName { get; }
    }
    public class TypeConfig<TStyle>(
        Func<List<JsonProperty>, JsonSerializerOptions, TStyle> createFunc,
        Action<TStyle, Utf8JsonWriter, JsonSerializerOptions> writeAction
    ) : ITypeConfig
         where TStyle : THeaderStyle
    {
        private Func<List<JsonProperty>, JsonSerializerOptions, TStyle> _createFunc = createFunc;
        private Action<TStyle, Utf8JsonWriter, JsonSerializerOptions> _writeAction = writeAction;

        public THeaderStyle Create(List<JsonProperty> properties, JsonSerializerOptions options) => _createFunc(properties, options);
        public void Write(THeaderStyle style, Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            // Normally this kind of casting would be avoid by simply having an interface method
            // the problem is that for it to be accessible here it would need to be defined in this package
            // And we don't want types in the main package to leak types from this one 
            if (style is not TStyle typedStyle)
            {
                throw new JsonException($"Invalid style type: expected {typeof(TStyle).FullName}, got {style?.GetType().FullName}.");
            }
            _writeAction(typedStyle, writer, options);
        }
        public string TypeName => typeof(TStyle).Name;
    }

    public const string TypeDiscriminator = "$type";

    public HeaderStyleJsonConverterBase(IEnumerable<ITypeConfig> configs) 
        => Configs = configs.ToDictionary(c => c.TypeName, StringComparer.OrdinalIgnoreCase);

    public abstract bool? IsDefault(object value);

    public readonly IReadOnlyDictionary<string, ITypeConfig> Configs;

    private ITypeConfig GetConfig(string typeName) 
        => Configs.GetValueOrDefault(typeName) ?? throw new JsonException($"Unknown {nameof(THeaderStyle)} '{TypeDiscriminator}': '{typeName}'. Known types: {string.Join(", ", Configs.Keys)}.");

    public override THeaderStyle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        List<JsonProperty> properties = [];
        var typePropertyName = JsonWriteHelpers.ConvertName(TypeDiscriminator, options).ToLowerInvariant();

        string? typeName = null;
        foreach (var prop in root.EnumerateObject())
        {
            var propName = prop.Name.ToLowerInvariant();
            if (propName == typePropertyName)
            {
                typeName = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString();
            }
            else
            {
                properties.Add(prop);
            }
        }

        if (string.IsNullOrEmpty(typeName))
        {
            throw new JsonException($"Missing required discriminator '{TypeDiscriminator}' on {nameof(THeaderStyle)} entry.");
        }

        var typeConfig = GetConfig(typeName);
        return typeConfig.Create(properties, options);
    }

    public override void Write(Utf8JsonWriter writer, THeaderStyle value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        // The $type discriminator is a System.Text.Json convention and is
        // written literally (not transformed by PropertyNamingPolicy and never
        // skipped, even under DefaultIgnoreCondition).
        var typeName = value?.GetType().Name 
            ?? throw new JsonException($"Cannot determine type of {nameof(THeaderStyle)} value: value is null.");
        
        writer.WriteString(JsonWriteHelpers.ConvertName(TypeDiscriminator, options), typeName);
        var typeConfig = GetConfig(typeName);
        typeConfig.Write(value, writer,options);

        writer.WriteEndObject();
    }
}