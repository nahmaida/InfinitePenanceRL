using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace InfinitePenanceRL
{
    // Главный движок игры - управляет всеми системами и обновлением состояния
    public class GameEngine
    {
        public GameState State { get; private set; } = GameState.Playing;  // Текущее состояние игры
        public Scene CurrentScene { get; private set; }  // Активная сцена
        public RenderSystem RenderSystem { get; } = new RenderSystem();  // Система отрисовки
        public InputManager Input { get; } = new InputManager();  // Обработка ввода
        public UIManager UI { get; private set; }  // Управление интерфейсом
        public SpriteManager Sprites { get; } = new SpriteManager();  // Управление спрайтами
        public Camera Camera { get; set; }  // Игровая камера
        public PhysicsSystem Physics { get; } = new PhysicsSystem();  // Физика и коллизии
        public Size WorldSize { get; set; }  // Размеры игрового мира
        public ParticleSystem Particles { get; } = new ParticleSystem();  // Система частиц
        public MusicManager Music { get; } = new MusicManager();  // Менеджер музыки
        private SoundManager _sounds;  // Менеджер звуковых эффектов (ленивая инициализация)
        public SoundManager Sounds 
        { 
            get 
            { 
                if (_sounds == null)
                    _sounds = new SoundManager();
                return _sounds; 
            } 
        }

        public List<Entity> Projectiles => _projectiles; // Публичный доступ к снарядам

        private System.Windows.Forms.Timer gameTimer;  // Таймер для обновления игры
        private MainForm mainForm;  // Главное окно игры
        private const int TARGET_FPS = 60;  // Целевой FPS
        private const int MAZE_WIDTH = 51;  // Ширина подземелья (увеличено для комнат и коридоров)
        private const int MAZE_HEIGHT = 51;  // Высота подземелья (увеличено для комнат и коридоров)
        private List<Entity> _entitiesToDelete = new List<Entity>(); // Список сущностей для удаления
        private bool _shouldProcessDeletions = false; // Флаг для обработки удалений в следующем кадре
        private List<Entity> _projectiles = new List<Entity>(); // Отдельный список для снарядов

        // Создаём новый движок и привязываем его к окну
        public GameEngine(MainForm form)
        {
            mainForm = form;
            Camera = new Camera(form.ClientSize);
            CurrentScene = new Scene(this);
            UI = new UIManager(this);

            InitializeGame();
            SetupGameTimer();
        }

        // Инициализация игры при запуске
        private void InitializeGame()
        {
            Initialize(mainForm.ClientSize);
        }

        // Настраиваем таймер для обновления игры
        private void SetupGameTimer()
        {
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000 / TARGET_FPS;  // Интервал между кадрами
            gameTimer.Tick += (sender, e) =>
            {
                Update();  // Обновляем состояние
                mainForm.Invalidate();  // Перерисовываем окно
            };
            gameTimer.Start();
        }

        // Инициализация или перезапуск игры
        public void Initialize(Size initialViewportSize)
        {
            // Загружаем сохраненные данные игрока
            // Player.LoadFromFile(); // Временно отключено для автогенерации карт
            
            CurrentScene = new Scene(this);
            Camera = new Camera(initialViewportSize);
            UI = new UIManager(this);
            Sprites.LoadSpritesheets();

            // Задаём размер мира на основе размеров лабиринта
            WorldSize = new Size(MAZE_WIDTH * Scene.CellSize, MAZE_HEIGHT * Scene.CellSize);

            // Создаём новый лабиринт
            CurrentScene.GenerateMaze(MAZE_WIDTH, MAZE_HEIGHT);
            
            // Запускаем фоновую музыку
            Music.StartMusic();
        }

        // Обновление состояния игры
        public void Update()
        {
            if (State != GameState.Playing) return;

            Physics.Update(CurrentScene);

            // обновляем игрока для регенерации здоровья
            Player.Update(1.0f / TARGET_FPS);

            // Логируем количество врагов и EnemyComponent
            int enemyCount = CurrentScene.Entities.Count(e => e.GetComponent<EnemyComponent>() != null);
            int enemyCompCount = CurrentScene.Entities.SelectMany(e => e.Components).OfType<EnemyComponent>().Count();
            // LogThrottler.Log($"Врагов на сцене: {enemyCount}, EnemyComponent всего: {enemyCompCount}", "enemy_debug");

            // Обновляем все компоненты всех объектов
            foreach (var entity in CurrentScene.Entities)
            {
                foreach (var component in entity.Components)
                {
                    component.Update();
                }
            }

            // Обновляем снаряды отдельно
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _projectiles[i];
                var projectileComponent = projectile.GetComponent<ProjectileComponent>();
                
                if (projectileComponent != null)
                {
                    projectileComponent.Update();
                    
                    // Если снаряд нужно удалить
                    if (projectileComponent.ShouldDelete())
                    {
                        _projectiles.RemoveAt(i);
                    }
                }
            }

            // Камера следует за игроком
            var player = CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                Camera.CenterOn(player.Position, WorldSize);
            }

            // Обновляем тряску камеры
            Camera.UpdateShake(1.0f / TARGET_FPS);

            // Обновляем систему частиц
            Particles.Update(1.0f / TARGET_FPS);

            // Убираем мёртвые объекты
            CurrentScene.CleanupMarkedEntities();

            // Обновляем UI (например, полоску здоровья)
            UI.Update();
        }

        // Отрисовка игры
        public void Render(Graphics graphics)
        {
            graphics.Clear(Color.Black);  // Чистим экран
            if (CurrentScene != null)
            {
                RenderSystem.Render(graphics, this, CurrentScene.Entities);  // Рисуем игровые объекты
                
                // Рисуем снаряды отдельно
                foreach (var projectile in _projectiles)
                {
                    var render = projectile.GetComponent<RenderComponent>();
                    if (render != null)
                    {
                        render.Draw(graphics);
                    }
                }
                
                Particles.Draw(graphics, Camera);  // Рисуем частицы
                UI.Draw(graphics);  // Рисуем интерфейс поверх всего
            }
        }

        public void TogglePause()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                var pauseMenu = UI.GetPauseMenu();
                if (pauseMenu != null) pauseMenu.IsActive = true;
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                var pauseMenu = UI.GetPauseMenu();
                if (pauseMenu != null) pauseMenu.IsActive = false;
            }
        }

        public void SaveGame()
        {
            var player = CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                Player.Position = player.Position;
            }
        }

        public void ExitGame()
        {
            State = GameState.GameOver;
            Music.StopMusic(); // Останавливаем музыку при выходе
            mainForm.Close();
        }

        public void SaveGameToFile(string filename)
        {
            Directory.CreateDirectory("saves");
            var player = CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                Player.Position = player.Position;
            }
            var saveData = new FullSaveData
            {
                Player = Player.GetSaveData(),
                Maze = CurrentScene.GetMazeSaveData(),
            };
            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine("saves", filename), json);
        }

        public void LoadGameFromFile(string filename)
        {
            // Добавляем расширение .json если его нет
            if (!filename.EndsWith(".json"))
            {
                filename += ".json";
            }
            
            string path = Path.Combine("saves", filename);
            LogThrottler.Log($"Пытаемся загрузить файл: {path}", "game_engine");
            
            if (!File.Exists(path))
            {
                LogThrottler.Log($"Файл не найден: {path}", "game_engine");
                return;
            }
            
            try
            {
                string json = File.ReadAllText(path);
                var saveData = JsonSerializer.Deserialize<FullSaveData>(json);
                if (saveData != null)
                {
                    LogThrottler.Log("Данные сохранения загружены успешно", "game_engine");
                    Player.LoadFromSaveData(saveData.Player);
                    CurrentScene.LoadMazeSaveData(saveData.Maze);
                    var player = CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
                    if (player != null)
                    {
                        player.Position = Player.Position;
                        LogThrottler.Log($"Позиция игрока установлена: {Player.Position.X}, {Player.Position.Y}", "game_engine");
                    }
                    else
                    {
                        LogThrottler.Log("Игрок не найден на сцене", "game_engine");
                    }
                }
                else
                {
                    LogThrottler.Log("Ошибка десериализации данных сохранения", "game_engine");
                }
            }
            catch (Exception ex)
            {
                LogThrottler.Log($"Ошибка загрузки сохранения: {ex.Message}", "game_engine");
            }
        }

        // Запускаем тряску экрана при получении урона
        public void TriggerScreenShake()
        {
            Camera.Shake(10f, 0.3f); // Интенсивность 10 пикселей, длительность 0.3 секунды
        }

        public class FullSaveData
        {
            public Player.PlayerData Player { get; set; }
            public Scene.MazeSaveData Maze { get; set; } // теперь MazeLayout это bool[][]
        }
    }

    // Возможные состояния игры
    public enum GameState
    {
        MainMenu,    // Главное меню
        Playing,     // Идёт игра
        Paused,      // Игра на паузе
        GameOver     // Игра окончена
    }
}