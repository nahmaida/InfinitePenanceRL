using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InfinitePenanceRL
{
    // Система физики - проверяет столкновения и возможность движения
    public class PhysicsSystem
    {
        public void Update(Scene scene)
        {
            // не нужно теперь по факту
        }

        // Проверяет, может ли объект переместиться в новую позицию
        public bool CanMoveTo(Entity entity, Vector2 newPosition)
        {
            var movingCollider = entity.GetComponent<ColliderComponent>();
            if (movingCollider == null || !movingCollider.IsSolid) return true;

            var render = entity.GetComponent<RenderComponent>();
            if (render == null) return true;

            // Проверяем, чтобы объект не вышел за пределы игрового мира
            if (newPosition.X < 0 || 
                newPosition.Y < 0 || 
                newPosition.X + render.Size.Width > entity.Game.WorldSize.Width ||
                newPosition.Y + render.Size.Height > entity.Game.WorldSize.Height)
            {
                return false;
            }

            // Создаем прямоугольник для проверки столкновений в новой позиции
            var testBounds = new RectangleF(
                newPosition.X,
                newPosition.Y,
                render.Size.Width,
                render.Size.Height);

            // Проверяем столкновения со всеми твёрдыми объектами на сцене
            foreach (var other in entity.Game.CurrentScene.Entities)
            {
                if (other == entity) continue;  // Пропускаем сам объект

                var otherCollider = other.GetComponent<ColliderComponent>();
                if (otherCollider == null || !otherCollider.IsSolid) continue;  // Пропускаем объекты без коллайдера или нетвёрдые

                // Если есть пересечение с другим объектом - движение невозможно
                if (testBounds.IntersectsWith(otherCollider.Bounds))
                {
                    return false;
                }
            }

            // Если столкновений нет - можно двигаться
            return true;
        }
    }
}