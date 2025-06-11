using System.Drawing;
using System.IO;

namespace InfinitePenanceRL
{
    // Компонент коллайдера - отвечает за физические границы объекта и проверку столкновений
    public class ColliderComponent : Component
    {
        private RectangleF _bounds;  // Границы объекта
        public bool IsSolid { get; set; } = true;  // Можно ли сквозь объект проходить
        private bool _hasLoggedBounds = false;  // Флаг для одноразового логирования границ

        // Возвращает текущие границы объекта, обновляя их при необходимости
        public RectangleF Bounds
        {
            get
            {
                UpdateBounds();
                return _bounds;
            }
        }

        public override void Update()
        {
            UpdateBounds();
        }

        // Записываем отладочную информацию в лог
        private void Log(string message)
        {
            using (StreamWriter writer = File.AppendText("game.log"))
            {
                writer.WriteLine($"[Collider] {message}");
            }
        }

        // Обновляем границы объекта на основе его позиции и размера спрайта
        public void UpdateBounds()
        {
            if (Owner == null) return;

            var render = Owner.GetComponent<RenderComponent>();
            if (render != null)
            {
                // Для игрока (и других объектов с регионом спрайта) используем реальный размер спрайта
                if (render.SpriteRegion.HasValue)
                {
                    float width = render.SpriteRegion.Value.Width * render.Scale;
                    float height = render.SpriteRegion.Value.Height * render.Scale;

                    _bounds = new RectangleF(
                        Owner.Position.X,
                        Owner.Position.Y,
                        width,
                        height);

                    // Логируем размеры один раз для отладки
                    if (!_hasLoggedBounds)
                    {
                        Log($"Границы объекта: Позиция({_bounds.X}, {_bounds.Y}), Размер({_bounds.Width}, {_bounds.Height})");
                        _hasLoggedBounds = true;
                    }
                }
                else
                {
                    // Для остальных объектов используем размер из RenderComponent
                    _bounds = new RectangleF(
                        Owner.Position.X,
                        Owner.Position.Y,
                        render.Size.Width,
                        render.Size.Height);
                }
            }
        }

        // Проверяем столкновение с другим коллайдером
        public bool CheckCollision(ColliderComponent other)
        {
            if (other == null || Owner == null || other.Owner == null)
                return false;

            bool collides = Bounds.IntersectsWith(other.Bounds);
            if (collides)
            {
                Log($"Обнаружено столкновение между объектами в {Bounds} и {other.Bounds}");
            }
            return collides;
        }
    }
}