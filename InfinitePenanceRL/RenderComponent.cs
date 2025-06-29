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
        public float Rotation { get; set; } = 0.0f; // Поворот в радианах
        private const int TILE_SIZE = 16;
        private Vector2 _lastLoggedPosition;
        private bool _hasLoggedInitial;
        
        // Анимация
        public bool IsAnimated { get; set; } = false;
        public int CurrentFrame { get; set; } = 0;
        public int FrameCount { get; set; } = 1;
        public int FrameWidth { get; set; } = 16;
        public int FrameHeight { get; set; } = 16;

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

            // пока не логируем
            // if (!_hasLoggedInitial || Vector2.Distance(_lastLoggedPosition, Owner.Position) > 100)
            // {
            //     Log($"Drawing {SpriteName} at {screenPos.X}, {screenPos.Y} with scale {Scale}");
            //     _lastLoggedPosition = Owner.Position;
            //     _hasLoggedInitial = true;
            // }

            // проверяем, нужно ли рисовать эффект наведения для двери
            var doorComponent = Owner.GetComponent<DoorComponent>();
            bool isDoorHovered = doorComponent?.IsHovered ?? false;

            if (!string.IsNullOrEmpty(SpriteName))
            {
                if (Rotation != 0.0f)
                {
                    // Сохраняем текущую трансформацию
                    var transform = g.Transform;
                    
                    // Настраиваем поворот вокруг центра спрайта
                    float centerX = screenPos.X + (Size.Width * Scale) / 2;
                    float centerY = screenPos.Y + (Size.Height * Scale) / 2;
                    
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(Rotation * 180.0f / (float)Math.PI); // Конвертируем радианы в градусы
                    g.TranslateTransform(-centerX, -centerY);
                    
                    // Рисуем спрайт
                    Owner.Game.Sprites.DrawSprite(g, SpriteName, screenPos.X, screenPos.Y, Scale, SpriteRegion);
                    
                    // Возвращаем исходную трансформацию
                    g.Transform = transform;
                }
                else if (FlipHorizontal)
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
                
                // рисуем эффект наведения для двери
                if (isDoorHovered)
                {
                    // подсвечиваем только по размеру спрайта (16x16), а не по коллайдеру
                    float highlightWidth = 16 * Scale;
                    float highlightHeight = 16 * Scale;
                    using (var pen = new Pen(Color.Yellow, 3.0f))
                    {
                        g.DrawRectangle(pen, screenPos.X, screenPos.Y, highlightWidth, highlightHeight);
                    }
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
                
                // рисуем эффект наведения для двери
                if (isDoorHovered)
                {
                    // подсвечиваем только по размеру спрайта (16x16), а не по коллайдеру
                    float highlightWidth = 16 * Scale;
                    float highlightHeight = 16 * Scale;
                    using (var pen = new Pen(Color.Yellow, 3.0f))
                    {
                        g.DrawRectangle(pen, screenPos.X, screenPos.Y, highlightWidth, highlightHeight);
                    }
                }
            }
        }
    }
}