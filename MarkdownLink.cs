namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Represents a link found within a markdown document.
    /// This is a dependency-free type safe for use in public interfaces.
    /// </summary>
    public class MarkdownLink
    {
        public MarkdownLink(string url, string content, bool isImage = false)
        {
            Url = url;
            Content = content;
            IsImage = isImage;
        }

        /// <summary>
        /// The URL target of the link
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// The content included within the '['/']'
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Whether this is an image link
        /// </summary>
        public bool IsImage { get; }

        public override string ToString()
            => $"{(IsImage ? "!" : string.Empty)}[{Content}]({Url})";
    }
}
