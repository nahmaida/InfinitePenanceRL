using System.Drawing;
using System.Collections.Generic;

namespace InfinitePenanceRL
{
    // Управляет всеми элементами UI
    public class UIManager
    {
        private readonly List<UIComponent> _uiComponents = new List<UIComponent>();  // Список всех UI элементов
        private readonly GameEngine _game;  // Ссылка на движок игры
        private readonly Entity _uiEntity;  // Специальный объект для UI компонентов

        public UIManager(GameEngine game)
        {
            _game = game;
            _uiEntity = new Entity { Game = game };
            InitializeUI();
        }

        // Создаём все элементы интерфейса
        private void InitializeUI()
        {
            // Полоска здоровья в левом верхнем углу
            var healthBar = new HealthBarComponent
            {
                ScreenPosition = new Vector2(10, 10),
                MaxHealth = 100,
                CurrentHealth = 100
            };

            // Инвентарь под полоской здоровья
            var inventory = new InventoryComponent
            {
                ScreenPosition = new Vector2(10, 40),
                SlotCount = 8  // Количество слотов в инвентаре
            };

            // Добавляем тестовые предметы в инвентарь
            inventory.AddItem(new InventoryItem("Зелье", Color.Red) { Count = 3, SpriteName = "potion" });  // Три зелья лечения
            inventory.AddItem(new InventoryItem("Меч", Color.LightBlue) { SpriteName = "sword" });  // Один меч

            // Привязываем компоненты к UI объекту
            _uiEntity.AddComponent(healthBar);
            _uiEntity.AddComponent(inventory);

            // Добавляем компоненты в список для отрисовки
            _uiComponents.Add(healthBar);
            _uiComponents.Add(inventory);
        }

        // Отрисовка всех видимых элементов интерфейса
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

        // Получить компонент интерфейса определённого типа
        public T GetComponent<T>() where T : UIComponent
        {
            return _uiComponents.OfType<T>().FirstOrDefault();
        }
    }
} 