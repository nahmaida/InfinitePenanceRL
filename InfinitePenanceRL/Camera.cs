using System.Drawing;

namespace InfinitePenanceRL
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        public Size ViewportSize { get; private set; }

        public Camera(Size viewportSize)
        {
            ViewportSize = viewportSize;
            Position = Vector2.Zero;
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return new Vector2(
                worldPosition.X - Position.X + ViewportSize.Width / 2,
                worldPosition.Y - Position.Y + ViewportSize.Height / 2
            );
        }

        public Vector2 ScreenToWorld(Point screenPosition)
        {
            return new Vector2(
                screenPosition.X + Position.X - ViewportSize.Width / 2,
                screenPosition.Y + Position.Y - ViewportSize.Height / 2
            );
        }

        public void CenterOn(Vector2 target, Size worldBounds)
        {
            // Вычисляем максимально допустимую позицию камеры
            float maxX = worldBounds.Width - ViewportSize.Width / 2f;
            float maxY = worldBounds.Height - ViewportSize.Height / 2f;
            float minX = ViewportSize.Width / 2f;
            float minY = ViewportSize.Height / 2f;

            // Ограничиваем позицию цели, чтобы камера оставалась в пределах мира
            float x = Math.Clamp(target.X, minX, maxX);
            float y = Math.Clamp(target.Y, minY, maxY);

            Position = new Vector2(x, y);
        }

        public RectangleF GetViewport()
        {
            return new RectangleF(Position.X, Position.Y, ViewportSize.Width, ViewportSize.Height);
        }
    }
}