using System.Text;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;
using Spectre.Console.Rendering;

using MDTable = Markdig.Extensions.Tables.Table;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal abstract partial class ConsoleRendererBase : RendererBase
{
    protected ConsoleRendererBase(SpectreDisplayOptions options)
    {
        Options = options;
        if (Options.IncludeDebug)
        {
            ObjectWriteBefore += Before;
        }
    }

    public override object Render(MarkdownObject markdownObject)
    {
        var startingState = CaptureState();
        Write(markdownObject);
        AssertStateMatches(startingState, "at the end of rendering", includeInlineContent: true);
        return Root ?? new object();
    }

    public IRenderable? Root {get; private set;}

    public SpectreDisplayOptions Options {get; init;}

    public IReadOnlyList<LinkItem> Links => m_links;

    public IReadOnlySet<Type>? UnhandledTypes => m_unhandledTypes;
    public IReadOnlySet<UnknownEmphasisDelimiter>? UnknownEmphasisDelimiters => m_unknownEmphasisDelimiters;

    public void RecordUnknownEmphasisDelimiter(char delimiterChar, int delimiterCount)
    {
        m_unknownEmphasisDelimiters ??= new HashSet<UnknownEmphasisDelimiter>();
        m_unknownEmphasisDelimiters.Add(new UnknownEmphasisDelimiter(delimiterChar, delimiterCount));
    }

    protected void NewFrameImplementation(Style? borderStyle = default, TableBorder? border = default)
    {
        borderStyle ??= Options.IncludeDebug ? Style.Plain : (Style?)null;
        var frame = new Frame(borderStyle, border);
        frame.Table.AddColumn(string.Empty);
        if (Options.TableExpand && !m_frames.Any())
        {
            frame.Table.Expand = true;
        }
        PushFrame(frame);
    }

    private Table PushFrame(Frame frame)
    {
        if (!m_frames.Any())
        {
            Root = frame.Table;
        }
        m_frames.Push(frame);
        return frame.Table;
    }

    public void CompleteFrame(Func<Table, IRenderable>? transform = null)
    {
        var table = Pop(m_frames, "frame", nameof(CompleteFrame)).Table;
        if (m_frames.Any())
        {
            IRenderable renderable = transform?.Invoke(table) ?? table;
            Peek(m_frames, "frame", nameof(CompleteFrame)).AddRow(renderable);
        }
    }

    protected void NewTableFrameImplementation(MDTable table)
    {
        var frame = new TableFrame(table, Options);
        m_tables.Push(frame);
        PushFrame(frame);
    }

    public void CompleteTableFrame()
    {
        CompleteFrame();
        Pop(m_tables, "table", nameof(CompleteTableFrame));
    }

    public void CompleteTableRow() => Peek(m_tables, "table", nameof(CompleteTableRow)).CompletesRow();
    public void CompleteTableCell()
        => Peek(m_tables, "table", nameof(CompleteTableCell))
            .AddCell(Pop(m_frames, "frame", nameof(CompleteTableCell)).Table);

    protected void StartTableCellImplementation() => NewFrameImplementation();

    protected void NewListBlockFrameImplementation(ListBlock list)
    {
        var frame = new ListBlockFrame(this, list);
        m_lists.Push(frame);
        PushFrame(frame);
    }

    public void SetNextListItemCheck(bool isChecked)
        => Peek(m_lists, "list", nameof(SetNextListItemCheck)).NextItemChecked = isChecked;

    public void CompleteListBlockFrame()
    {
        CompleteFrame();
        Pop(m_lists, "list", nameof(CompleteListBlockFrame));
    }

    protected void PushLinkImplementation() => m_linkFrames.Push(new LinkFrame());

    protected void PopLinkImplementation(string url, bool isImage = false)
    {
        var old = Pop(m_linkFrames, "link frame", "PopLink");
        var content = old.Content;
        AddInLineImplementation(content);
        m_links.Add(new LinkItem(url: url, content: content, isImage: isImage));
    }

    protected void StartInlineImplementation()
    {
        if (m_inlineContent.Length != 0)
        {
            throw CreateBalanceException(
                "inline content buffer",
                "StartInline",
                expectedDepth: 0,
                actualDepth: m_inlineContent.Length);
        }

        m_inlineOwner = m_rendererContexts.TryPeek(out var current) ? current : null;
    }

    protected void StartHtmlInlineStyleImplementation()
    {
        AddInLineImplementation($"[{Options.HtmlInline.ToMarkup()}]");
        m_openHtmlInlineStyles++;
    }

    protected void EndHtmlInlineStyleImplementation()
    {
        if (m_openHtmlInlineStyles > 0)
        {
            AddInLineImplementation("[/]");
            m_openHtmlInlineStyles--;
        }
    }

    protected void AddInLineImplementation(string content)
    {
        if (LeftTrimNextContent)
        {
            content = content.TrimStart();
            LeftTrimNextContent = false;
        }
        if (m_linkFrames.Any())
        {
            Peek(m_linkFrames, "link frame", "AddInLine").Append(content);
        }
        else
        {
            m_inlineContent.Append(content);
        }
    }

    protected void EndInlineImplementation()
    {
        while (m_openHtmlInlineStyles > 0)
        {
            EndHtmlInlineStyleImplementation();
        }

        Peek(m_frames, "frame", "EndInline").AddRow(new Markup(m_inlineContent.ToString(), CurrentStyle));
        m_inlineContent.Clear();
        m_inlineOwner = null;
    }

    protected void AddThematicBreakImplementation()
        => Peek(m_frames, "frame", "AddThematicBreak").AddRow(new Rule { Style = Options.ThematicBreak });

    protected void AddRenderableImplementation(IRenderable renderable)
        => Peek(m_frames, "frame", "AddRenderable").AddRow(renderable);

    protected void AddFilledBlockImplementation(LeafBlock block, Style style, string indent, string? fence)
    {
        // Emit a block's raw source lines behind a full-width background fill: every rendered
        // line (including short lines, the blank padding rows, and any wrapped continuation
        // rows) is padded out to the block width so the style's background color forms a solid
        // rectangle instead of only sitting behind the text.
        var lines = new List<string>();
        if (fence is not null)
        {
            lines.Add(fence);
        }

        for (int i = 0; i < block.Lines.Count; i++)
        {
            ref var slice = ref block.Lines.Lines[i].Slice;
            if (!string.IsNullOrEmpty(slice.Text))
            {
                lines.Add(indent + slice.Text.Substring(slice.Start, slice.Length) + indent);
            }
        }

        if (fence is not null)
        {
            lines.Add(fence);
        }

        var body = Environment.NewLine + string.Join(Environment.NewLine, lines) + Environment.NewLine;

        Peek(m_frames, "frame", "AddFilledBlock").AddRow(new BackgroundFillRenderable(new Text(body, style), style));
    }

    protected void PushStyleImplementation(Style style) => m_styles.Push(style);
    protected void PopStyleImplementation() => Pop(m_styles, "style", "PopStyle");
    public Style? CurrentStyle => m_styles.TryPeek(out var style) ? style : (Style?)null;

    public bool LeftTrimNextContent;

    internal void WriteWithBalanceCheck(string rendererName, MarkdownObject obj, Action write)
    {
        var startingState = CaptureState();
        var context = new RendererContext(rendererName, obj.GetType().Name);
        m_rendererContexts.Push(context);
        try
        {
            write();
            AssertStateMatches(
                startingState,
                $"after {rendererName}.Write for Markdig node {context.NodeType}",
                includeInlineContent: false);
        }
        finally
        {
            m_rendererContexts.Pop();
        }
    }

    private void Before(IMarkdownRenderer renderer, MarkdownObject obj)
    {
        Type type = obj.GetType();
        m_seenTypes ??= new HashSet<Type>();
        if (m_seenTypes.Contains(type))
        {
            return;
        }

        m_seenTypes.Add(type);

        if (!ObjectRenderers.Cast<IConsoleObjectRenderer>().Any(x => x.SupportsType(this, type)))
        {
            m_unhandledTypes ??= new();
            m_unhandledTypes.Add(type);
        }
    }

    private StackState CaptureState()
        => new(
            m_frames.Count,
            m_styles.Count,
            m_tables.Count,
            m_lists.Count,
            m_linkFrames.Count,
            m_inlineContent.Length);

    private void AssertStateMatches(StackState expected, string operation, bool includeInlineContent)
    {
        AssertDepth("frame", expected.Frames, m_frames.Count, operation);
        AssertDepth("style", expected.Styles, m_styles.Count, operation);
        AssertDepth("table", expected.Tables, m_tables.Count, operation);
        AssertDepth("list", expected.Lists, m_lists.Count, operation);
        AssertDepth("link frame", expected.LinkFrames, m_linkFrames.Count, operation);
        if (includeInlineContent)
        {
            AssertDepth("inline content buffer", expected.InlineContentLength, m_inlineContent.Length, operation);
        }
    }

    private void AssertDepth(string stackName, int expectedDepth, int actualDepth, string operation)
    {
        if (expectedDepth != actualDepth)
        {
            throw CreateBalanceException(stackName, operation, expectedDepth, actualDepth);
        }
    }

    private T Pop<T>(Stack<T> stack, string stackName, string operation)
    {
        if (stack.TryPop(out var value))
        {
            return value;
        }

        throw CreateBalanceException(stackName, operation, expectedDepth: 1, actualDepth: 0);
    }

    private T Peek<T>(Stack<T> stack, string stackName, string operation)
    {
        if (stack.TryPeek(out var value))
        {
            return value;
        }

        throw CreateBalanceException(stackName, operation, expectedDepth: 1, actualDepth: 0);
    }

    private InvalidOperationException CreateBalanceException(
        string stackName,
        string operation,
        int expectedDepth,
        int actualDepth)
    {
        RendererContext? context = stackName == "inline content buffer" && m_inlineOwner is { } inlineOwner
            ? inlineOwner
            : m_rendererContexts.TryPeek(out var current) ? current : null;
        var contextMessage = context is { } value
            ? $" Renderer: {value.RendererName}; Markdig node: {value.NodeType}."
            : string.Empty;
        return new InvalidOperationException(
            $"Stack balance violation in {operation}: {stackName} expected depth {expectedDepth}, actual depth {actualDepth}.{contextMessage}");
    }

    private readonly record struct StackState(
        int Frames,
        int Styles,
        int Tables,
        int Lists,
        int LinkFrames,
        int InlineContentLength);

    private readonly record struct RendererContext(string RendererName, string NodeType);

    private readonly Stack<Frame> m_frames = new();
    private readonly Stack<Style> m_styles = new();
    private readonly Stack<TableFrame> m_tables = new();
    private readonly Stack<ListBlockFrame> m_lists = new();
    private readonly Stack<LinkFrame> m_linkFrames = new();
    private readonly Stack<RendererContext> m_rendererContexts = new();
    private readonly List<LinkItem> m_links = new();
    private readonly StringBuilder m_inlineContent = new();
    private RendererContext? m_inlineOwner;
    private int m_openHtmlInlineStyles;

    private HashSet<Type>? m_seenTypes;
    private HashSet<Type>? m_unhandledTypes;
    private HashSet<UnknownEmphasisDelimiter>? m_unknownEmphasisDelimiters;
}

