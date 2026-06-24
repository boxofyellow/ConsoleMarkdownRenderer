using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter
{
    [SpectreSourceFile]
    internal sealed class SpectreDisplayOptionsJsonConverter : MappedJsonConverterBase<SpectreDisplayOptions>
    {
        public SpectreDisplayOptionsJsonConverter(Func<SpectreDisplayOptions>? createObject) => CreateObjectFunction = createObject;

        protected override IReadOnlyDictionary<string, Action<SpectreDisplayOptions, JsonSerializerOptions, JsonElement>> Deserializers 
            => SpectreDisplayOptions.Deserializers;

        protected override IReadOnlyList<Action<SpectreDisplayOptions, Utf8JsonWriter, JsonSerializerOptions>> Serializers 
            => SpectreDisplayOptions.Serializers;

        protected override Func<SpectreDisplayOptions> CreateObject => CreateObjectFunction ?? (() => new SpectreDisplayOptions());

        internal readonly Func<SpectreDisplayOptions>? CreateObjectFunction;
    }
}