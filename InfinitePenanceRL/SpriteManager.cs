using System.Drawing;
using System.Collections.Generic;

namespace InfinitePenanceRL
{
    public class SpriteManager
    {
        private readonly Dictionary<string, Bitmap> _spritesheets = new Dictionary<string, Bitmap>();
        private readonly Dictionary<string, Rectangle> _spriteRegions = new Dictionary<string, Rectangle>();
        private const int TILE_SIZE = 16;

        public void LoadSpritesheets()
        {
            try
            {
                // Load spritesheets
                _spritesheets["characters"] = new Bitmap("assets/roguelike_characters.png");
                _spritesheets["items"] = new Bitmap("assets/roguelike_items.png");
                _spritesheets["walls"] = new Bitmap("assets/roguelike_walls.png");
                _spritesheets["floors"] = new Bitmap("assets/roguelike_floors.png");
                _spritesheets["ui"] = new Bitmap("assets/roguelike_ui.png");

                // Define sprite regions (16x16 each)
                
                // Player
                _spriteRegions["player"] = GetTileRect(6, 0, "characters");
                
                // Items
                _spriteRegions["potion"] = GetTileRect(5, 3, "items");
                _spriteRegions["sword"] = GetTileRect(6, 0, "items");
                
                // Wall variants
                _spriteRegions["wall_top_left"] = GetTileRect(0, 0, "walls");
                _spriteRegions["wall_top"] = GetTileRect(1, 0, "walls");
                _spriteRegions["wall_top_right"] = GetTileRect(2, 0, "walls");
                _spriteRegions["wall_left"] = GetTileRect(0, 1, "walls");
                _spriteRegions["wall_middle"] = GetTileRect(1, 1, "walls");
                _spriteRegions["wall_right"] = GetTileRect(2, 1, "walls");
                _spriteRegions["wall_bottom_left"] = GetTileRect(0, 2, "walls");
                _spriteRegions["wall_bottom"] = GetTileRect(1, 2, "walls");
                _spriteRegions["wall_bottom_right"] = GetTileRect(2, 2, "walls");
                _spriteRegions["wall_diagonal_br"] = GetTileRect(5, 1, "walls");
                _spriteRegions["wall_diagonal_bl"] = GetTileRect(6, 1, "walls");
                
                // Floor
                _spriteRegions["floor"] = GetTileRect(0, 1, "floors");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading sprites: {ex.Message}");
                Console.WriteLine($"Current directory: {Environment.CurrentDirectory}");
            }
        }

        public void DrawSprite(Graphics g, string spriteName, float x, float y, float scale = 2.0f)
        {
            string spritesheet = GetSpritesheetForSprite(spriteName);
            if (string.IsNullOrEmpty(spritesheet) || !_spritesheets.ContainsKey(spritesheet))
                return;

            if (!_spriteRegions.TryGetValue(spriteName, out Rectangle region))
                return;

            try
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                var destRect = new RectangleF(x, y, region.Width * scale, region.Height * scale);

                g.DrawImage(_spritesheets[spritesheet],
                    destRect,
                    region,
                    GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR drawing sprite {spriteName}: {ex.Message}");
            }
        }

        private string GetSpritesheetForSprite(string spriteName)
        {
            if (spriteName.StartsWith("wall_"))
                return "walls";
                
            return spriteName switch
            {
                "player" => "characters",
                "potion" or "sword" => "items",
                "floor" => "floors",
                _ => string.Empty
            };
        }

        private Rectangle GetTileRect(int x, int y, string spritesheet)
        {
            if (_spritesheets.TryGetValue(spritesheet, out Bitmap bmp))
            {
                int maxX = bmp.Width / TILE_SIZE - 1;
                int maxY = bmp.Height / TILE_SIZE - 1;
                
                x = Math.Min(x, maxX);
                y = Math.Min(y, maxY);
                
                return new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            }
            return new Rectangle(0, 0, TILE_SIZE, TILE_SIZE);
        }

        public void Dispose()
        {
            foreach (var spritesheet in _spritesheets.Values)
            {
                spritesheet.Dispose();
            }
            _spritesheets.Clear();
        }
    }
} 