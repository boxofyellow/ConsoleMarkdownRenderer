using System.Runtime.CompilerServices;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig;

[assembly: InternalsVisibleTo("ConsoleMarkdownRenderer.Spectre.Tests")]
[assembly: InternalsVisibleTo("ConsoleMarkdownRenderer.Tests")]
[assembly: InternalsVisibleTo("BoxOfYellow.ConsoleMarkdownRenderer.Fakes")]

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre;

[SpectreSourceFile]
public sealed class MarkdownRenderer : ISpectreMarkdownRenderer
{
    public MarkdownRenderResult Render(string text, SpectreDisplayOptions? options = null)
        => Render(text, options, rendererOverride: default);

    internal MarkdownRenderResult Render(string text, SpectreDisplayOptions? options, ConsoleRenderer? rendererOverride = null)
    {
        options ??= new SpectreDisplayOptions();
        var pipeline = BuildPipeline(options);
        var renderer = rendererOverride ?? new ConsoleRenderer(options);
        pipeline.Setup(renderer);

        var document = Markdown.Parse(text, pipeline);
        renderer.Render(document);
        
        return new MarkdownRenderResult
        {
            Root = renderer.Root,
            Links = renderer.Links,
            UnhandledTypes = renderer.UnhandledTypes ?? new HashSet<Type>(),
            UnknownEmphasisDelimiters = renderer.UnknownEmphasisDelimiters ?? new HashSet<UnknownEmphasisDelimiter>(),
        };
    }

    internal static MarkdownPipeline BuildPipeline(SpectreDisplayOptions options)
    {
        var builder = new MarkdownPipelineBuilder()
            .UseEmojiAndSmiley()
            .UseYamlFrontMatter();
        if (options.SmartyPants)
        {
            builder.UseSmartyPants();
        }

        // All Calls that **_ADD_** extensions should come before adding advanced
        builder.UseAdvancedExtensions()
        // All Calls that **_MODIFY_** extensions should come after adding advanced
                .UseAlertBlocks(allowNestedAlerts: true)
                .UseCjkFriendlyEmphasis();
        return builder.Build();
    }
}