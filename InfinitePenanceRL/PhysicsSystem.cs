using System.Collections.Generic;
using System.Linq;

namespace InfinitePenanceRL
{
    public class PhysicsSystem
    {
        public void Update(Scene scene)
        {
            // не нужно теперь по факту
        }

        public bool CanMoveTo(Entity entity, Vector2 newPosition)
        {
            var movingCollider = entity.GetComponent<ColliderComponent>();
            if (movingCollider == null || !movingCollider.IsSolid) return true;

            var render = entity.GetComponent<RenderComponent>();
            if (render == null) return true;

            var testBounds = new RectangleF(
                newPosition.X,
                newPosition.Y,
                render.Size.Width,
                render.Size.Height);

            foreach (var other in entity.Game.CurrentScene.Entities)
            {
                if (other == entity) continue;

                var otherCollider = other.GetComponent<ColliderComponent>();
                if (otherCollider == null || !otherCollider.IsSolid) continue;

                if (testBounds.IntersectsWith(otherCollider.Bounds))
                {
                    return false;
                }
            }

            return true;
        }
    }
}