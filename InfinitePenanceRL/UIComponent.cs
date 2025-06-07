using System.Drawing;

namespace InfinitePenanceRL
{
    public abstract class UIComponent : Component
    {
        public Vector2 ScreenPosition { get; set; }
        public bool IsVisible { get; set; } = true;

        public abstract void Draw(Graphics g);
    }
} 