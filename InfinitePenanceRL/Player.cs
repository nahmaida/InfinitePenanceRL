using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace InfinitePenanceRL
{
    public static class Player
    {
        public static float Health { get; set; } = 100f;
        public static float Damage { get; set; } = 10f;
        public static float Speed { get; set; } = 5f;
        public static List<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
        public static Vector2 Position { get; set; } = new Vector2(100, 100);

        private const string SaveFilePath = "player_save.json";

        public static void SaveToFile()
        {
            var playerData = new PlayerData
            {
                Health = Health,
                Damage = Damage,
                Speed = Speed,
                PositionX = Position.X,
                PositionY = Position.Y,
                Inventory = Inventory.Select(item => new InventoryItemData
                {
                    Name = item.Name,
                    Count = item.Count,
                    ColorArgb = item.Color.ToArgb(),
                    IsStackable = item.IsStackable,
                    SpriteName = item.SpriteName
                }).ToList()
            };

            string json = JsonSerializer.Serialize(playerData);
            File.WriteAllText(SaveFilePath, json);
        }

        public static void LoadFromFile()
        {
            if (File.Exists(SaveFilePath))
            {
                string json = File.ReadAllText(SaveFilePath);
                var playerData = JsonSerializer.Deserialize<PlayerData>(json);

                if (playerData != null)
                {
                    Health = playerData.Health;
                    Damage = playerData.Damage;
                    Speed = playerData.Speed;
                    Position = new Vector2(playerData.PositionX, playerData.PositionY);
                    Inventory = playerData.Inventory.Select(data => new InventoryItem(data.Name, Color.FromArgb(data.ColorArgb))
                    {
                        Count = data.Count,
                        IsStackable = data.IsStackable,
                        SpriteName = data.SpriteName
                    }).ToList();
                }
            }
        }
    }

    public class PlayerData
    {
        public float Health { get; set; }
        public float Damage { get; set; }
        public float Speed { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public List<InventoryItemData> Inventory { get; set; } = new List<InventoryItemData>();
    }

    public class InventoryItemData
    {
        public string Name { get; set; } = "";
        public int Count { get; set; } = 1;
        public int ColorArgb { get; set; }
        public bool IsStackable { get; set; } = true;
        public string SpriteName { get; set; } = "";
    }
}