namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal class ConsoleRenderer : ConsoleRendererBase<ConsoleRenderer>
    {
        public ConsoleRenderer(DisplayOptions options) : this(options, omitAutolinkInlineRenderer: false) { }

        /// <summary>
        /// Intended for testing only. When <paramref name="omitAutolinkInlineRenderer"/> is <see langword="true"/>,
        /// <see cref="ConsoleAutolinkInlineRenderer"/> is excluded from the renderer list, allowing tests to
        /// exercise the unhandled-type code path without relying on shared mutable state.
        /// </summary>
        internal ConsoleRenderer(DisplayOptions options, bool omitAutolinkInlineRenderer) : base(options)
        {
            if (!omitAutolinkInlineRenderer)
            {
                ObjectRenderers.Add(new ConsoleAutolinkInlineRenderer());
            }

            ObjectRenderers.AddRange([
                new ConsoleCodeBlockRenderer(),
                new ConsoleCodeInlineRenderer(),
                new ConsoleDocumentRenderer(),
                new ConsoleEmphasisInlineRenderer(),
                new ConsoleFootnoteGroupRenderer(),
                new ConsoleFootnoteLinkRenderer(),
                new ConsoleFootnoteRenderer(),
                new ConsoleHeadingBlockRenderer(),
                new ConsoleHtmlBlockRenderer(),
                new ConsoleHtmlEntityInlineRenderer(),
                new ConsoleHtmlInlineRenderer(),
                new ConsoleLineBreakInlineRenderer(),
                new ConsoleLinkInlineRenderer(),
                new ConsoleLinkReferenceDefinitionGroupRenderer(),
                new ConsoleListBlockRenderer(),
                new ConsoleListItemBlockRenderer(),
                new ConsoleLiteralInlineRenderer(),
                new ConsoleParagraphBlockRenderer(),
                new ConsoleQuoteBlockRenderer(),
                new ConsoleTableCellRenderer(),
                new ConsoleTableRenderer(),
                new ConsoleTableRowRenderer(),
                new ConsoleTaskListRenderer(),
                new ConsoleThematicBreakBlockRenderer(),
                // The order of this list is important
                // Each render will be asked if it wants handle the object to render
                // This is done by checking if the object in question is assignable to the type supported by this renderer
                // ConsoleContainerInlineRenders supports ContainerInline
                // It must be listed After all items that support things that are assignable to ContainerInline for example LinkInline
                new ConsoleContainerInlineRenderer(),
            ]);
        }
    }
}