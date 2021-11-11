using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleEmphasisInlineRenderer : ConsoleObjectRenderer<EmphasisInline>
    {
        protected override void Write(ConsoleRenderer renderer, EmphasisInline obj)
        {
            string style;
            if (obj.DelimiterChar is '*' or '_')
            {
                style = obj.DelimiterCount > 1 ? "[bold]" : "[italic]";
            }
            else if (obj.DelimiterChar == '~')
            {
                // Hey, I'm sure there might be something better for subscript... but sometimes you have to make due with what you got 
                // And the blink does not seem to render well
                style = obj.DelimiterCount > 1
                    ? "[strikethrough]"      /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough"/>
                    : "[slowblink]";         /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Subscript"/>
            }
            else if (obj.DelimiterChar == '^')
            {
                // This another one.  Don't have an exact match for superscript
                style = "[rapidblink]";      /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Superscript"/>
            }
            else if (obj.DelimiterChar == '+' && obj.DelimiterCount == 2)
            {
                style = "[underline]";       /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Inserted"/>
            }
            else if (obj.DelimiterChar == '=' && obj.DelimiterCount == 2)
            {
                style = "[black on yellow]"; /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Marked"/>
            }
            else
            {
                // Yes, this more a style, but it should help identify where things need updating
                style = $"[dim]({obj.DelimiterChar}{obj.DelimiterCount})[/][invert]";
            }

            renderer
                .AddInLine(style)
                .WriteChildrenChain(obj)
                .AddInLine("[/]");
            
        }
    }
}