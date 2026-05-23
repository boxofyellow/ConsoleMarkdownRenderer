using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig.Extensions.Yaml;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    /// <summary>
    /// Renders a <see cref="YamlFrontMatterBlock"/> — the optional YAML metadata block delimited by
    /// <c>---</c> at the top of a Markdown document, as parsed by Markdig's
    /// <see cref="Markdig.MarkdownExtensions.UseYamlFrontMatter(Markdig.MarkdownPipelineBuilder)"/> extension.
    /// The raw YAML source is emitted inside a styled frame using
    /// <see cref="DisplayOptions.YamlFrontMatter"/>.
    /// </summary>
    internal class ConsoleYamlFrontMatterBlockRenderer : ConsoleObjectRenderer<YamlFrontMatterBlock>
    {
        protected override void Write(ConsoleRenderer renderer, YamlFrontMatterBlock obj)
        {
            renderer
                .NewFrame()
                .PushStyle(renderer.Options.YamlFrontMatter.ToSpectreStyle())
                .StartInline()
                .AddInLine("---")
                .AddInLine(Environment.NewLine);

            for (int i = 0; i < obj.Lines.Lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(obj.Lines.Lines[i].Slice.Text))
                {
                    renderer
                        .WriteEscape(ref obj.Lines.Lines[i].Slice)
                        .AddInLine(Environment.NewLine);
                }
            }

            renderer
                .AddInLine("---")
                .EndInline()
                .PopStyle()
                .CompleteFrame();
        }
    }
}
