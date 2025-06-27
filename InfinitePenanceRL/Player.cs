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

        public static PlayerData GetSaveData()
        {
            return new PlayerData
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
        }

        public static void LoadFromSaveData(PlayerData data)
        {
            if (data == null) return;
            Health = data.Health;
            Damage = data.Damage;
            Speed = data.Speed;
            Position = new Vector2(data.PositionX, data.PositionY);
            Inventory = data.Inventory.Select(d => new InventoryItem(d.Name, Color.FromArgb(d.ColorArgb))
            {
                Count = d.Count,
                IsStackable = d.IsStackable,
                SpriteName = d.SpriteName
            }).ToList();
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
}