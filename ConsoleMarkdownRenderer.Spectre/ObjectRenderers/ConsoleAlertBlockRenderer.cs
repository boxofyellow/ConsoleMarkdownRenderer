using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Alerts;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    [SpectreSourceFile]
    internal class ConsoleAlertBlockRenderer : ConsoleObjectRendererBase<AlertBlock>
    {
        protected override void Write(ConsoleRenderer renderer, AlertBlock obj)
        {
            var kind = obj.Kind.ToString();
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
}
