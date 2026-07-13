using System.Text;
using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Syntax;
using Spectre.Console;
using Spectre.Console.Json;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleCodeBlockRenderer : ConsoleObjectRendererBase<CodeBlock>
{
    protected override void Write(ConsoleRenderer renderer, CodeBlock obj)
    {
        renderer.NewFrame();

        if (obj is FencedCodeBlock fenced
            && string.Equals(fenced.Info, "json", StringComparison.OrdinalIgnoreCase)
            && TryCreateJsonText(obj, renderer.Options, out var jsonText))
        {
            renderer.AddRenderable(jsonText);
        }
        else
        {
            renderer.AddFilledBlock(obj, renderer.Options.CodeBlock, indent: "  ");
        }

        if (renderer.Options.ShowFencedCodeBlockInfo && obj is FencedCodeBlock infoFenced && !string.IsNullOrEmpty(infoFenced.Info))
        {
            var style = renderer.Options.FencedCodeBlockInfo;
            var header = $"[{style.ToMarkup()}]{Markup.Escape(infoFenced.Info)}[/]";
            renderer.CompleteFrame(t => new Panel(t)
            {
                Border = renderer.Options.FencedCodeBlockInfoPanelBorder,
                BorderStyle = style,
                Header = new PanelHeader(header),
            });
        }
        else
        {
            renderer.CompleteFrame();
        }
    }

    // JsonText parses lazily while rendering and throws on malformed JSON, so validate up front
    // and fall back to the plain code block rendering when the content is not valid JSON.
    private static bool TryCreateJsonText(CodeBlock obj, SpectreDisplayOptions options, out JsonText jsonText)
    {
        var text = GetText(obj);
        try
        {
            using var _ = JsonDocument.Parse(text);
        }
        catch (JsonException)
        {
            jsonText = null!;
            return false;
        }

        jsonText = new JsonText(text)
        {
            BracesStyle = options.JsonBraces,
            BracketsStyle = options.JsonBrackets,
            MemberStyle = options.JsonMember,
            ColonStyle = options.JsonColon,
            CommaStyle = options.JsonComma,
            StringStyle = options.JsonString,
            NumberStyle = options.JsonNumber,
            BooleanStyle = options.JsonBoolean,
            NullStyle = options.JsonNull,
        };
        return true;
    }

    private static string GetText(CodeBlock obj)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < obj.Lines.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('\n');
            }
            builder.Append(obj.Lines.Lines[i].Slice.ToString());
        }
        return builder.ToString();
    }
}
