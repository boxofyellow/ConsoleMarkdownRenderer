namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleRenderer : ConsoleRendererBase<ConsoleRenderer>
    {
        public ConsoleRenderer(DisplayOptions options) : base(options)
        {
            ObjectRenderers.AddRange([
                new ConsoleCodeBlockRenderer(),
                new ConsoleCodeInlineRenderer(),
                new ConsoleContainerInlineRenderer(),
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
            ]);
        }
    }
}