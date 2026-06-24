using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter
{
    [SourceFile]
    internal sealed class DisplayOptionsJsonConverter : MappedJsonConverterBase<DisplayOptions>
    {
        public DisplayOptionsJsonConverter(Func<DisplayOptions>? createObject) => CreateObjectFunction = createObject;

        protected override IReadOnlyDictionary<string, Action<DisplayOptions, JsonSerializerOptions, JsonElement>> Deserializers 
            => DisplayOptions.Deserializers;

        protected override IReadOnlyList<Action<DisplayOptions, Utf8JsonWriter, JsonSerializerOptions>> Serializers 
            => DisplayOptions.Serializers;

        protected override Func<DisplayOptions> CreateObject => CreateObjectFunction ?? (() => new DisplayOptions());

        internal readonly Func<DisplayOptions>? CreateObjectFunction;
    }
}