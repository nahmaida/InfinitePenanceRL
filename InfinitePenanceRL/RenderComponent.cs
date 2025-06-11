using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;

namespace InfinitePenanceRL
{
    // Компонент для отрисовки (спрайты, цвета, масштаб и все такое)
    public class RenderComponent : Component
    {
        public Color Color { get; set; } = Color.White;
        public Size Size { get; set; } = new Size(32, 32);
        public string SpriteName { get; set; }
        public float Scale { get; set; } = 1.0f;
        public Rectangle? SpriteRegion { get; set; }
        public bool FlipHorizontal { get; set; } = false;
        private const int TILE_SIZE = 16;
        private Vector2 _lastLoggedPosition;
        private bool _hasLoggedInitial;

        private void Log(string message)
        {
            using (StreamWriter writer = File.AppendText("game.log"))
            {
                writer.WriteLine($"[Render] {message}");
            }
        }

        public void Draw(Graphics g)
        {
            if (Owner == null || g == null) 
            {
                if (!_hasLoggedInitial)
                {
                    Log("Draw called with null Owner or Graphics");
                    _hasLoggedInitial = true;
                }
                return;
            }

            var screenPos = Owner.Game.Camera.WorldToScreen(Owner.Position);

            // Only log position if it's changed significantly
            if (!_hasLoggedInitial || Vector2.Distance(_lastLoggedPosition, Owner.Position) > 50)
            {
                Log($"Drawing {SpriteName} at {screenPos.X}, {screenPos.Y} with scale {Scale}");
                _lastLoggedPosition = Owner.Position;
                _hasLoggedInitial = true;
            }

            if (!string.IsNullOrEmpty(SpriteName))
            {
                if (FlipHorizontal)
                {
                    // Сохраняем текущую трансформацию
                    var transform = g.Transform;
                    
                    // Настраиваем отражение по горизонтали
                    g.TranslateTransform(screenPos.X + Size.Width, screenPos.Y);
                    g.ScaleTransform(-1, 1);
                    
                    // Рисуем спрайт (в точке 0,0, т.к. мы уже сдвинули координаты)
                    Owner.Game.Sprites.DrawSprite(g, SpriteName, 0, 0, Scale, SpriteRegion);
                    
                    // Возвращаем исходную трансформацию
                    g.Transform = transform;
                }
                else
                {
                    Owner.Game.Sprites.DrawSprite(g, SpriteName, screenPos.X, screenPos.Y, Scale, SpriteRegion);
                }
            }
            else
            {
                // Если спрайта нет - рисуем цветной прямоугольник
                if (!_hasLoggedInitial)
                {
                    Log("Drawing fallback rectangle due to missing sprite");
                    _hasLoggedInitial = true;
                }
                using (var brush = new SolidBrush(Color))
                {
                    g.FillRectangle(brush, screenPos.X, screenPos.Y, Size.Width, Size.Height);
                }
            }
        }
    }
}