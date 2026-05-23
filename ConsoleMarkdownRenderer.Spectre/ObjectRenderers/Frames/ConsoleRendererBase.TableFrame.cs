using Markdig.Renderers;
using Spectre.Console;
using Spectre.Console.Rendering;

using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using Markdig.Extensions.Tables;

using MDTable = Markdig.Extensions.Tables.Table;
using MDTableRow = Markdig.Extensions.Tables.TableRow;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal abstract partial class ConsoleRendererBase : RendererBase
    {
        private class TableFrame : Frame
        {
            public TableFrame(MDTable mdTable, SpectreDisplayOptions options)
                : base(Style.Plain)
            {
                Table.ShowHeaders();
                Table.Border = options.TableBorder;
                Table.BorderStyle = options.TableBorderStyle;
                MDTable = mdTable;

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
                if (data is Spectre.Console.Table cellTable
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
                        var column = new TableColumn(m_columnData[i]);
                        if (i < MDTable.ColumnDefinitions.Count)
                        {
                            column.Alignment = ToJustify(MDTable.ColumnDefinitions[i].Alignment);
                        }
                        Table.AddColumn(column);
                    }
                    m_addedColumns = true;
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

            private readonly IRenderable[] m_columnData;
            private int m_pos;
            public readonly MDTable MDTable;
            private static readonly IRenderable s_emptyContent = new Markup(" ");
        }
    }
}
