namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleRenderer : ConsoleRendererBase<ConsoleRenderer>
    {
        public ConsoleRenderer(DisplayOptions options) : base(options)
        {
            ObjectRenderers.AddRange([
                new ConsoleCodeBlockRenderer(),
                new ConsoleCodeInlineRenderer(),
                new ConsoleDocumentRenderer(),
                new ConsoleEmphasisInlineRenderer(),
                new ConsoleHeadingBlockRenderer(),
                new ConsoleHtmlBlockRenderer(),
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