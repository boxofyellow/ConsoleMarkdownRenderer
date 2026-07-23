using Markdig.Extensions.Tables;
using Markdig.Renderers;
using Spectre.Console;
using Spectre.Console.Rendering;

using MDTable = Markdig.Extensions.Tables.Table;
using MDTableRow = Markdig.Extensions.Tables.TableRow;
using STable = Spectre.Console.Table;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

internal abstract partial class ConsoleRendererBase : RendererBase
{
    private class TableFrame : Frame
    {
        public TableFrame(MDTable mdTable, SpectreDisplayOptions options)
            : base(Style.Plain)
        {
            MDTable = mdTable;

            // GridTables (from UseGridTables()) can have no header row, in which case every
            // TableRow.IsHeader is false. Spectre.Console builds columns from the first AddRow call
            // and renders them as the header, so when the source has no header we must hide the
            // header row and add the first row as data instead of consuming it as column headers.
            m_hasHeader = MDTable.Cast<MDTableRow>().Any(x => x.IsHeader);
            if (m_hasHeader)
            {
                Table.ShowHeaders();
            }
            else
            {
                Table.HideHeaders();
            }
            Table.Border = options.TableBorder;
            Table.BorderStyle = options.TableBorderStyle;
            Table.Expand = options.TableExpand;

            int count = MDTable.Cast<MDTableRow>().Max(x => x.Count);
            m_columnData = new IRenderable[count];
            for (int i = 0; i < m_columnData.Length; i++)
            {
                m_columnData[i] = s_emptyContent;
            }
        }

        public void CompletesRow() => AddRow(s_emptyContent);

        public void AddCell(IRenderable data)
        {
            // The data is the cell's frame Table (created by NewFrameImplementation in
            // StartTableCellImplementation). To make the parent column's alignment
            // visible we expand the inner frame Table to fill the parent column and
            // apply the matching Justify to its single column — Spectre.Console only
            // honors TableColumn.Alignment on Markup-like cell content and ignores it
            // for nested Table renderables.
            if (data is STable cellTable
                && m_pos < MDTable.ColumnDefinitions.Count
                && cellTable.Columns.Count > 0)
            {
                cellTable.Expand = true;
                cellTable.Columns[0].Alignment = ToJustify(MDTable.ColumnDefinitions[m_pos].Alignment);
            }

            m_columnData[m_pos] = data;
            m_pos++;
        }

        public override void AddRow(IRenderable data)
        {
            if (!m_addedColumns)
            {
                for (int i = 0; i < m_columnData.Length; i++)
                {
                    var column = new TableColumn(m_hasHeader ? m_columnData[i] : s_emptyContent);
                    if (i < MDTable.ColumnDefinitions.Count)
                    {
                        column.Alignment = ToJustify(MDTable.ColumnDefinitions[i].Alignment);
                    }
                    Table.AddColumn(column);
                }
                m_addedColumns = true;

                // With no header row the first row's content was not consumed as column headers,
                // so it still needs to be rendered as a normal data row.
                if (!m_hasHeader)
                {
                    Table.AddRow(m_columnData);
                }
            }
            else
            {
                Table.AddRow(m_columnData);
            }
            for (int i = 0; i < m_columnData.Length; i++)
            {
                m_columnData[i] = s_emptyContent;
            }
            m_pos = 0;
        }

        private static Justify ToJustify(TableColumnAlign? alignment) => alignment switch
        {
            TableColumnAlign.Center => Justify.Center,
            TableColumnAlign.Right => Justify.Right,
            _ => Justify.Left,
        };

        private bool m_addedColumns;
        private readonly bool m_hasHeader;

        private readonly IRenderable[] m_columnData;
        private int m_pos;
        public readonly MDTable MDTable;
        private static readonly IRenderable s_emptyContent = new Markup(" ");
    }
}