using System.Linq;
using Markdig.Renderers;
using Spectre.Console;
using Spectre.Console.Rendering;

using MDTable = Markdig.Extensions.Tables.Table;
using MDTableRow = Markdig.Extensions.Tables.TableRow;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public abstract partial class ConsoleRendererBase : RendererBase
    {
        private class TableFrame : Frame
        {
            public TableFrame(MDTable mdTable)
                : base(Style.Plain)
            {
                Table.ShowHeaders();
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
                m_columnData[m_pos] = data;
                m_pos++;
            }

            public override void AddRow(IRenderable data)
            {
                if (!m_addedColumns)
                {
                    for (int i = 0; i < m_columnData.Length; i++)
                    {
                        Table.AddColumn(new TableColumn(m_columnData[i]));
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

            private bool m_addedColumns;

            private readonly IRenderable[] m_columnData;
            private int m_pos;
            public readonly MDTable MDTable;
            private static readonly IRenderable s_emptyContent = new Markup(" ");
        }
    }
}