using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Alerts;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleAlertBlockRenderer : ConsoleObjectRendererBase<AlertBlock>
{
    protected override void Write(ConsoleRenderer renderer, AlertBlock obj)
    {
        renderer
            .NewFrame()
            .PushStyle(renderer.Options.QuotedBlock)
            .WriteChildrenChain(obj)
            .PopStyle();

        var kind = obj.Kind.ToString();
        var style = renderer.Options.EffectiveAlert(kind);
        var header = $"[{style.ToMarkup()}]{Markup.Escape(kind.ToUpperInvariant())}[/]";

        renderer.CompleteFrame(t =>
        {
            return new Panel(t)
            {
                Border = renderer.Options.AlertPanelBorder,
                BorderStyle = style,
                Header = new PanelHeader(header),
            };
        });        
    }
}
