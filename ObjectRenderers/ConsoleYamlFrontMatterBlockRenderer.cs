using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig.Extensions.Yaml;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    /// <summary>
    /// Renders a <see cref="YamlFrontMatterBlock"/> — the optional YAML metadata block delimited by
    /// <c>---</c> at the top of a Markdown document, as parsed by Markdig's
    /// <see cref="Markdig.MarkdownExtensions.UseYamlFrontMatter(Markdig.MarkdownPipelineBuilder)"/> extension.
    /// By default the block is silently suppressed (matching how most HTML renderers handle it).
    /// When <see cref="DisplayOptions.ShowYamlFrontMatter"/> is <see langword="true"/>, the raw YAML
    /// source is emitted inside a styled frame using <see cref="DisplayOptions.YamlFrontMatter"/>.
    /// </summary>
    internal class ConsoleYamlFrontMatterBlockRenderer : ConsoleObjectRenderer<YamlFrontMatterBlock>
    {
        protected override void Write(ConsoleRenderer renderer, YamlFrontMatterBlock obj)
        {
            if (!renderer.Options.ShowYamlFrontMatter)
            {
                return;
            }

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
