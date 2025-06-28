using System.Drawing;

namespace InfinitePenanceRL
{
    // Камера следит за игроком и определяет, что видно на экране
    public class Camera
    {
        public Vector2 Position { get; private set; }  // Позиция камеры в игровом мире
        public Size ViewportSize { get; private set; } // Размер видимой области (окна)
        
        // Эффект тряски экрана
        private float _shakeIntensity = 0f;
        private float _shakeDuration = 0f;
        private float _shakeTimer = 0f;
        private Vector2 _shakeOffset = Vector2.Zero;

        public Camera(Size viewportSize)
        {
            ViewportSize = viewportSize;
            Position = Vector2.Zero;
        }

        // Запускаем тряску экрана
        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeTimer = duration;
        }

        // Обновляем тряску экрана
        public void UpdateShake(float deltaTime)
        {
            if (_shakeTimer > 0)
            {
                _shakeTimer -= deltaTime;
                
                // Вычисляем случайное смещение для тряски
                var random = new Random();
                float offsetX = (float)(random.NextDouble() - 0.5) * _shakeIntensity * (_shakeTimer / _shakeDuration);
                float offsetY = (float)(random.NextDouble() - 0.5) * _shakeIntensity * (_shakeTimer / _shakeDuration);
                _shakeOffset = new Vector2(offsetX, offsetY);
                
                if (_shakeTimer <= 0)
                {
                    _shakeOffset = Vector2.Zero;
                    _shakeIntensity = 0f;
                }
            }
        }

        // Переводит координаты из игрового мира в координаты экрана
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return new Vector2(
                worldPosition.X - Position.X + ViewportSize.Width / 2 + _shakeOffset.X,
                worldPosition.Y - Position.Y + ViewportSize.Height / 2 + _shakeOffset.Y
            );
        }

        // Переводит координаты с экрана в координаты игрового мира
        public Vector2 ScreenToWorld(Point screenPosition)
        {
            return new Vector2(
                screenPosition.X + Position.X - ViewportSize.Width / 2 - _shakeOffset.X,
                screenPosition.Y + Position.Y - ViewportSize.Height / 2 - _shakeOffset.Y
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