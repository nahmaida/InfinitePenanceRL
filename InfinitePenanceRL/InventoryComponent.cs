using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System;

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

        public bool IsMouseOverInventory(Point mousePosition)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                float x = ScreenPosition.X + i * (SlotSize + Spacing);
                float y = ScreenPosition.Y;
                var slotBounds = new RectangleF(x, y, SlotSize, SlotSize);

                if (slotBounds.Contains(mousePosition))
                {
                    return true;
                }
            }
            return false;
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
                    // определяем цвет фона (выбранный предмет имеет желтый фон)
                    Color backgroundColor = Items[i].IsSelected ? Color.Yellow : Items[i].Color;
                    
                    // Фон
                    g.FillRectangle(new SolidBrush(backgroundColor),
                        x + 2, y + 2, SlotSize - 4, SlotSize - 4);

                    // Спрайт предмета
                    if (!string.IsNullOrEmpty(Items[i].SpriteName))
                    {
                        Owner.Game.Sprites.DrawSprite(g, Items[i].SpriteName, 
                            x + 4, y + 4, (SlotSize - 8) / 16.0f);
                    }

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
                // получаем описание предмета
                string description = GetItemDescription(item);
                
                // измеряем размеры текста
                var nameSize = g.MeasureString(item.Name, font);
                var descSize = g.MeasureString(description, font);
                
                // размеры подсказки
                int padding = 5;
                int tooltipWidth = (int)Math.Max(nameSize.Width, descSize.Width) + padding * 2;
                int tooltipHeight = (int)(nameSize.Height + descSize.Height) + padding * 3; // +3 для отступа между строками

                // позиция подсказки (чуть выше курсора)
                int tooltipX = mousePos.X;
                int tooltipY = mousePos.Y - tooltipHeight - 10;

                // фон подсказки
                g.FillRectangle(new SolidBrush(Color.Black),
                    tooltipX, tooltipY, tooltipWidth, tooltipHeight);

                // граница подсказки
                g.DrawRectangle(Pens.White,
                    tooltipX, tooltipY, tooltipWidth, tooltipHeight);

                // название предмета
                g.DrawString(item.Name, font, Brushes.White,
                    tooltipX + padding, tooltipY + padding);
                
                // описание предмета
                g.DrawString(description, font, Brushes.LightGray,
                    tooltipX + padding, tooltipY + padding + (int)nameSize.Height + 3);
            }
        }
        
        private string GetItemDescription(InventoryItem item)
        {
            switch (item.Type)
            {
                case ItemType.Potion:
                    return "Восстанавливает 50 здоровья";
                case ItemType.Weapon:
                    return "Увеличивает урон на 5";
                default:
                    return "Неизвестный предмет";
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

        public void HandleClick(Point clickPosition)
        {
            int clickedSlot = GetHoveredSlotIndex();
            if (clickedSlot >= 0 && clickedSlot < Items.Count && Items[clickedSlot] != null)
            {
                var item = Items[clickedSlot];
                
                if (item.Selectable)
                {
                    // для выбираемых предметов - переключаем выбор
                    item.IsSelected = !item.IsSelected;
                    if (item.IsSelected)
                    {
                        item.Use(); // применяем эффект при выборе
                    }
                    else
                    {
                        // убираем эффект при отмене выбора
                        RemoveItemEffect(item);
                    }
                }
                else
                {
                    // для потребляемых предметов - используем
                    if (item.Use())
                    {
                        // если предмет использован успешно
                        if (item.Type == ItemType.Potion)
                        {
                            // зелье потребляется
                            item.Count--;
                            if (item.Count <= 0)
                            {
                                Items.RemoveAt(clickedSlot);
                            }
                        }
                    }
                }
            }
        }
        
        private void RemoveItemEffect(InventoryItem item)
        {
            switch (item.Type)
            {
                case ItemType.Weapon:
                    Player.WeaponDamageBonus = 0f; // убираем бонус урона
                    LogThrottler.Log($"Меч снят! Урон: {Player.GetTotalDamage()}", "item_use");
                    break;
            }
        }
    }

    public class InventoryItem
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public int Count { get; set; } = 1;
        public bool IsStackable { get; set; } = true;
        public string SpriteName { get; set; }
        public ItemType Type { get; set; } = ItemType.Consumable;
        public bool Selectable { get; set; } = false; // можно ли выбрать предмет
        public bool IsSelected { get; set; } = false; // выбран ли предмет

        public InventoryItem(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        // метод для использования предмета
        public bool Use()
        {
            switch (Type)
            {
                case ItemType.Potion:
                    if (Player.Health < Player.MaxHealth)
                    {
                        Player.Health = Math.Min(Player.MaxHealth, Player.Health + 50);
                        LogThrottler.Log($"Использовано зелье! Здоровье: {Player.Health}/{Player.MaxHealth}", "item_use");
                        return true; // предмет использован
                    }
                    else
                    {
                        LogThrottler.Log("Здоровье уже максимальное!", "item_use");
                        return false; // предмет не использован
                    }
                
                case ItemType.Weapon:
                    // меч дает +5 к урону
                    Player.WeaponDamageBonus = 5f;
                    LogThrottler.Log($"Экипирован меч! Урон: {Player.GetTotalDamage()} (базовый {Player.Damage} + меч {Player.WeaponDamageBonus})", "item_use");
                    return true;
                
                default:
                    return false;
            }
        }
    }

    public enum ItemType
    {
        Consumable,
        Potion,
        Weapon
    }
} 