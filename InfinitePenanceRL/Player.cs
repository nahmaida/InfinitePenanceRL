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
        public static float MaxHealth { get; set; } = 100f;
        public static float Damage { get; set; } = 10f;
        public static float Speed { get; set; } = 5f;
        public static List<InventoryItem> Inventory { get; set; } = new List<InventoryItem>();
        public static Vector2 Position { get; set; } = new Vector2(100, 100);
        
        // система прокачки
        public static int Level { get; set; } = 1;
        public static int Experience { get; set; } = 0;
        public static int KillsForNextLevel { get; set; } = 3; // количество убийств для следующего уровня
        public static int CurrentKills { get; set; } = 0;
        public static bool CanLevelUp { get; set; } = false;
        
        // система оружия
        public static float WeaponDamageBonus { get; set; } = 0f; // бонус урона от оружия
        
        // таймер для регенерации здоровья
        private static float regenerationTimer = 0f;
        private static float regenerationInterval = 1f; // 1 секунда

        public static void Update(float deltaTime)
        {
            // регенерация здоровья
            if (Health < MaxHealth)
            {
                regenerationTimer += deltaTime;
                if (regenerationTimer >= regenerationInterval)
                {
                    Health = Math.Min(MaxHealth, Health + 1f);
                    regenerationTimer = 0f;
                }
            }
            else
            {
                regenerationTimer = 0f;
            }
        }

        // добавляем опыт за убийство врага
        public static void AddKill()
        {
            CurrentKills++;
            Experience++;
            
            // проверяем, можно ли повысить уровень
            if (CurrentKills >= KillsForNextLevel)
            {
                CanLevelUp = true;
            }
        }

        // повышение уровня
        public static void LevelUp(StatUpgrade upgrade)
        {
            if (!CanLevelUp) return;
            
            Level++;
            CurrentKills = 0;
            CanLevelUp = false;
            
            // увеличиваем количество убийств для следующего уровня
            KillsForNextLevel = 3 + (Level - 1) * 2; // каждый уровень требует на 2 убийства больше
            
            // применяем улучшение статов
            switch (upgrade)
            {
                case StatUpgrade.Health:
                    MaxHealth += 20f;
                    Health = MaxHealth; // восстанавливаем здоровье до максимума при улучшении
                    break;
                case StatUpgrade.Damage:
                    Damage += 5f;
                    break;
                case StatUpgrade.Speed:
                    Speed += 1f;
                    break;
            }
        }

        // получаем общий урон (базовый + от оружия)
        public static float GetTotalDamage()
        {
            return Damage + WeaponDamageBonus;
        }

        public static PlayerData GetSaveData()
        {
            return new PlayerData
            {
                Health = Health,
                MaxHealth = MaxHealth,
                Damage = Damage,
                Speed = Speed,
                PositionX = Position.X,
                PositionY = Position.Y,
                Level = Level,
                Experience = Experience,
                KillsForNextLevel = KillsForNextLevel,
                CurrentKills = CurrentKills,
                WeaponDamageBonus = WeaponDamageBonus,
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
            MaxHealth = data.MaxHealth;
            Damage = data.Damage;
            Speed = data.Speed;
            Position = new Vector2(data.PositionX, data.PositionY);
            Level = data.Level;
            Experience = data.Experience;
            KillsForNextLevel = data.KillsForNextLevel;
            CurrentKills = data.CurrentKills;
            WeaponDamageBonus = data.WeaponDamageBonus;
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
            public float MaxHealth { get; set; }
            public float Damage { get; set; }
            public float Speed { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public int Level { get; set; }
            public int Experience { get; set; }
            public int KillsForNextLevel { get; set; }
            public int CurrentKills { get; set; }
            public float WeaponDamageBonus { get; set; }
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

    public enum StatUpgrade
    {
        Health,
        Damage,
        Speed
    }
}