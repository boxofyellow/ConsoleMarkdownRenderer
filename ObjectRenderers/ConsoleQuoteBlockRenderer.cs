using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig.Syntax;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    /// <summary>
    /// Renders a Markdown <see cref="QuoteBlock"/>. When
    /// <see cref="DisplayOptions.UseBorderForQuotedBlock"/> is <see langword="true"/>
    /// (the default) the quoted content is wrapped in a Spectre.Console
    /// <see cref="Panel"/> so it is visually delineated from surrounding text. When
    /// <see langword="false"/> the legacy behavior is used: an unbordered (or
    /// debug-bordered) frame with only the <see cref="DisplayOptions.QuotedBlock"/>
    /// style applied to the contents.
    /// </summary>
    internal class ConsoleQuoteBlockRenderer : ConsoleObjectRenderer<QuoteBlock>
    {
        protected override void Write(ConsoleRenderer renderer, QuoteBlock obj)
        {
            if (renderer.Options.UseBorderForQuotedBlock)
            {
                // Render the children into an unbordered inner frame and then wrap that
                // frame in a Spectre.Console Panel so the blockquote is visually delineated
                // by a border from the surrounding text.
                renderer
                    .NewFrame()
                    .PushStyle(renderer.Options.QuotedBlock.ToSpectreStyle())
                    .StartInline()
                    .WriteChildrenChain(obj)
                    .EndInline()
                    .PopStyle()
                    .CompleteFrameAsPanel();
            }
            else
            {
                renderer
                    .NewFrame(borderStyle: Style.Plain)
                    .PushStyle(renderer.Options.QuotedBlock.ToSpectreStyle())
                    .StartInline()
                    .WriteChildrenChain(obj)
                    .EndInline()
                    .PopStyle()
                    .CompleteFrame();
            }
        }
    }
}
