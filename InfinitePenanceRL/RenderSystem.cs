using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    public class RenderSystem
    {
        public void Render(Graphics g, IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                var render = entity.GetComponent<RenderComponent>();
                if (render != null)
                {
                    var screenPos = entity.Game.Camera.WorldToScreen(entity.Position);
                    g.FillRectangle(new SolidBrush(render.Color),
                        screenPos.X, screenPos.Y,
                        render.Size.Width, render.Size.Height);

                    // Тестовые стены
                    var collider = entity.GetComponent<ColliderComponent>();
                    if (collider != null)
                    {
                        var colliderScreen = new RectangleF(
                            screenPos.X,
                            screenPos.Y,
                            collider.Bounds.Width,
                            collider.Bounds.Height);
                        g.DrawRectangle(Pens.White,
                            colliderScreen.X, colliderScreen.Y,
                            colliderScreen.Width, colliderScreen.Height);
                    }
                }
            }
        }
    }
}