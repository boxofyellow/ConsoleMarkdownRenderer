using Markdig.Syntax.Inlines;
using Spectre.Console;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleEmphasisInlineRenderer : ConsoleObjectRenderer<EmphasisInline>
    {
        protected override void Write(ConsoleRenderer renderer, EmphasisInline obj)
        {
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
                // Yes, this more a style, but it should help identify where things need updating
                renderer.AddInLine($"[{renderer.Options.UnknownDelimiterChar.ToMarkup()}]({obj.DelimiterChar}{obj.DelimiterCount})[/]");
                style = renderer.Options.UnknownDelimiterContent;
            }

            // Maybe this should be changed to leave PushStyle/PopStyle
            renderer
                .AddInLine($"[{style.ToMarkup()}]")
                .WriteChildrenChain(obj)
                .AddInLine("[/]");
        }
    }
}