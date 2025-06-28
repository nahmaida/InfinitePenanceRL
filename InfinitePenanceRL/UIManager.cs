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

            // Система прокачки
            var levelingUI = new LevelingUIComponent();

            // Привязываем компоненты к UI объекту
            _uiEntity.AddComponent(healthBar);
            _uiEntity.AddComponent(inventory);
            _uiEntity.AddComponent(levelingUI);

            // Добавляем компоненты в список для отрисовки
            _uiComponents.Add(healthBar);
            _uiComponents.Add(inventory);
            _uiComponents.Add(levelingUI);

            // Создаем меню паузы
            var pauseMenu = new PauseMenuComponent { ScreenPosition = new Vector2(0, 0), IsVisible = true };
            _uiEntity.AddComponent(pauseMenu);
            _uiComponents.Add(pauseMenu);
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

            // Рисуем систему прокачки отдельно, так как ей нужен размер viewport
            var levelingUI = GetLevelingUI();
            if (levelingUI != null)
            {
                levelingUI.Draw(g, _game.Camera.ViewportSize);
            }
        }

        // Получить компонент интерфейса определённого типа
        public T GetComponent<T>() where T : UIComponent
        {
            return _uiComponents.OfType<T>().FirstOrDefault();
        }

        public PauseMenuComponent GetPauseMenu()
        {
            return _uiComponents.OfType<PauseMenuComponent>().FirstOrDefault();
        }

        public LevelingUIComponent GetLevelingUI()
        {
            return _uiComponents.OfType<LevelingUIComponent>().FirstOrDefault();
        }

        // Обновление всех UI-компонентов
        public void Update()
        {
            foreach (var component in _uiComponents)
            {
                component.Update();
            }
        }
    }
} 