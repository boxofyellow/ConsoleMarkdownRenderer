using System.Diagnostics.CodeAnalysis;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Syntax.Inlines;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    [SpectreSourceFile]
    internal class ConsoleEmphasisInlineRenderer : ConsoleObjectRendererBase<EmphasisInline>
    {
        protected override void Write(ConsoleRenderer renderer, EmphasisInline obj)
        {
            if (TryGetCitationContent(obj, out var citationContent))
            {
                renderer
                    .AddInLine($"[{renderer.Options.Citation.ToMarkup()}]")
                    .WriteChildrenChain(citationContent)
                    .AddInLine("[/]");
                return;
            }

            Style style;
            if (obj.DelimiterChar is '*' or '_')
            {
                style = obj.DelimiterCount > 1
                    ? renderer.Options.Bold
                    : renderer.Options.Italic;
            }
            else if (obj.DelimiterChar == '~')
            {
                style = obj.DelimiterCount > 1
                    ? renderer.Options.Strikethrough
                    : renderer.Options.Subscript;
            }
            else if (obj.DelimiterChar == '^')
            {
                style = renderer.Options.Superscript;
            }
            else if (obj.DelimiterChar == '+' && obj.DelimiterCount == 2)
            {
                style = renderer.Options.Inserted;
            }
            else if (obj.DelimiterChar == '=' && obj.DelimiterCount == 2)
            {
                style = renderer.Options.Marked;
            }
            else
            {
                renderer.RecordUnknownEmphasisDelimiter(obj.DelimiterChar, obj.DelimiterCount);
                // Yes, this is more than a style, but it should help identify where things need updating
                renderer.AddInLine($"[{renderer.Options.UnknownDelimiterChar.ToMarkup()}]({obj.DelimiterChar}{obj.DelimiterCount})[/]");
                style = renderer.Options.UnknownDelimiterContent;
            }

            // Maybe this should be changed to leave PushStyle/PopStyle
            renderer
                .AddInLine($"[{style.ToMarkup()}]")
                .WriteChildrenChain(obj)
                .AddInLine("[/]");
        }

        private static bool TryGetCitationContent(EmphasisInline obj, [NotNullWhen(true)] out EmphasisInline? citationContent)
        {
            if (obj.FirstChild is EmphasisInline firstChild
                && obj.DelimiterChar == '^'
                && obj.DelimiterCount == 1
                && firstChild.NextSibling is null
                && firstChild.DelimiterChar == '^'
                && firstChild.DelimiterCount == 1)
            {
                citationContent = firstChild;
                return true;
            }

            citationContent = null;
            return false;
        }
    }
}
