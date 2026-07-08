using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
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

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal interface IConsoleObjectRenderer
{
    bool SupportsType(RendererBase renderer, Type type);
}

/// <summary>
/// The base class for all of our Object Renderers 
/// </summary>
/// <typeparam name="TObject">The type that renders can handle</typeparam>
[SpectreSourceFile]
internal abstract class ConsoleObjectRendererBase<TObject> : MarkdownObjectRenderer<ConsoleRenderer, TObject>, IConsoleObjectRenderer 
    where TObject : MarkdownObject
{
    public virtual bool SupportsType(RendererBase renderer, Type type) => Accept(renderer, type);
}

#region  Simple-One liner Renders

[SpectreSourceFile]
internal class ConsoleAbbreviationInlineRenderer : ConsoleObjectRendererBase<AbbreviationInline>
{
    protected override void Write(ConsoleRenderer renderer, AbbreviationInline obj)
        => renderer
            .WriteEscape(obj.Abbreviation.Label)
            .AddInLine($" ([{renderer.Options.AbbreviationTitle.ToMarkup()}]")
            .WriteEscape(obj.Abbreviation.Text.ToString())
            .AddInLine("[/])");
}

// Note we conditionally included this one to help with tests.  In non-test scenarios it is always included.
[SpectreSourceFile]
internal class ConsoleAutolinkInlineRenderer : ConsoleObjectRendererBase<AutolinkInline>
{
    protected override void Write(ConsoleRenderer renderer, AutolinkInline obj) 
        => renderer.WriteLink(r => r.WriteEscape(obj.Url), obj.IsEmail ? $"mailto:{obj.Url}" : obj.Url);
}

[SpectreSourceFile]
internal class ConsoleCodeInlineRenderer : ConsoleObjectRendererBase<CodeInline>
{
    protected override void Write(ConsoleRenderer renderer, CodeInline obj) 
        => renderer
            .AddInLine($"[{renderer.Options.CodeInLine.ToMarkup()}]")
            .WriteEscape(obj.Content)
            .AddInLine("[/]");
}

[SpectreSourceFile]
internal class ConsoleCustomContainerInlineRenderer : ConsoleObjectRendererBase<CustomContainerInline>
{
    protected override void Write(ConsoleRenderer renderer, CustomContainerInline obj)
        => renderer
            .AddInLine($"[{renderer.Options.CustomContainerInline.ToMarkup()}]")
            .WriteChildrenChain(obj)
            .AddInLine("[/]");
}

