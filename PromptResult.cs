using System;

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// The kinds of results that can be returned from a selection prompt
    /// </summary>
    internal enum PromptResultKind
    {
        /// <summary>
        /// The user has indicated they are done and want to return control to the caller
        /// </summary>
        Done,

        /// <summary>
        /// The user wants to view the previously displayed content
        /// </summary>
        Back,

        /// <summary>
        /// The user has selected a link to follow
        /// </summary>
        Link,
    }

    /// <summary>
    /// Represents a selection made by the user from a <see cref="Spectre.Console.SelectionPrompt{T}"/>
    /// </summary>
    internal class PromptResult
    {
        /// <summary>
        /// Indicates what action the user selected
        /// </summary>
        public PromptResultKind Kind;

        /// <summary>
        /// For <see cref="PromptResultKind.Link"/> selections, the URI of the selected link; otherwise null
        /// </summary>
        public Uri? Uri;
    }
}