[SpectreSourceFile]
internal abstract class ConsoleRendererBase<T> : ConsoleRendererBase where T : ConsoleRendererBase<T>
{
    protected ConsoleRendererBase(SpectreDisplayOptions options) : base(options) { }

    public T WriteEscape(ref StringSlice slice) 
        => WriteEscape(slice.Text?.Substring(slice.Start, slice.Length));


    public T WriteEscape(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            AddInLine(Markup.Escape(text));
        }
        return CastThis;
    }

    public T WriteLeafInline(LeafBlock leafBlock)
    {
        var inline = (Inline)leafBlock.Inline!;
      
        while (inline != default)
        {
            Write(inline);
            inline = inline.NextSibling;
        }

        return CastThis;
    }

    #region Method to aid in chaining calls

    public T NewFrame(Style? borderStyle = default, TableBorder? border = default)
    {
        NewFrameImplementation(borderStyle, border);
        return CastThis;
    }

    public T StartInline()
    {
        StartInlineImplementation();
        return CastThis;
    }

    public T StartHtmlInlineStyle()
    {
        StartHtmlInlineStyleImplementation();
        return CastThis;
    }

    public T EndHtmlInlineStyle()
    {
        EndHtmlInlineStyleImplementation();
        return CastThis;
    }

    public T AddInLine(string content)
    {
        AddInLineImplementation(content);
        return CastThis;
    }

    public T EndInline()
    {
        EndInlineImplementation();
        return CastThis;
    }

    public T PushLink()
    {
        PushLinkImplementation();
        return CastThis;
    }

    public T PopLink(string url, bool isImage = false)
    {
        PopLinkImplementation(url, isImage);
        return CastThis;
    }

    public T WriteLink(Action<T> writeDisplay, string url, bool isImage = false)
    {
        // When enabled (and the URL is non-empty), wrap the rendered link text with
        // Spectre.Console's [link=...]...[/] markup so that supported terminals emit
        // an OSC 8 hyperlink, making the link clickable inline.
        var useHyperlink = Options.UseTerminalHyperlinks && !string.IsNullOrEmpty(url);
        if (useHyperlink)
        {
            AddInLine($"[link={Markup.Escape(url)}]");
        }
        if (isImage)
        {
            AddInLine("!");
        }
        WriteEscape("[")
            .PushLink();
        writeDisplay(CastThis);
        PopLink(url, isImage)
            .WriteEscape("](")
            .WriteEscape(url)
            .AddInLine(")");
        if (useHyperlink)
        {
            AddInLine("[/]");
        }
        return CastThis;
    }

    public T NewListBlockFrame(ListBlock list)
    {
        NewListBlockFrameImplementation(list);
        return CastThis;
    }

    public T PushStyle(Style style)
    {
        PushStyleImplementation(style);
        return CastThis;
    }

    public T PopStyle()
    {
        PopStyleImplementation();
        return CastThis;
    }

    public T StartTableCell()
    {
        StartTableCellImplementation();
        return CastThis;
    } 

    public T NewTableFrame(MDTable table)
    {
        NewTableFrameImplementation(table);
        return CastThis;
    }

    public T WriteChildrenChain(ContainerBlock containerBlock)
    {
        WriteChildren(containerBlock);
        return CastThis;
    }

    public T WriteChildrenChain(ContainerInline containerInline)
    {
        WriteChildren(containerInline);
        return CastThis;
    }

    public T WithLeftTrimNextContent(bool trim)
    {
        LeftTrimNextContent = trim;
        return CastThis;
    }

    public T AddThematicBreak()
    {
        AddThematicBreakImplementation();
        return CastThis;
    }

    public T AddRenderable(IRenderable renderable)
    {
        AddRenderableImplementation(renderable);
        return CastThis;
    }

    public T AddFilledBlock(LeafBlock block, Style style, string indent = "", string? fence = null)
    {
        AddFilledBlockImplementation(block, style, indent, fence);
        return CastThis;
    }


    private T CastThis => (T)this;

    #endregion Method to aid in chaining calls
}