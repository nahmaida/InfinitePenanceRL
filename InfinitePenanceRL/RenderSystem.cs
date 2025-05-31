using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    public class RenderSystem
    {
        public void Render(Graphics g, GameEngine game, IEnumerable<Entity> entities)
        {
            // Черный фон
            g.Clear(Color.Black);

            // Фиксируем в границах экрана
            g.SetClip(new Rectangle(Point.Empty, game.Camera.ViewportSize));

            foreach (var entity in entities)
            {
                var render = entity.GetComponent<RenderComponent>();
                if (render != null)
                {
                    var screenPos = entity.Game.Camera.WorldToScreen(entity.Position);
                    g.FillRectangle(new SolidBrush(render.Color),
                        screenPos.X, screenPos.Y,
                        render.Size.Width, render.Size.Height);

                    // Рисуем коллайдеры (можно потом убрать)
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
            g.ResetClip();
        }
    }
}