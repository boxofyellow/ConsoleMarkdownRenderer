using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Alerts;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleAlertBlockRenderer : ConsoleObjectRendererBase<AlertBlock>
{
    protected override void Write(ConsoleRenderer renderer, AlertBlock obj)
    {
        var kind = obj.Kind.ToString();
        if (renderer.Options.AlertUsePanelBorder)
        {
            var style = renderer.Options.EffectiveAlert(kind);
            renderer
                .NewFrame()
                .PushStyle(renderer.Options.QuotedBlock)
                .WriteChildrenChain(obj)
                .PopStyle();

            var content = renderer.CompleteFrame(addToParent: false);
            var header = $"[{style.ToMarkup()}]{Markup.Escape(kind.ToUpperInvariant())}[/]";
            var panel = new Panel(content)
            {
                Border = renderer.Options.AlertPanelBorder,
                BorderStyle = style,
                Header = new PanelHeader(header),
            };
            renderer.AddRenderable(panel);
            return;
        }

        renderer
            .NewFrame(borderStyle: Style.Plain)
            .StartInline()
            .AddInLine($"[{renderer.Options.EffectiveAlert(kind).ToMarkup()}]")
            .WriteEscape($"[{kind.ToUpperInvariant()}]")
            .AddInLine("[/]")
            .EndInline()
            .PushStyle(renderer.Options.QuotedBlock)
            .WriteChildrenChain(obj)
            .PopStyle()
            .CompleteFrame();
    }
}
