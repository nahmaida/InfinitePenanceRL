using System.Drawing;
using System.Collections.Generic;

namespace InfinitePenanceRL
{
    public class UIManager
    {
        private readonly List<UIComponent> _uiComponents = new List<UIComponent>();
        private readonly GameEngine _game;
        private readonly Entity _uiEntity;

        public UIManager(GameEngine game)
        {
            _game = game;
            _uiEntity = new Entity { Game = game };
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Строка здоровья
            var healthBar = new HealthBarComponent
            {
                ScreenPosition = new Vector2(10, 10),
                MaxHealth = 100,
                CurrentHealth = 100
            };

            // Инвентарь
            var inventory = new InventoryComponent
            {
                ScreenPosition = new Vector2(10, 40),
                SlotCount = 8
            };

            // Несколько тестовых предметов
            inventory.AddItem(new InventoryItem("Зелье", Color.Red) { Count = 3, SpriteName = "potion" });
            inventory.AddItem(new InventoryItem("Меч", Color.LightBlue) { SpriteName = "sword" });

            _uiEntity.AddComponent(healthBar);
            _uiEntity.AddComponent(inventory);

            _uiComponents.Add(healthBar);
            _uiComponents.Add(inventory);
        }

        public void Draw(Graphics g)
        {
            foreach (var component in _uiComponents)
            {
                if (component.IsVisible)
                {
                    component.Draw(g);
                }
            }
        }

        public T GetComponent<T>() where T : UIComponent
        {
            return _uiComponents.OfType<T>().FirstOrDefault();
        }
    }
} 