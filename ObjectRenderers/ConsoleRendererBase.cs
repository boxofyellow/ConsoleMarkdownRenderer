using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;
using Spectre.Console.Rendering;

using MDTable = Markdig.Extensions.Tables.Table;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public abstract partial class ConsoleRendererBase : RendererBase
    {
        protected ConsoleRendererBase(bool includeDebug)
        {
            if (includeDebug)
            {
                ObjectWriteBefore += Before;
            }
            m_includeDebug = includeDebug;
        }

        public override object Render(MarkdownObject markdownObject)
        {
            Write(markdownObject);
            return Root ?? new object();
        }

        public IRenderable? Root {get; private set;}

        public IReadOnlyList<LinkItem> Links => m_links;

        public IReadOnlySet<Type>? UnhandledTypes => m_unhandledTypes;

        protected void NewFrameImplementation(Style? borderStyle = default)
        {
            borderStyle ??= m_includeDebug ? Style.Plain : default;
            var frame = new Frame(borderStyle);
            frame.Table.AddColumn(string.Empty);
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

        public Table CompleteFrame() 
        {
            var result = m_frames.Pop().Table;
            if (m_frames.Any())
            {
                m_frames.Peek().AddRow(result);
            }
            return result;
        }

        protected void NewTableFrameImplementation(MDTable table)
        {
            var frame = new TableFrame(table);
            m_tables.Push(frame);
            PushFrame(frame);
        }

        public Table CompleteTableFrame()
        {
            var result = CompleteFrame();
            m_tables.Pop();
            return result;
        }

        public void CompleteTableRow() => m_tables.Peek().CompletesRow();

        public void CompleteTableCell() => m_tables.Peek().AddCell(m_frames.Pop().Table);

        protected void StartTableCellImplementation() => NewFrameImplementation();

        protected void NewListBlockFrameImplementation(ListBlock list)
        {
            var frame = new ListBlockFrame(this, list);
            m_lists.Push(frame);
            PushFrame(frame);
        }

        public void SetNextListItemCheck(bool isChecked) => m_lists.Peek().NextItemChecked = isChecked;

        public Table CompleteListBlockFrame()
        {
            var result = CompleteFrame();
            m_lists.Pop();
            return result;
        }

        protected void PushLinkImplementation() => m_linkFrames.Push(new LinkFrame());

        protected void PopLinkImplementation(LinkInline link)
        {
            var old = m_linkFrames.Pop();
            var content = old.Content;
            AddInLineImplementation(content);
            m_links.Add(new LinkItem(link, content));
        }

        protected void StartInlineImplementation() => m_inlineContent.Clear();  // TODO : We should have a check in here that is already cleared

        protected void AddInLineImplementation(string content)
        {
            if (LeftTrimNextContent)
            {
                content = content.TrimStart();
                LeftTrimNextContent = false;
            }
            if (m_linkFrames.Any())
            {
                m_linkFrames.Peek().Append(content);
            }
            else
            {
                m_inlineContent.Append(content);
            }
        }

        protected void EndInlineImplementation()
        {
            m_frames.Peek().AddRow(new Markup(m_inlineContent.ToString(), m_styles.FirstOrDefault()));
            m_inlineContent.Clear();
        }

        protected void PushStyleImplementation(Style style) => m_styles.Push(style);
        protected void PopStyleImplementation() => m_styles.Pop();
        public Style? CurrentStyle => m_styles.FirstOrDefault();

        public void Clear()
        {
            m_frames.Clear();
            m_styles.Clear();
            m_tables.Clear();
            m_links.Clear();
            m_linkFrames.Clear();
            m_links.Clear();
            m_inlineContent.Clear();
            Root = default;
            LeftTrimNextContent = false;
            m_seenTypes = default;
            m_unhandledTypes = default;
        }

        public bool LeftTrimNextContent;

        private void Before(IMarkdownRenderer renderer, MarkdownObject obj)
        {
            m_seenTypes ??= new HashSet<Type>();
            if (m_seenTypes.Contains(obj.GetType()))
            {
                return;
            }
            m_seenTypes.Add(obj.GetType());

            if (!ObjectRenderers.Any(x => x.Accept(this, obj)))
            {
                m_unhandledTypes ??= new();
                m_unhandledTypes.Add(obj.GetType());
            }
        }

        private readonly Stack<Frame> m_frames = new();
        private readonly Stack<Style> m_styles = new();
        private readonly Stack<TableFrame> m_tables = new();
        private readonly Stack<ListBlockFrame> m_lists = new();
        private readonly Stack<LinkFrame> m_linkFrames = new();
        private readonly List<LinkItem> m_links = new();
        private readonly StringBuilder m_inlineContent = new();
        private readonly bool m_includeDebug;

        private HashSet<Type>? m_seenTypes;
        private HashSet<Type>? m_unhandledTypes;
    }

    public abstract class ConsoleRendererBase<T> : ConsoleRendererBase where T : ConsoleRendererBase<T>
    {
        protected ConsoleRendererBase(bool includeDebug) : base(includeDebug) { }

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

            if (leafBlock.Lines.Lines != default)
            {
                for (int i = 0; i < leafBlock.Lines.Lines.Length; i++)
                {
                    var slice = leafBlock.Lines.Lines[i].Slice;
                    var text = slice.Text?.Substring(slice.Start, slice.Length);
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (i > 0)
                        {
                            WriteEscape(Environment.NewLine);
                        }
                        WriteEscape(text);
                    }
                }
            }
            
            return CastThis;
        }

        #region Method to aid in chaining calls

        public T NewFrame(Style? borderStyle = default)
        {
            NewFrameImplementation(borderStyle);
            return CastThis;
        }

        public T StartInline()
        {
            StartInlineImplementation();
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
        public T PopLink(LinkInline link)
        {
            PopLinkImplementation(link);
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

        private T CastThis => (T)this;

        #endregion Method to aid in chaining calls
    }
}