using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace InfinitePenanceRL
{
    // Менеджер спрайтов - грузит и хранит все текстурки
    public class SpriteManager
    {
        private readonly Dictionary<string, Bitmap> _spritesheets = new Dictionary<string, Bitmap>();
        private readonly Dictionary<string, Rectangle> _spriteRegions = new Dictionary<string, Rectangle>();
        private const int TILE_SIZE = 16;
        private HashSet<string> _loggedSprites = new HashSet<string>();

        private void Log(string message)
        {
            using (StreamWriter writer = File.AppendText("game.log"))
            {
                writer.WriteLine($"[Sprite] {message}");
            }
        }

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
                _spritesheets["warrior"] = new Bitmap("assets/animations/warrior_48px.png");
                _spritesheets["projectiles_16px"] = new Bitmap("assets/animations/projectiles_16px.png");
                _spritesheets["ghoul"] = new Bitmap("assets/animations/ghoul_48px.png");
                Log("Загрузили все спрайтшиты");

                // Задаем регионы для всех спрайтов (размер 16x16)
                
                // Игрок - берем кадр анимации "стоим на месте"
                _spriteRegions["player"] = GetTileRect(1, 2, "warrior");
                
                // Предметы
                _spriteRegions["potion"] = GetTileRect(5, 3, "items");
                _spriteRegions["sword"] = GetTileRect(6, 0, "items");
                
                // Варианты стен
                _spriteRegions["wall_top_left"] = GetTileRect(0, 0, "walls");
                _spriteRegions["wall_top"] = GetTileRect(1, 0, "walls");
                _spriteRegions["wall_top_right"] = GetTileRect(2, 0, "walls");
                _spriteRegions["wall_left"] = GetTileRect(0, 1, "walls");
                _spriteRegions["wall_middle"] = GetTileRect(1, 1, "walls");
                _spriteRegions["wall_right"] = GetTileRect(2, 1, "walls");
                _spriteRegions["wall_bottom_left"] = GetTileRect(0, 2, "walls");
                _spriteRegions["wall_bottom"] = GetTileRect(1, 2, "walls");
                _spriteRegions["wall_bottom_right"] = GetTileRect(2, 2, "walls");
                _spriteRegions["wall_diagonal_tl"] = GetTileRect(6, 2, "walls");  // Диагональ сверху-слева
                _spriteRegions["wall_diagonal_tr"] = GetTileRect(5, 2, "walls");  // Диагональ сверху-справа
                _spriteRegions["wall_diagonal_bl"] = GetTileRect(6, 1, "walls");  // Диагональ снизу-слева
                _spriteRegions["wall_diagonal_br"] = GetTileRect(5, 1, "walls");  // Диагональ снизу-справа
                
                // Пол
                _spriteRegions["floor"] = GetTileRect(0, 1, "floors");
                Log("Задали все регионы спрайтов");
            }
            catch (Exception ex)
            {
                Log($"ОШИБКА при загрузке спрайтов: {ex.Message}");
                Log($"Текущая директория: {Environment.CurrentDirectory}");
            }
        }

        public void DrawSprite(Graphics g, string spriteName, float x, float y, float scale = 2.0f, Rectangle? customRegion = null)
        {
            string spritesheet = GetSpritesheetForSprite(spriteName);
            if (string.IsNullOrEmpty(spritesheet) || !_spritesheets.ContainsKey(spritesheet))
            {
                if (!_loggedSprites.Contains(spriteName))
                {
                    Log($"Не нашли спрайтшит для {spriteName}");
                    _loggedSprites.Add(spriteName);
                }
                return;
            }

            Rectangle region;
            if (customRegion.HasValue)
            {
                region = customRegion.Value;
            }
            else if (!_spriteRegions.TryGetValue(spriteName, out region))
            {
                if (!_loggedSprites.Contains(spriteName))
                {
                    Log($"Не нашли регион для спрайта {spriteName}");
                    _loggedSprites.Add(spriteName);
                }
                return;
            }

            try
            {
                // Настраиваем рендер для пиксель-арта
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
                if (!_loggedSprites.Contains(spriteName))
                {
                    Log($"ОШИБКА при отрисовке спрайта {spriteName}: {ex.Message}");
                    _loggedSprites.Add(spriteName);
            }
        }
        }

        // Определяем, какой спрайтшит использовать для спрайта
        private string GetSpritesheetForSprite(string spriteName)
        {
            if (spriteName.StartsWith("wall_"))
                return "walls";
                
            return spriteName switch
            {
                "player" => "warrior",
                "enemy" => "ghoul",
                "potion" or "sword" => "items",
                "floor" => "floors",
                "projectiles_16px" => "projectiles_16px",
                _ => string.Empty
            };
        }

        // Получаем прямоугольник для тайла из спрайтшита
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