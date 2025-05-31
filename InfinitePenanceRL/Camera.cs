using System.Drawing;

namespace InfinitePenanceRL
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        public Size ViewportSize { get; set; }

        public void CenterOn(Vector2 target, Size worldBounds)
        {
            // Центрируем на персе
            float x = target.X - ViewportSize.Width / 2;
            float y = target.Y - ViewportSize.Height / 2;

            // Чтобы не выходила за границы мира
            x = Math.Max(0, Math.Min(x, worldBounds.Width - ViewportSize.Width));
            y = Math.Max(0, Math.Min(y, worldBounds.Height - ViewportSize.Height));

            Position = new Vector2(x, y);
        }

        public RectangleF GetViewport()
        {
            return new RectangleF(Position.X, Position.Y, ViewportSize.Width, ViewportSize.Height);
        }

        public PointF WorldToScreen(Vector2 worldPos)
        {
            return new PointF(worldPos.X - Position.X, worldPos.Y - Position.Y);
        }
    }
}