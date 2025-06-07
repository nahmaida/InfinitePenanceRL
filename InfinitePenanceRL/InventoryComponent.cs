using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace InfinitePenanceRL
{
    public class InventoryComponent : UIComponent
    {
        public int SlotCount { get; set; } = 8;
        public int SlotSize { get; set; } = 40;
        public int Spacing { get; set; } = 5;
        public List<InventoryItem> Items { get; } = new List<InventoryItem>();
        private Point _mousePosition;

        public void UpdateMousePosition(Point mousePosition)
        {
            _mousePosition = mousePosition;
        }

        private int GetHoveredSlotIndex()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                float x = ScreenPosition.X + i * (SlotSize + Spacing);
                float y = ScreenPosition.Y;
                var slotBounds = new RectangleF(x, y, SlotSize, SlotSize);

                if (slotBounds.Contains(_mousePosition))
                {
                    return i;
                }
            }
            return -1;
        }

        public override void Draw(Graphics g)
        {
            if (!IsVisible) return;

            for (int i = 0; i < SlotCount; i++)
            {
                float x = ScreenPosition.X + i * (SlotSize + Spacing);
                float y = ScreenPosition.Y;

                // Фон для слотов
                g.FillRectangle(new SolidBrush(Color.DarkGray),
                    x, y, SlotSize, SlotSize);

                // Вещи в инвентаре
                if (i < Items.Count && Items[i] != null)
                {
                    // Фон
                    g.FillRectangle(new SolidBrush(Items[i].Color),
                        x + 2, y + 2, SlotSize - 4, SlotSize - 4);

                    // Количество
                    if (Items[i].Count > 1)
                    {
                        using (var font = new Font("Arial", 8))
                        {
                            g.DrawString(Items[i].Count.ToString(), font, Brushes.White,
                                x + SlotSize - 15, y + SlotSize - 15);
                        }
                    }
                }

                // Граница слота
                g.DrawRectangle(Pens.White, x, y, SlotSize, SlotSize);
            }

            // Draw tooltip for hovered item
            int hoveredSlot = GetHoveredSlotIndex();
            if (hoveredSlot >= 0 && hoveredSlot < Items.Count && Items[hoveredSlot] != null)
            {
                DrawTooltip(g, Items[hoveredSlot], _mousePosition);
            }
        }

        private void DrawTooltip(Graphics g, InventoryItem item, Point mousePos)
        {
            using (var font = new Font("Arial", 10))
            {
                // Измеряем размер текста
                var textSize = g.MeasureString(item.Name, font);
                
                // Отступы для подсказки
                int padding = 5;
                int tooltipWidth = (int)textSize.Width + padding * 2;
                int tooltipHeight = (int)textSize.Height + padding * 2;

                // Позиция подсказки (чуть выше курсора)
                int tooltipX = mousePos.X;
                int tooltipY = mousePos.Y - tooltipHeight - 10;

                // Фон подсказки
                g.FillRectangle(new SolidBrush(Color.Black),
                    tooltipX, tooltipY, tooltipWidth, tooltipHeight);

                // Граница подсказки
                g.DrawRectangle(Pens.White,
                    tooltipX, tooltipY, tooltipWidth, tooltipHeight);

                // Текст подсказки
                g.DrawString(item.Name, font, Brushes.White,
                    tooltipX + padding, tooltipY + padding);
            }
        }

        public bool AddItem(InventoryItem item)
        {
            if (Items.Count < SlotCount)
            {
                Items.Add(item);
                return true;
            }
            return false;
        }
    }

    public class InventoryItem
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public int Count { get; set; } = 1;
        public bool IsStackable { get; set; } = true;

        public InventoryItem(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
} 