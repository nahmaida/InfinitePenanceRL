using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    // Система отрисовки - рисует всё, что видит игрок
    public class RenderSystem
    {
        private const int FLOOR_TILE_SIZE = 48; // Размер тайла пола (16 пикселей * 3 - масштаб)

        public void Render(Graphics g, GameEngine game, IEnumerable<Entity> entities)
        {
            // Заливаем фон серым цветом
            g.Clear(ColorTranslator.FromHtml("#5E6356"));

            // Ограничиваем область рисования размером окна
            g.SetClip(new Rectangle(Point.Empty, game.Camera.ViewportSize));

            // Сначала рисуем пол (чтобы он был под всеми объектами)
            DrawFloorTiles(g, game);

            // Теперь рисуем все игровые объекты
            foreach (var entity in entities)
            {
                var render = entity.GetComponent<RenderComponent>();
                if (render != null)
                {
                    var screenPos = entity.Game.Camera.WorldToScreen(entity.Position);
                    render.Draw(g);

                    // // Для отладки: рисуем границы коллайдеров белым цветом
                    // var collider = entity.GetComponent<ColliderComponent>();
                    // if (collider != null)
                    // {
                    //     var colliderScreen = new RectangleF(
                    //         screenPos.X,
                    //         screenPos.Y,
                    //         collider.Bounds.Width,
                    //         collider.Bounds.Height);
                    //     g.DrawRectangle(Pens.White,
                    //         colliderScreen.X, colliderScreen.Y,
                    //         colliderScreen.Width, colliderScreen.Height);
                    // }
                }
            }

            g.ResetClip();
        }

        // Рисуем тайлы пола в видимой области
        private void DrawFloorTiles(Graphics g, GameEngine game)
        {
            // Находим границы видимой области в координатах игрового мира
            var viewportWorldPos = game.Camera.ScreenToWorld(Point.Empty);
            var viewportBottomRight = game.Camera.ScreenToWorld(new Point(
                game.Camera.ViewportSize.Width,
                game.Camera.ViewportSize.Height
            ));

            // Считаем, какие тайлы пола нужно нарисовать
            // Добавляем по одному тайлу с каждой стороны для плавного скроллинга
            int startX = ((int)viewportWorldPos.X / FLOOR_TILE_SIZE) - 1;
            int startY = ((int)viewportWorldPos.Y / FLOOR_TILE_SIZE) - 1;
            int endX = ((int)viewportBottomRight.X / FLOOR_TILE_SIZE) + 1;
            int endY = ((int)viewportBottomRight.Y / FLOOR_TILE_SIZE) + 1;

            // Рисуем все видимые тайлы пола по сетке
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