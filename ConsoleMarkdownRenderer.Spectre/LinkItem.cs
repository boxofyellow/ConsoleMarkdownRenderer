using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    [SpectreSourceFile]
    public sealed class LinkItem
    {
        public LinkItem(string url, string content, bool isImage = false)
        {
            Url = url;
            Content = content;
            IsImage = isImage;
        }

        public readonly string Content;

        public readonly string Url;

        public readonly bool IsImage;

        public override string ToString() 
            => $"{(IsImage ? "!" : string.Empty)}[{Content}]({Url})";
    }
}