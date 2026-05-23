using Markdig.Extensions.Emoji;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    /// <summary>
    /// Renders Markdig's <see cref="EmojiInline"/> nodes (produced by
    /// <see cref="Markdig.MarkdownExtensions.UseEmojiAndSmiley(Markdig.MarkdownPipelineBuilder, bool)"/>).
    /// When <see cref="BoxOfYellow.ConsoleMarkdownRenderer.Spectre.SpectreDisplayOptions.Emojis"/> is <see langword="true"/> the resolved Unicode emoji
    /// character (<see cref="Markdig.Syntax.Inlines.LiteralInline.Content"/>) is written; otherwise
    /// the original shortcode or smiley text (<see cref="EmojiInline.Match"/>) is emitted.
    /// </summary>
    internal class ConsoleEmojiInlineRenderer : ConsoleObjectRenderer<EmojiInline>
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
}
