using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    public class RenderSystem
    {
        private const int FLOOR_TILE_SIZE = 48; // 16 * 3 (масштаб)

        public void Render(Graphics g, GameEngine game, IEnumerable<Entity> entities)
        {
            // Очищаем фон
            g.Clear(ColorTranslator.FromHtml("#5E6356"));

            // Устанавливаем область отсечения по размеру окна
            g.SetClip(new Rectangle(Point.Empty, game.Camera.ViewportSize));

            // Рисуем тайлы пола
            DrawFloorTiles(g, game);

            // Рисуем сущности
            foreach (var entity in entities)
            {
                var render = entity.GetComponent<RenderComponent>();
                if (render != null)
                {
                    var screenPos = entity.Game.Camera.WorldToScreen(entity.Position);
                    render.Draw(g);

                //     // Рисуем коллайдеры (для отладки)
                //     var collider = entity.GetComponent<ColliderComponent>();
                //     if (collider != null)
                //     {
                //         var colliderScreen = new RectangleF(
                //             screenPos.X,
                //             screenPos.Y,
                //             collider.Bounds.Width,
                //             collider.Bounds.Height);
                //         g.DrawRectangle(Pens.White,
                //             colliderScreen.X, colliderScreen.Y,
                //             colliderScreen.Width, colliderScreen.Height);
                //     }
                }
            }

            g.ResetClip();
        }

        private void DrawFloorTiles(Graphics g, GameEngine game)
        {
            // Вычисляем видимую область в мировых координатах
            var viewportWorldPos = game.Camera.ScreenToWorld(Point.Empty);
            var viewportBottomRight = game.Camera.ScreenToWorld(new Point(
                game.Camera.ViewportSize.Width,
                game.Camera.ViewportSize.Height
            ));

            // Вычисляем диапазон тайлов для отрисовки
            int startX = ((int)viewportWorldPos.X / FLOOR_TILE_SIZE) - 1;
            int startY = ((int)viewportWorldPos.Y / FLOOR_TILE_SIZE) - 1;
            int endX = ((int)viewportBottomRight.X / FLOOR_TILE_SIZE) + 1;
            int endY = ((int)viewportBottomRight.Y / FLOOR_TILE_SIZE) + 1;

            // Рисуем видимые тайлы пола
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var worldPos = new Vector2(x * FLOOR_TILE_SIZE, y * FLOOR_TILE_SIZE);
                    var screenPos = game.Camera.WorldToScreen(worldPos);
                    game.Sprites.DrawSprite(g, "floor", screenPos.X, screenPos.Y, 3.0f);
                }
            }
        }
    }
}