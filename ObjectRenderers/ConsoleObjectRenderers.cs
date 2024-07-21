using System;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;

using Table = Markdig.Extensions.Tables.Table;
using TableRow = Markdig.Extensions.Tables.TableRow;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public interface IConsoleObjectRenderer
    {
        bool SupportsType(RendererBase renderer, Type type);
    }

    /// <summary>
    /// The base class for all of our Object Renderers 
    /// </summary>
    /// <typeparam name="TObject">The type that renders can handle</typeparam>
    public abstract class ConsoleObjectRenderer<TObject> : MarkdownObjectRenderer<ConsoleRenderer, TObject>, IConsoleObjectRenderer 
        where TObject : MarkdownObject
    {
        public virtual bool SupportsType(RendererBase renderer, Type type) => Accept(renderer, type);
    }

    #region  Simple-One liner Renders

    public class ConsoleCodeInlineRenderer : ConsoleObjectRenderer<CodeInline>
    {
        protected override void Write(ConsoleRenderer renderer, CodeInline obj) 
            => renderer.AddInLine($"[{renderer.Options.CodeInLine.ToMarkup()}]")
                .AddInLine(obj.Content)
                .AddInLine("[/]");
    }

    public class ConsoleDocumentRenderer : ConsoleObjectRenderer<MarkdownDocument>
    {
        protected override void Write(ConsoleRenderer renderer, MarkdownDocument obj)
            => renderer
                .NewFrame()
                .WriteChildrenChain(obj)
                .CompleteFrame();
    }

    public class ConsoleHtmlBlockRenderer : ConsoleObjectRenderer<HtmlBlock>
    {
        protected override void Write(ConsoleRenderer renderer, HtmlBlock obj) 
            => renderer
                .NewFrame(Style.Plain)
                .StartInline()
                .AddInLine($"[{renderer.Options.HtmlBlock.ToMarkup()}]")
                .WriteLeafInline(obj)
                .AddInLine("[/]")
                .EndInline()
                .CompleteFrame();
    }

    public class ConsoleLineBreakInlineRenderer : ConsoleObjectRenderer<LineBreakInline>
    {
        protected override void Write(ConsoleRenderer renderer, LineBreakInline obj) 
            => renderer.AddInLine(Environment.NewLine);
    }

    public class ConsoleLinkReferenceDefinitionGroupRenderer : ConsoleObjectRenderer<LinkReferenceDefinitionGroup>
    {
        protected override void Write(ConsoleRenderer renderer, LinkReferenceDefinitionGroup obj) { }
    }

    public class ConsoleListBlockRenderer : ConsoleObjectRenderer<ListBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ListBlock obj) 
            => renderer
                .NewListBlockFrame(obj)
                .WriteChildrenChain(obj)
                .CompleteListBlockFrame();
    }

    public class ConsoleListItemBlockRenderer : ConsoleObjectRenderer<ListItemBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ListItemBlock obj) 
            => renderer
                .NewFrame()
                .WithLeftTrimNextContent(true)
                .WriteChildrenChain(obj)
                .CompleteFrame();
    }

    public class ConsoleLiteralInlineRenderer : ConsoleObjectRenderer<LiteralInline>
    {
        protected override void Write(ConsoleRenderer renderer, LiteralInline obj) 
            => renderer.WriteEscape(ref obj.Content);
    }

    public class ConsoleParagraphBlockRenderer : ConsoleObjectRenderer<ParagraphBlock>
    {
        protected override void Write(ConsoleRenderer renderer, ParagraphBlock obj) 
            => renderer
                .StartInline()
                .WriteLeafInline(obj)
                .EndInline();
    }

    public class ConsoleQuoteBlockRenderer : ConsoleObjectRenderer<QuoteBlock>
    {
        protected override void Write(ConsoleRenderer renderer, QuoteBlock obj)
            => renderer
                .NewFrame(borderStyle: Style.Plain)
                .PushStyle(renderer.Options.QuotedBlock)
                .StartInline()
                .WriteChildrenChain(obj)
                .EndInline()
                .PopStyle()
                .CompleteFrame();
    }

    public class ConsoleTableCellRenderer : ConsoleObjectRenderer<TableCell>
    {
        protected override void Write(ConsoleRenderer renderer, TableCell obj) 
            => renderer
                .StartTableCell()
                .WriteChildrenChain(obj)
                .CompleteTableCell();
    }

    public class ConsoleTableRenderer : ConsoleObjectRenderer<Table>
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

    public class ConsoleTableRowRenderer : ConsoleObjectRenderer<TableRow>
    {
        protected override void Write(ConsoleRenderer renderer, TableRow obj)
            => renderer
                .WriteChildrenChain(obj)
                .CompleteTableRow();
    }

    public class ConsoleTaskListRenderer : ConsoleObjectRenderer<TaskList>
    {
        protected override void Write(ConsoleRenderer renderer, TaskList obj) 
            => renderer.SetNextListItemCheck(obj.Checked);
    }

    #endregion  Simple-One liner Renders
}