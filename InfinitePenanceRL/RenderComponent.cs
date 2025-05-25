using System.Drawing;

namespace InfinitePenanceRL
{
    // Рендер перса
    public class RenderComponent : Component
    {
        public Color Color { get; set; } = Color.Red;
        public Size Size { get; set; } = new Size(32, 32);

        public void Draw(Graphics g)
        {
            if (Owner == null || g == null) return;

            g.FillRectangle(new SolidBrush(Color),
                Owner.Position.X, Owner.Position.Y,
                Size.Width, Size.Height);
        }
    }
}