using System;
using Markdig.Renderers;
using Markdig.Syntax;
using RomanNumerals;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public abstract partial class ConsoleRendererBase : RendererBase
    {
        private class ListBlockFrame : Frame
        {
            public ListBlockFrame(ConsoleRendererBase render, ListBlock obj)
                : base()
            {
                m_listBock = obj;
                if (m_listBock.IsOrdered)
                {
                    m_orderType = m_listBock.BulletType switch
                    {
                        '1' => OrderType.Number,
                        'a' => OrderType.LowerLetter,
                        'A' => OrderType.UpperLetter,
                        'i' => OrderType.LowerRomanNumeral,
                        'I' => OrderType.UpperRomanNumeral,
                        _ => OrderType.Number,
                    };

                    m_count = Math.Max(0, int.Parse(obj.OrderedStart ?? obj.DefaultOrderedStart ?? "1"));
                }
                else
                {
                    m_orderType = OrderType.None;
                }

                Table
                    .AddColumn(string.Empty)  // One Column for the Bullets
                    .AddColumn(string.Empty); // One Column for the Data

                // Mark our bullet column Right Aligned
                Table.Columns[0]
                    .RightAligned();

                m_render = render;
            }

            public bool? NextItemChecked;

            public override void AddRow(IRenderable data)
            {
                string bulletText;
                if (NextItemChecked.HasValue)
                {
                    bulletText = Markup.Escape($"[{(NextItemChecked.Value ? 'X' : " ")}]"); 
                    NextItemChecked = default;
                }
                else if (m_listBock.IsOrdered)
                {
                    bulletText = m_orderType switch
                    {
                        OrderType.LowerLetter => Utilities.LetterBase(m_count, lower: true),
                        OrderType.UpperLetter => Utilities.LetterBase(m_count, lower: false),
                        OrderType.LowerRomanNumeral => new RomanNumeral(m_count).ToString().ToLower(),
                        OrderType.UpperRomanNumeral => new RomanNumeral(m_count).ToString(),
                        _ => m_count.ToString(),
                    } + ".";

                    m_count++;
                }
                else
                {
                    bulletText = m_listBock.BulletType.ToString();
                }
                Table.AddRow(new Markup(bulletText, m_render.CurrentStyle), data);
            }

            private int m_count;
            private readonly ListBlock m_listBock;
            private readonly OrderType m_orderType;
            private readonly ConsoleRendererBase m_render;

            private enum OrderType
            {
                None,
                Number,
                LowerLetter,
                UpperLetter,
                LowerRomanNumeral,
                UpperRomanNumeral
            }
        }
    }
}