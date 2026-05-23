using Markdig.Extensions.CustomContainers;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal class ConsoleCustomContainerRenderer : ConsoleObjectRenderer<CustomContainer>
    {
        protected override void Write(ConsoleRenderer renderer, CustomContainer obj)
        {
            renderer.NewFrame(borderStyle: Style.Plain);
            if (!string.IsNullOrEmpty(obj.Info))
            {
                renderer
                    .StartInline()
                    .AddInLine($"[{renderer.Options.CustomContainerInfo.ToMarkup()}]")
                    .WriteEscape(obj.Info)
                    .AddInLine("[/]")
                    .EndInline();
            }
            renderer
                .PushStyle(renderer.Options.CustomContainer)
                .WriteChildrenChain(obj)
                .PopStyle()
                .CompleteFrame();
        }
    }
}
