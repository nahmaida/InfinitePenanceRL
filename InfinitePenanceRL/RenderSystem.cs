using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    public class RenderSystem
    {
        public void Render(Graphics g, GameEngine game, IEnumerable<Entity> entities)
        {
            // БЕЛЫЙ фон
            g.Clear(Color.White);

            // Фиксируем в границах экрана
            g.SetClip(new Rectangle(Point.Empty, game.Camera.ViewportSize));

            Console.WriteLine("\n--- Frame Start ---");
            foreach (var entity in entities)
            {
                var render = entity.GetComponent<RenderComponent>();
                if (render != null)
                {
                    Console.WriteLine($"Rendering entity at {entity.Position} with sprite: {render.SpriteName}");
                    var screenPos = entity.Game.Camera.WorldToScreen(entity.Position);
                    render.Draw(g);

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
            Console.WriteLine("--- Frame End ---\n");
            g.ResetClip();
        }
    }
}