using System;
using System.Threading.Tasks;

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// This is the main class for this assembly and contains the method that is expected to be consumed by others
    /// It mainly provide a method <see cref="DisplayMarkdownAsync"/> (with a few overloads) for displaying markdown content in console
    /// </summary>
    public static class Displayer
    {
        /// <summary>
        /// Will display markdown content from the provided uri (local or from the web)
        /// Optionally after the markdown is displayed, a list of links from the document are presented and the user can select them to view more content
        /// The intent is to display information/documentation so it is treated as best effort,
        /// any problems like (missing file or problems downloading content) are displayed in line as opposed to exceptions that bubble out.
        /// 
        /// Selected links are handled in the following way
        ///  - If they yield markdown, that content is displayed
        ///  - If they yield an image and it looks like this being run in iTerm2 (https://iterm2.com/) it will be displayed inline
        ///  - For everything else it is thrown at the OS to see if it can sort it out.
        /// </summary>
        /// <param name="uri">The uri to pull the content from</param>
        /// <param name="options">options to control how to display the content</param>
        /// <param name="allowFollowingLinks">when set to true, the list of links will be provided, when false the list is omitted</param>
        public static async Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            using var displayer = new MarkdownDisplayer();
            await displayer.DisplayMarkdownAsync(uri, options, allowFollowingLinks);
        }

        /// <summary>
        /// Will display markdown content from the provided uri (local or from the web)
        /// Optionally after the markdown is displayed, a list of links from the document are presented and the user can select them to view more content
        /// The intent is to display information/documentation so it is treated as best effort,
        /// any problems like (missing file or problems downloading content) are displayed in line as opposed to exceptions that bubble out.
        /// 
        /// Selected links are handled in the following way
        ///  - If they yield markdown, that content is displayed
        ///  - If they yield an image and it looks like this being run in iTerm2 (https://iterm2.com/) it will be displayed inline
        ///  - For everything else it is thrown at the OS to see if it can sort it out.
        /// </summary>
        /// <param name="text">the content to display</param>
        /// <param name="baseUri">uri for that content, this base is used to calculate relative links.  If null, all links will be assumed to be relative to the current directory</param>
        /// <param name="options">options to control how to display the content</param>
        /// <param name="allowFollowingLinks">when set to true, the list of links will be provided, when false the list is omitted</param>
        public static async Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            using var displayer = new MarkdownDisplayer();
            await displayer.DisplayMarkdownAsync(text, baseUri, options, allowFollowingLinks);
        }
    }
}
