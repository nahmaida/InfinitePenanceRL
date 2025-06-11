using System.Drawing;

namespace InfinitePenanceRL
{
    // Камера следит за игроком и определяет, что видно на экране
    public class Camera
    {
        public Vector2 Position { get; private set; }  // Позиция камеры в игровом мире
        public Size ViewportSize { get; private set; } // Размер видимой области (окна)

        public Camera(Size viewportSize)
        {
            ViewportSize = viewportSize;
            Position = Vector2.Zero;
        }

        // Переводит координаты из игрового мира в координаты экрана
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return new Vector2(
                worldPosition.X - Position.X + ViewportSize.Width / 2,
                worldPosition.Y - Position.Y + ViewportSize.Height / 2
            );
        }

        // Переводит координаты с экрана в координаты игрового мира
        public Vector2 ScreenToWorld(Point screenPosition)
        {
            return new Vector2(
                screenPosition.X + Position.X - ViewportSize.Width / 2,
                screenPosition.Y + Position.Y - ViewportSize.Height / 2
            );
        }

        // Центрирует камеру на объекте (обычно на игроке)
        public void CenterOn(Vector2 target, Size worldBounds)
        {
            // Считаем, как далеко камера может двигаться в каждую сторону
            float maxX = worldBounds.Width - ViewportSize.Width / 2f;
            float maxY = worldBounds.Height - ViewportSize.Height / 2f;
            float minX = ViewportSize.Width / 2f;
            float minY = ViewportSize.Height / 2f;

            // Не даём камере выйти за пределы игрового мира
            float x = Math.Clamp(target.X, minX, maxX);
            float y = Math.Clamp(target.Y, minY, maxY);

            Position = new Vector2(x, y);
        }

        // Возвращает прямоугольник, описывающий видимую область
        public RectangleF GetViewport()
        {
            return new RectangleF(Position.X, Position.Y, ViewportSize.Width, ViewportSize.Height);
        }
    }
}