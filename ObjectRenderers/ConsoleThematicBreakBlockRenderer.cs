using Markdig.Syntax;
using Spectre.Console;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleThematicBreakBlockRenderer : ConsoleObjectRenderer<ThematicBreakBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ThematicBreakBlock obj)
            => renderer.AddThematicBreak();
    }
}
