using System.Text;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    internal class ConsoleHeadingBlockRenderer : ConsoleObjectRenderer<HeadingBlock>
    {
        protected override void Write(ConsoleRenderer renderer, HeadingBlock obj)
        {
            var style = renderer.Options.EffectiveHeader(obj.Level);
            var text = ExtractPlainText(obj);

            if (style is SpectreFigletHeaderStyle figlet)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var figletText = figlet.Font is { } font
                        ? new FigletText(font, text)
                        : new FigletText(text);
                    if (figlet.Justification.HasValue)
                    {
                        figletText.Justification = figlet.Justification.Value;
                    }
                    if (figlet.Foreground.HasValue)
                    {
                        figletText.Color = figlet.Foreground.Value;
                    }
                    renderer.AddRenderable(figletText);
                }
                return;
            }

            if (style is SpectreRuleHeaderStyle rule)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var titleMarkup = Markup.Escape(text);
                    if (rule.Foreground.HasValue)
                    {
                        titleMarkup = $"[{rule.Foreground.Value.ToMarkup()}]{titleMarkup}[/]";
                    }
                    var ruleWidget = new Rule(titleMarkup);
                    if (rule.Justification.HasValue)
                    {
                        ruleWidget.Justification = rule.Justification.Value;
                    }
                    if (rule.Border is not null)
                    {
                        ruleWidget.Border = rule.Border;
                    }
                    renderer.AddRenderable(ruleWidget);
                }
                return;
            }

            if (style is SpectreStyleHeaderStyle styleHeader)
            {
                WriteStyled(renderer, obj, styleHeader.Style);
                return;
            }

            throw new InvalidOperationException($"Unsupported {nameof(ISpectreHeaderStyle)} implementation: {style?.GetType().FullName ?? "(null)"}");
        }

        private static void WriteStyled(ConsoleRenderer renderer, HeadingBlock obj, Style style)
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

            var markup = style.ToMarkup();
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
