namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Represents a link that we found within a markdown document
    /// </summary>
    public class LinkItem
    {
        public LinkItem(string url, string content, bool isImage = false)
        {
            Url = url;
            Content = content;
            IsImage = isImage;
        }

        /// <summary>
        /// The Content included within the '['/']'
        /// </summary>
        public readonly string Content;

        /// <summary>
        /// The URL target of the link
        /// </summary>
        public readonly string Url;

        /// <summary>
        /// Whether this is an image link
        /// </summary>
        public readonly bool IsImage;

        public override string ToString() 
            => $"{(IsImage ? "!" : string.Empty)}[{Content}]({Url})";
    }
}