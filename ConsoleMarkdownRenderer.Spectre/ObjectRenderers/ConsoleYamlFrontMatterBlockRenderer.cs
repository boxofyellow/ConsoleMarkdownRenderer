using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Yaml;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

/// <summary>
/// Renders a <see cref="YamlFrontMatterBlock"/> — the optional YAML metadata block delimited by
/// <c>---</c> at the top of a Markdown document, as parsed by Markdig's
/// <see cref="Markdig.MarkdownExtensions.UseYamlFrontMatter(Markdig.MarkdownPipelineBuilder)"/> extension.
/// The raw YAML source is emitted inside a styled frame using
/// <see cref="DisplayOptions.YamlFrontMatter"/>.
/// </summary>
[SpectreSourceFile]
internal class ConsoleYamlFrontMatterBlockRenderer : ConsoleObjectRendererBase<YamlFrontMatterBlock>
{
    protected override void Write(ConsoleRenderer renderer, YamlFrontMatterBlock obj)
        => renderer
            .NewFrame()
            .AddFilledBlock(obj, renderer.Options.YamlFrontMatter, fence: "---")
            .CompleteFrame();
}
