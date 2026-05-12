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

            switch (style)
            {
                case FigletTextStyle figletStyle:
                    WriteFiglet(renderer, obj, figletStyle);
                    return;
                case TextStyle textStyle:
                    WriteStyled(renderer, obj, textStyle);
                    return;
                default:
                    // Defensive: unknown IHeaderStyle implementation. Fall back to the plain styled
                    // path with no extra styling so we still emit something readable.
                    WriteStyled(renderer, obj, TextStyle.Plain);
                    return;
            }
        }

        private static void WriteStyled(ConsoleRenderer renderer, HeadingBlock obj, TextStyle style)
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

            renderer
                .StartInline()
                .AddInLine(Environment.NewLine)
                .AddInLine($"[{style.ToSpectreStyle().ToMarkup()}]")
                .AddInLine(leftWrap)
                .WriteLeafInline(obj)
                .AddInLine(rightWrap)
                .AddInLine("[/]")
                .AddInLine(Environment.NewLine)
                .EndInline();
        }

        private static void WriteFiglet(ConsoleRenderer renderer, HeadingBlock obj, FigletTextStyle figletStyle)
        {
            var text = ExtractPlainText(obj);
            // FigletText itself does not render anything when given an empty string, so fall back to a single
            // space to avoid the surrounding frame collapsing for a stray empty heading.
            var figlet = new FigletText(string.IsNullOrEmpty(text) ? " " : text);
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