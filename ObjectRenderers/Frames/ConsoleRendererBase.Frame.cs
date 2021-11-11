using Markdig.Renderers;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public abstract partial class ConsoleRendererBase : RendererBase
    {
        private class Frame
        {
            public Frame(Style? borderStyle = default)
            {
                Table = new Table()
                    .HideHeaders();

                if (borderStyle == default)
                {
                    Table.NoBorder();
                }
                else
                {
                    Table.BorderStyle(borderStyle);
                }   
            }

            public readonly Table Table;
            public virtual void AddRow(IRenderable data) => Table.AddRow(data);
        }
    }
}