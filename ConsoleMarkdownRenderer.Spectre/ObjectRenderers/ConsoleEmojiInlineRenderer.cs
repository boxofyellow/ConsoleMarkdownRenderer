using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Emoji;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleEmojiInlineRenderer : ConsoleObjectRendererBase<EmojiInline>
{
    protected override void Write(ConsoleRenderer renderer, EmojiInline obj)
    {
        if (renderer.Options.Emojis)
        {
            renderer.WriteEscape(ref obj.Content);
        }
        else
        {
            renderer.WriteEscape(obj.Match);
        }
    }
}
