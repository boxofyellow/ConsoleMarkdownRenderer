using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.TaskLists;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;

using Table = Markdig.Extensions.Tables.Table;
using TableCell = Markdig.Extensions.Tables.TableCell;
using TableRow = Markdig.Extensions.Tables.TableRow;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal interface IConsoleObjectRenderer
    {
        bool SupportsType(RendererBase renderer, Type type);
    }

    /// <summary>
    /// The base class for all of our Object Renderers 
    /// </summary>
    /// <typeparam name="TObject">The type that renders can handle</typeparam>
    internal abstract class ConsoleObjectRenderer<TObject> : MarkdownObjectRenderer<ConsoleRenderer, TObject>, IConsoleObjectRenderer 
        where TObject : MarkdownObject
    {
        public virtual bool SupportsType(RendererBase renderer, Type type) => Accept(renderer, type);
    }

    #region  Simple-One liner Renders


    // Note we conditionally included this one to help with tests.  In non-test scenarios it is always included.
    internal class ConsoleAutolinkInlineRenderer : ConsoleObjectRenderer<AutolinkInline>
    {
        protected override void Write(ConsoleRenderer renderer, AutolinkInline obj) 
            => renderer.WriteLink(r => r.WriteEscape(obj.Url), obj.IsEmail ? $"mailto:{obj.Url}" : obj.Url);
    }

    internal class ConsoleCodeInlineRenderer : ConsoleObjectRenderer<CodeInline>
    {
        protected override void Write(ConsoleRenderer renderer, CodeInline obj) 
            => renderer
                .AddInLine($"[{renderer.Options.CodeInLine.ToSpectreStyle().ToMarkup()}]")
                .WriteEscape(obj.Content)
                .AddInLine("[/]");
    }

    internal class ConsoleDocumentRenderer : ConsoleObjectRenderer<MarkdownDocument>
    {
        protected override void Write(ConsoleRenderer renderer, MarkdownDocument obj)
            => renderer
                .NewFrame()
                .WriteChildrenChain(obj)
                .CompleteFrame();
    }

    internal class ConsoleFootnoteRenderer : ConsoleObjectRenderer<Footnote>
    {
        protected override void Write(ConsoleRenderer renderer, Footnote obj)
            => renderer
                .NewFrame()
                .StartInline()
                .AddInLine($"[{renderer.Options.Footnote.ToSpectreStyle().ToMarkup()}]")
                .WriteEscape($"[{obj.Label}]:")
                .AddInLine("[/]")
                .EndInline()
                .WriteChildrenChain(obj)
                .CompleteFrame();
    }

    internal class ConsoleFootnoteGroupRenderer : ConsoleObjectRenderer<FootnoteGroup>
    {
        protected override void Write(ConsoleRenderer renderer, FootnoteGroup obj)
            => renderer
                .NewFrame(borderStyle: Style.Plain)
                .AddThematicBreak()
                .PushStyle(renderer.Options.FootnoteGroup.ToSpectreStyle())
                .WriteChildrenChain(obj)
                .PopStyle()
                .CompleteFrame();
    }

    internal class ConsoleFootnoteLinkRenderer : ConsoleObjectRenderer<FootnoteLink>
    {
        protected override void Write(ConsoleRenderer renderer, FootnoteLink obj)
        {
            var marker = obj.IsBackLink ? "↩" : $"^{obj.Index}";
            renderer
                .AddInLine($"[{renderer.Options.FootnoteLink.ToSpectreStyle().ToMarkup()}]")
                .WriteEscape($"[{marker}]")
                .AddInLine("[/]");
        }
    }

    internal class ConsoleHtmlBlockRenderer : ConsoleObjectRenderer<HtmlBlock>
    {
        protected override void Write(ConsoleRenderer renderer, HtmlBlock obj) 
            => renderer
                .NewFrame(Style.Plain)
                .StartInline()
                .AddInLine($"[{renderer.Options.HtmlBlock.ToSpectreStyle().ToMarkup()}]")
                .WriteLeafInline(obj)
                .AddInLine("[/]")
                .EndInline()
                .CompleteFrame();
    }

    internal class ConsoleLineBreakInlineRenderer : ConsoleObjectRenderer<LineBreakInline>
    {
        protected override void Write(ConsoleRenderer renderer, LineBreakInline obj) 
            => renderer.AddInLine(Environment.NewLine);
    }

    internal class ConsoleLinkInlineRenderer : ConsoleObjectRenderer<LinkInline>
    {
        protected override void Write(ConsoleRenderer renderer, LinkInline obj)
            => renderer.WriteLink(r => r.WriteChildrenChain(obj), obj.Url ?? string.Empty, obj.IsImage);
    }

    internal class ConsoleLinkReferenceDefinitionGroupRenderer : ConsoleObjectRenderer<LinkReferenceDefinitionGroup>
    {
        protected override void Write(ConsoleRenderer renderer, LinkReferenceDefinitionGroup obj) { }
    }

    internal class ConsoleListBlockRenderer : ConsoleObjectRenderer<ListBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ListBlock obj) 
            => renderer
                .NewListBlockFrame(obj)
                .WriteChildrenChain(obj)
                .CompleteListBlockFrame();
    }

    internal class ConsoleListItemBlockRenderer : ConsoleObjectRenderer<ListItemBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ListItemBlock obj) 
            => renderer
                .NewFrame()
                .WithLeftTrimNextContent(true)
                .WriteChildrenChain(obj)
                .CompleteFrame();
    }

    internal class ConsoleLiteralInlineRenderer : ConsoleObjectRenderer<LiteralInline>
    {
        protected override void Write(ConsoleRenderer renderer, LiteralInline obj) 
            => renderer.WriteEscape(ref obj.Content);
    }

    internal class ConsoleParagraphBlockRenderer : ConsoleObjectRenderer<ParagraphBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ParagraphBlock obj) 
            => renderer
                .StartInline()
                .WriteLeafInline(obj)
                .EndInline();
    }

    internal class ConsoleQuoteBlockRenderer : ConsoleObjectRenderer<QuoteBlock>
    {
        protected override void Write(ConsoleRenderer renderer, QuoteBlock obj)
        {
            if (renderer.Options.UseBorderForQuotedBlock)
            {
                // Render the children into an unbordered inner frame and then wrap that
                // frame in a Spectre.Console Panel so the blockquote is visually delineated
                // by a border from the surrounding text.
                renderer
                    .NewFrame()
                    .PushStyle(renderer.Options.QuotedBlock.ToSpectreStyle())
                    .StartInline()
                    .WriteChildrenChain(obj)
                    .EndInline()
                    .PopStyle()
                    .CompleteFrameAsPanel();
            }
            else
            {
                renderer
                    .NewFrame(borderStyle: Style.Plain)
                    .PushStyle(renderer.Options.QuotedBlock.ToSpectreStyle())
                    .StartInline()
                    .WriteChildrenChain(obj)
                    .EndInline()
                    .PopStyle()
                    .CompleteFrame();
            }
        }
    }

    internal class ConsoleTableCellRenderer : ConsoleObjectRenderer<TableCell>
    {
        protected override void Write(ConsoleRenderer renderer, TableCell obj) 
            => renderer
                .StartTableCell()
                .WriteChildrenChain(obj)
                .CompleteTableCell();
    }

    internal class ConsoleTableRenderer : ConsoleObjectRenderer<Table>
    {
        protected override void Write(ConsoleRenderer renderer, Table obj)
        {
            // TODO we should check obj.IsValid()
            renderer
                .NewTableFrame(obj)
                .WriteChildrenChain(obj)
                .CompleteTableFrame();
        }
    }

    internal class ConsoleTableRowRenderer : ConsoleObjectRenderer<TableRow>
    {
        protected override void Write(ConsoleRenderer renderer, TableRow obj)
            => renderer
                .WriteChildrenChain(obj)
                .CompleteTableRow();
    }

    internal class ConsoleTaskListRenderer : ConsoleObjectRenderer<TaskList>
    {
        protected override void Write(ConsoleRenderer renderer, TaskList obj) 
            => renderer.SetNextListItemCheck(obj.Checked);
    }

    internal class ConsoleThematicBreakBlockRenderer : ConsoleObjectRenderer<ThematicBreakBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ThematicBreakBlock obj)
            => renderer.AddThematicBreak();
    }

    #endregion  Simple-One liner Renders
}