[SpectreSourceFile]
internal class ConsoleDocumentRenderer : ConsoleObjectRendererBase<MarkdownDocument>
{
    protected override void Write(ConsoleRenderer renderer, MarkdownDocument obj)
        => renderer
            .NewFrame()
            .WriteChildrenChain(obj)
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleDefinitionItemRenderer : ConsoleObjectRendererBase<DefinitionItem>
{
    protected override void Write(ConsoleRenderer renderer, DefinitionItem obj)
        => renderer
            .NewFrame()
            .PushStyle(renderer.Options.DefinitionItem)
            .WriteChildrenChain(obj)
            .PopStyle()
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleDefinitionListRenderer : ConsoleObjectRendererBase<DefinitionList>
{
    protected override void Write(ConsoleRenderer renderer, DefinitionList obj)
        => renderer
            .NewFrame(borderStyle: Style.Plain)
            .PushStyle(renderer.Options.DefinitionList)
            .WriteChildrenChain(obj)
            .PopStyle()
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleDefinitionTermRenderer : ConsoleObjectRendererBase<DefinitionTerm>
{
    protected override void Write(ConsoleRenderer renderer, DefinitionTerm obj)
        => renderer
            .StartInline()
            .AddInLine($"[{renderer.Options.DefinitionTerm.ToMarkup()}]")
            .WriteLeafInline(obj)
            .AddInLine("[/]")
            .EndInline();
}

[SpectreSourceFile]
internal class ConsoleFigureRenderer : ConsoleObjectRendererBase<Figure>
{
    protected override void Write(ConsoleRenderer renderer, Figure obj)
        => renderer
            .NewFrame(borderStyle: Style.Plain)
            .WriteChildrenChain(obj)
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleFigureCaptionRenderer : ConsoleObjectRendererBase<FigureCaption>
{
    protected override void Write(ConsoleRenderer renderer, FigureCaption obj)
        => renderer
            .StartInline()
            .AddInLine($"[{renderer.Options.FigureCaption.ToMarkup()}]")
            .WriteLeafInline(obj)
            .AddInLine("[/]")
            .EndInline();
}

[SpectreSourceFile]
internal class ConsoleFooterBlockRenderer : ConsoleObjectRendererBase<FooterBlock>
{
    protected override void Write(ConsoleRenderer renderer, FooterBlock obj)
        => renderer
            .NewFrame(borderStyle: Style.Plain)
            .AddThematicBreak()
            .PushStyle(renderer.Options.Footer)
            .WriteChildrenChain(obj)
            .PopStyle()
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleFootnoteRenderer : ConsoleObjectRendererBase<Footnote>
{
    protected override void Write(ConsoleRenderer renderer, Footnote obj)
        => renderer
            .NewFrame()
            .StartInline()
            .AddInLine($"[{renderer.Options.Footnote.ToMarkup()}]")
            .WriteEscape($"[{obj.Label}]:")
            .AddInLine("[/]")
            .EndInline()
            .WriteChildrenChain(obj)
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleFootnoteGroupRenderer : ConsoleObjectRendererBase<FootnoteGroup>
{
    protected override void Write(ConsoleRenderer renderer, FootnoteGroup obj)
        => renderer
            .NewFrame(borderStyle: Style.Plain)
            .AddThematicBreak()
            .PushStyle(renderer.Options.FootnoteGroup)
            .WriteChildrenChain(obj)
            .PopStyle()
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleFootnoteLinkRenderer : ConsoleObjectRendererBase<FootnoteLink>
{
    protected override void Write(ConsoleRenderer renderer, FootnoteLink obj)
    {
        var marker = obj.IsBackLink ? "↩" : $"^{obj.Index}";
        renderer
            .AddInLine($"[{renderer.Options.FootnoteLink.ToMarkup()}]")
            .WriteEscape($"[{marker}]")
            .AddInLine("[/]");
    }
}

[SpectreSourceFile]
internal class ConsoleHtmlBlockRenderer : ConsoleObjectRendererBase<HtmlBlock>
{
    protected override void Write(ConsoleRenderer renderer, HtmlBlock obj) 
        => renderer
            .NewFrame(Style.Plain)
            .AddFilledBlock(obj, renderer.Options.HtmlBlock)
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleLineBreakInlineRenderer : ConsoleObjectRendererBase<LineBreakInline>
{
    protected override void Write(ConsoleRenderer renderer, LineBreakInline obj) 
        => renderer.AddInLine(obj.IsHard ? Environment.NewLine : " ");
}

[SpectreSourceFile]
internal class ConsoleLinkInlineRenderer : ConsoleObjectRendererBase<LinkInline>
{
    protected override void Write(ConsoleRenderer renderer, LinkInline obj)
        => renderer.WriteLink(r => r.WriteChildrenChain(obj), obj.Url ?? string.Empty, obj.IsImage);
}

[SpectreSourceFile]
internal class ConsoleLinkReferenceDefinitionGroupRenderer : ConsoleObjectRendererBase<LinkReferenceDefinitionGroup>
{
    protected override void Write(ConsoleRenderer renderer, LinkReferenceDefinitionGroup obj) { }
}

[SpectreSourceFile]
internal class ConsoleListBlockRenderer : ConsoleObjectRendererBase<ListBlock>
{
    protected override void Write(ConsoleRenderer renderer, ListBlock obj) 
        => renderer
            .NewListBlockFrame(obj)
            .WriteChildrenChain(obj)
            .CompleteListBlockFrame();
}

[SpectreSourceFile]
internal class ConsoleListItemBlockRenderer : ConsoleObjectRendererBase<ListItemBlock>
{
    protected override void Write(ConsoleRenderer renderer, ListItemBlock obj) 
        => renderer
            .NewFrame()
            .WithLeftTrimNextContent(true)
            .WriteChildrenChain(obj)
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleLiteralInlineRenderer : ConsoleObjectRendererBase<LiteralInline>
{
    protected override void Write(ConsoleRenderer renderer, LiteralInline obj) 
        => renderer.WriteEscape(ref obj.Content);
}

[SpectreSourceFile]
internal class ConsoleMathInlineRenderer : ConsoleObjectRendererBase<MathInline>
{
    protected override void Write(ConsoleRenderer renderer, MathInline obj)
        => renderer
            .AddInLine($"[{renderer.Options.MathInline.ToMarkup()}]")
            .WriteEscape(ref obj.Content)
            .AddInLine("[/]");
}

[SpectreSourceFile]
internal class ConsoleParagraphBlockRenderer : ConsoleObjectRendererBase<ParagraphBlock>
{
    protected override void Write(ConsoleRenderer renderer, ParagraphBlock obj) 
        => renderer
            .StartInline()
            .WriteLeafInline(obj)
            .EndInline();
}

[SpectreSourceFile]
internal class ConsoleQuoteBlockRenderer : ConsoleObjectRendererBase<QuoteBlock>
{
    protected override void Write(ConsoleRenderer renderer, QuoteBlock obj)
        => renderer
            .NewFrame(borderStyle: Style.Plain, border: QuoteBlockTableBorder.QuoteBlock)
            .PushStyle(renderer.Options.QuotedBlock)
            .StartInline()
            .WriteChildrenChain(obj)
            .EndInline()
            .PopStyle()
            .CompleteFrame();
}

[SpectreSourceFile]
internal class ConsoleTableCellRenderer : ConsoleObjectRendererBase<TableCell>
{
    protected override void Write(ConsoleRenderer renderer, TableCell obj) 
        => renderer
            .StartTableCell()
            .WriteChildrenChain(obj)
            .CompleteTableCell();
}

[SpectreSourceFile]
internal class ConsoleTableRenderer : ConsoleObjectRendererBase<Table>
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

[SpectreSourceFile]
internal class ConsoleTableRowRenderer : ConsoleObjectRendererBase<TableRow>
{
    protected override void Write(ConsoleRenderer renderer, TableRow obj)
        => renderer
            .WriteChildrenChain(obj)
            .CompleteTableRow();
}

[SpectreSourceFile]
internal class ConsoleTaskListRenderer : ConsoleObjectRendererBase<TaskList>
{
    protected override void Write(ConsoleRenderer renderer, TaskList obj) 
        => renderer.SetNextListItemCheck(obj.Checked);
}

[SpectreSourceFile]
internal class ConsoleThematicBreakBlockRenderer : ConsoleObjectRendererBase<ThematicBreakBlock>
{
    protected override void Write(ConsoleRenderer renderer, ThematicBreakBlock obj)
        => renderer.AddThematicBreak();
}

#endregion  Simple-One liner Renders
