using System.Drawing;

namespace InfinitePenanceRL
{
    // Базовый класс для всех элементов интерфейса (полоски здоровья, инвентарь и т.д.)
    public abstract class UIComponent : Component
    {
        public Vector2 ScreenPosition { get; set; }  // Позиция элемента на экране
        public bool IsVisible { get; set; } = true;  // Видимость элемента

        // Метод для отрисовки элемента интерфейса
        public abstract void Draw(Graphics g);
    }
} 