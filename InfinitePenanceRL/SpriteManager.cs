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
                // Загружаем все спрайтшиты
                _spritesheets["characters"] = new Bitmap("assets/roguelike_characters.png");
                _spritesheets["items"] = new Bitmap("assets/roguelike_items.png");
                _spritesheets["walls"] = new Bitmap("assets/roguelike_walls.png");
                _spritesheets["floors"] = new Bitmap("assets/roguelike_floors.png");
                _spritesheets["ui"] = new Bitmap("assets/roguelike_ui.png");

                Console.WriteLine("Loaded spritesheets:");
                foreach (var sheet in _spritesheets)
                {
                    Console.WriteLine($"- {sheet.Key}: {sheet.Value.Width}x{sheet.Value.Height}");
                }

                // Определяем регионы для спрайтов (16x16 каждый)
                
                // Игрок
                _spriteRegions["player"] = GetTileRect(3, 0, "characters");
                
                // Предметы
                _spriteRegions["potion"] = GetTileRect(5, 3, "items");  // зелье
                _spriteRegions["sword"] = GetTileRect(6, 0, "items");   // меч
                
                // Стены (каменная стена)
                _spriteRegions["wall"] = GetTileRect(2, 0, "walls");
                
                // Пол (каменный пол)
                _spriteRegions["floor"] = GetTileRect(0, 0, "floors");

                Console.WriteLine("\nDefined sprite regions:");
                foreach (var region in _spriteRegions)
                {
                    Console.WriteLine($"- {region.Key}: {region.Value}");
                }
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
            if (string.IsNullOrEmpty(spritesheet))
            {
                Console.WriteLine($"Spritesheet not found for sprite: {spriteName}");
                return;
            }

            if (!_spritesheets.ContainsKey(spritesheet))
            {
                Console.WriteLine($"Spritesheet not loaded: {spritesheet}");
                return;
            }

            if (!_spriteRegions.TryGetValue(spriteName, out Rectangle region))
            {
                Console.WriteLine($"Region not found for sprite: {spriteName}");
                return;
            }

            try
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                var destRect = new RectangleF(x, y, region.Width * scale, region.Height * scale);
                Console.WriteLine($"Drawing {spriteName} from {spritesheet} - Source: {region}, Dest: {destRect}, Scale: {scale}");

                g.DrawImage(_spritesheets[spritesheet],
                    destRect,
                    region,
                    GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR drawing sprite {spriteName}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private string GetSpritesheetForSprite(string spriteName)
        {
            return spriteName switch
            {
                "player" => "characters",
                "potion" or "sword" => "items",
                "wall" => "walls",
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
                
                // Убеждаемся что не выходим за границы
                x = Math.Min(x, maxX);
                y = Math.Min(y, maxY);
                
                var rect = new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                Console.WriteLine($"Created region for {spritesheet} [{x},{y}]: {rect}");
                return rect;
            }
            Console.WriteLine($"ERROR: Spritesheet {spritesheet} not found for creating region [{x},{y}]");
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