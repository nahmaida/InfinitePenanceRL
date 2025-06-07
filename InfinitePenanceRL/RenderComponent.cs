using System.Drawing;

namespace InfinitePenanceRL
{
    // Рендер перса
    public class RenderComponent : Component
    {
        public Color Color { get; set; } = Color.Red;
        public Size Size { get; set; } = new Size(32, 32);
        public string SpriteName { get; set; }
        public float Scale { get; set; } = 2.0f;
        private const int TILE_SIZE = 16;

        public void Draw(Graphics g)
        {
            if (Owner == null || g == null) return;

            var screenPos = Owner.Game.Camera.WorldToScreen(Owner.Position);

            if (!string.IsNullOrEmpty(SpriteName))
            {
                if (SpriteName == "wall")
                {
                    // стены
                    int tilesX = Size.Width / (int)(TILE_SIZE * Scale);
                    int tilesY = Size.Height / (int)(TILE_SIZE * Scale);
                    Console.WriteLine($"Drawing wall: {tilesX}x{tilesY} tiles at {screenPos}");

                    for (int y = 0; y < tilesY; y++)
                    {
                        for (int x = 0; x < tilesX; x++)
                        {
                            float tileX = screenPos.X + x * TILE_SIZE * Scale;
                            float tileY = screenPos.Y + y * TILE_SIZE * Scale;
                            Owner.Game.Sprites.DrawSprite(g, SpriteName, tileX, tileY, Scale);
                        }
                    }
                }
                else
                {
                    // спрайт
                    Console.WriteLine($"Drawing sprite {SpriteName} at {screenPos}");
                    Owner.Game.Sprites.DrawSprite(g, SpriteName, screenPos.X, screenPos.Y, Scale);
                }
            }
            else
            {
                // если нет спрайта, то рисуем прямоугольник
                Console.WriteLine("ERROR 404 SPRITE NOT FOUND!");
                g.FillRectangle(new SolidBrush(Color),
                    screenPos.X, screenPos.Y,
                    Size.Width, Size.Height);
            }
        }
    }
}