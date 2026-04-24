using System;
using Spectre.Console;

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
        private PromptResult(PromptResultKind kind, LinkItem? linkItem)
        {
            Kind = kind;
            _linkItem = linkItem;
        }

        /// <summary>
        /// Creates a <see cref="PromptResultKind.Done"/> result
        /// </summary>
        public static PromptResult CreateDone() => new PromptResult(PromptResultKind.Done, null);

        /// <summary>
        /// Creates a <see cref="PromptResultKind.Back"/> result
        /// </summary>
        public static PromptResult CreateBack() => new PromptResult(PromptResultKind.Back, null);

        /// <summary>
        /// Creates a <see cref="PromptResultKind.Link"/> result for the given <paramref name="linkItem"/>
        /// </summary>
        public static PromptResult CreateLink(LinkItem linkItem) => new PromptResult(PromptResultKind.Link, linkItem);

        /// <summary>
        /// Indicates what action the user selected
        /// </summary>
        public PromptResultKind Kind { get; }

        private LinkItem? _linkItem;

        /// <summary>
        /// For <see cref="PromptResultKind.Link"/> selections, the link item that was selected.
        /// Throws <see cref="NullReferenceException"/> if accessed on a non-Link result.
        /// </summary>
        public LinkItem LinkItem => _linkItem ?? throw new NullReferenceException($"{nameof(LinkItem)} is not set for {nameof(PromptResultKind)}: {Kind}");

        /// <summary>
        /// Returns the display string for this result as shown in the selection prompt
        /// </summary>
        public string ToDisplayString() => Kind switch
        {
            PromptResultKind.Done => "Done",
            PromptResultKind.Back => "Back",
            PromptResultKind.Link => Markup.Escape(LinkItem.ToString()),
            _ => throw new InvalidOperationException($"Unexpected {nameof(PromptResultKind)}: {Kind}"),
        };
    }
}
