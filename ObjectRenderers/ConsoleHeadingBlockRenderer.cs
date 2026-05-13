using System.Text;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal class ConsoleHeadingBlockRenderer : ConsoleObjectRenderer<HeadingBlock>
    {
        protected override void Write(ConsoleRenderer renderer, HeadingBlock obj)
        {
            var style = renderer.Options.EffectiveHeader(obj.Level);

            if (style is FigletTextStyle figletStyle)
            {
                // FigletText cannot render an empty string, so when the heading text is empty
                // fall through to the styled-markup path so something readable is still emitted.
                var text = ExtractPlainText(obj);
                if (!string.IsNullOrEmpty(text))
                {
                    WriteFiglet(renderer, figletStyle, text);
                    return;
                }
            }

            WriteStyled(renderer, obj, style);
        }

        private static void WriteStyled(ConsoleRenderer renderer, HeadingBlock obj, IHeaderStyle style)
        {
            string leftWrap;
            string rightWrap;

            if (renderer.Options.WrapHeader)
            {
                string decorate = new('#', obj.Level);
                leftWrap = $"{decorate} ";
                rightWrap = $" {decorate}";
            }
            else
            {
                leftWrap = rightWrap = string.Empty;
            }

            var markup = style.ToSpectreStyle().ToMarkup();
            // Spectre.Console rejects an empty markup tag (e.g. `[]`), so when the resolved
            // style produces no markup we simply omit the wrapping tags.
            string openTag  = string.IsNullOrEmpty(markup) ? string.Empty : $"[{markup}]";
            string closeTag = string.IsNullOrEmpty(markup) ? string.Empty : "[/]";

            renderer
                .StartInline()
                .AddInLine(Environment.NewLine)
                .AddInLine(openTag)
                .AddInLine(leftWrap)
                .WriteLeafInline(obj)
                .AddInLine(rightWrap)
                .AddInLine(closeTag)
                .AddInLine(Environment.NewLine)
                .EndInline();
        }

        private static void WriteFiglet(ConsoleRenderer renderer, FigletTextStyle figletStyle, string text)
        {
            var figlet = figletStyle.FontPath is { } fontPath
                ? new FigletText(FigletFont.Load(fontPath), text)
                : new FigletText(text);
            if (figletStyle.Justification.HasValue)
            {
                figlet.Justification = figletStyle.Justification.Value.ToSpectreJustify();
            }
            if (figletStyle.Foreground is not null)
            {
                figlet.Color = figletStyle.Foreground.ToSpectreColor();
            }
            renderer.AddRenderable(figlet);
        }

        /// <summary>
        /// Walks the inline tree of <paramref name="block"/> and concatenates the literal text content.
        /// This is needed because Spectre.Console's <c>FigletText</c> takes a plain string (no markup),
        /// so we cannot reuse the inline-with-markup accumulation path used by the styled renderer.
        /// </summary>
        private static string ExtractPlainText(LeafBlock block)
        {
            var sb = new StringBuilder();
            if (block.Inline is not null)
            {
                AppendInline(sb, block.Inline);
            }
            return sb.ToString();
        }

        private static void AppendInline(StringBuilder sb, ContainerInline container)
        {
            foreach (var inline in container)
            {
                switch (inline)
                {
                    case LiteralInline literal:
                        sb.Append(literal.Content.ToString());
                        break;
                    case CodeInline code:
                        sb.Append(code.Content);
                        break;
                    case ContainerInline child:
                        AppendInline(sb, child);
                        break;
                }
            }
        }
    }
}
