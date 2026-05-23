using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal class ConsoleRenderer : ConsoleRendererBase<ConsoleRenderer>
    {
        public ConsoleRenderer(SpectreDisplayOptions options) : this(options, omitAutolinkInlineRenderer: false) { }

        /// <summary>
        /// Intended for testing only. When <paramref name="omitAutolinkInlineRenderer"/> is <see langword="true"/>,
        /// <see cref="ConsoleAutolinkInlineRenderer"/> is excluded from the renderer list, allowing tests to
        /// exercise the unhandled-type code path without relying on shared mutable state.
        /// </summary>
        internal ConsoleRenderer(SpectreDisplayOptions options, bool omitAutolinkInlineRenderer) : base(options)
        {
            if (!omitAutolinkInlineRenderer)
            {
                ObjectRenderers.Add(new ConsoleAutolinkInlineRenderer());
            }

            ObjectRenderers.AddRange([
                new ConsoleAbbreviationInlineRenderer(),
                // ConsoleMathBlockRenderer must precede ConsoleCodeBlockRenderer because
                // Markdig's MathBlock extends FencedCodeBlock (which extends CodeBlock),
                // and renderer dispatch uses type assignability — so the math-specific
                // renderer has to win before the code-block renderer claims the type.
                new ConsoleMathBlockRenderer(),
                // ConsoleYamlFrontMatterBlockRenderer must also precede ConsoleCodeBlockRenderer
                // because Markdig's YamlFrontMatterBlock extends CodeBlock.
                new ConsoleYamlFrontMatterBlockRenderer(),
                new ConsoleCodeBlockRenderer(),
                new ConsoleCodeInlineRenderer(),
                new ConsoleCustomContainerInlineRenderer(),
                new ConsoleCustomContainerRenderer(),
                new ConsoleDefinitionItemRenderer(),
                new ConsoleDefinitionListRenderer(),
                new ConsoleDefinitionTermRenderer(),
                new ConsoleDocumentRenderer(),
                new ConsoleEmphasisInlineRenderer(),
                new ConsoleEmojiInlineRenderer(),
                new ConsoleFigureCaptionRenderer(),
                new ConsoleFigureRenderer(),
                new ConsoleFooterBlockRenderer(),
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
                new ConsoleMathInlineRenderer(),
                new ConsoleParagraphBlockRenderer(),
                new ConsoleQuoteBlockRenderer(),
                new ConsoleSmartyPantInlineRenderer(),
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
