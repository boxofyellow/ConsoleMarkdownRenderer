using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Represents a link that we found with a markdown document
    /// </summary>
    public class LinkItem
    {
        public LinkItem(LinkInline link, string content)
        {
            Link = link;
            Content = content;
        }

        /// <summary>
        /// The Content included within the '['/']'
        /// </summary>
        public readonly string Content;

        /// <summary>
        /// The raw link inforation
        /// </summary>
        public readonly LinkInline Link;

        public override string ToString() 
            => $"{(Link.IsImage ? "!" : string.Empty)}[{Content}]({Link.Url})"
                // Workaround for https://github.com/spectreconsole/spectre.console/issues/608
                .Replace('[', '{')
                .Replace(']', '}');
    }
}