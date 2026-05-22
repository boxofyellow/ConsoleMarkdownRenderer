using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.DefinitionLists;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Footers;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
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


    internal class ConsoleAbbreviationInlineRenderer : ConsoleObjectRenderer<AbbreviationInline>
    {
        protected override void Write(ConsoleRenderer renderer, AbbreviationInline obj)
            => renderer
                .WriteEscape(obj.Abbreviation.Label)
                .AddInLine($" ([{renderer.Options.AbbreviationTitle.ToSpectreStyle().ToMarkup()}]")
                .WriteEscape(obj.Abbreviation.Text.ToString())
                .AddInLine("[/])");
    }

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

    internal class ConsoleCustomContainerInlineRenderer : ConsoleObjectRenderer<CustomContainerInline>
    {
        protected override void Write(ConsoleRenderer renderer, CustomContainerInline obj)
            => renderer
                .AddInLine($"[{renderer.Options.CustomContainerInline.ToSpectreStyle().ToMarkup()}]")
                .WriteChildrenChain(obj)
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

    internal class ConsoleDefinitionItemRenderer : ConsoleObjectRenderer<DefinitionItem>
    {
        protected override void Write(ConsoleRenderer renderer, DefinitionItem obj)
            => renderer
                .NewFrame()
                .PushStyle(renderer.Options.DefinitionItem.ToSpectreStyle())
                .WriteChildrenChain(obj)
                .PopStyle()
                .CompleteFrame();
    }

    internal class ConsoleDefinitionListRenderer : ConsoleObjectRenderer<DefinitionList>
    {
        protected override void Write(ConsoleRenderer renderer, DefinitionList obj)
            => renderer
                .NewFrame(borderStyle: Style.Plain)
                .PushStyle(renderer.Options.DefinitionList.ToSpectreStyle())
                .WriteChildrenChain(obj)
                .PopStyle()
                .CompleteFrame();
    }

    internal class ConsoleDefinitionTermRenderer : ConsoleObjectRenderer<DefinitionTerm>
    {
        protected override void Write(ConsoleRenderer renderer, DefinitionTerm obj)
            => renderer
                .StartInline()
                .AddInLine($"[{renderer.Options.DefinitionTerm.ToSpectreStyle().ToMarkup()}]")
                .WriteLeafInline(obj)
                .AddInLine("[/]")
                .EndInline();
    }

    internal class ConsoleFigureRenderer : ConsoleObjectRenderer<Figure>
    {
        protected override void Write(ConsoleRenderer renderer, Figure obj)
            => renderer
                .NewFrame(borderStyle: Style.Plain)
                .WriteChildrenChain(obj)
                .CompleteFrame();
    }

    internal class ConsoleFigureCaptionRenderer : ConsoleObjectRenderer<FigureCaption>
    {
        protected override void Write(ConsoleRenderer renderer, FigureCaption obj)
            => renderer
                .StartInline()
                .AddInLine($"[{renderer.Options.FigureCaption.ToSpectreStyle().ToMarkup()}]")
                .WriteLeafInline(obj)
                .AddInLine("[/]")
                .EndInline();
    }

    internal class ConsoleFooterBlockRenderer : ConsoleObjectRenderer<FooterBlock>
    {
        protected override void Write(ConsoleRenderer renderer, FooterBlock obj)
            => renderer
                .NewFrame(borderStyle: Style.Plain)
                .AddThematicBreak()
                .PushStyle(renderer.Options.Footer.ToSpectreStyle())
                .WriteChildrenChain(obj)
                .PopStyle()
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

    internal class ConsoleMathInlineRenderer : ConsoleObjectRenderer<MathInline>
    {
        protected override void Write(ConsoleRenderer renderer, MathInline obj)
            => renderer
                .AddInLine($"[{renderer.Options.MathInline.ToSpectreStyle().ToMarkup()}]")
                .WriteEscape(ref obj.Content)
                .AddInLine("[/]");
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
            => renderer
                .NewFrame(borderStyle: Style.Plain)
                .PushStyle(renderer.Options.QuotedBlock.ToSpectreStyle())
                .StartInline()
                .WriteChildrenChain(obj)
                .EndInline()
                .PopStyle()
                .CompleteFrame();
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
