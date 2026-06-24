using System.Text;
using Markdig.Renderers;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    internal abstract partial class ConsoleRendererBase : RendererBase
    {
        private class LinkFrame
        {
            public void Append(string content) => m_content.Append(content);
            public string Content => m_content.ToString();
            private readonly StringBuilder m_content = new();
        }
    }
}