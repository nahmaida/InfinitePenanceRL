using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace InfinitePenanceRL
{
    // Класс для движка игры (рендеринг и прочее)
    public class GameEngine
    {
        public GameState State { get; private set; } = GameState.Playing;
        public Scene CurrentScene { get; private set; }
        public RenderSystem RenderSystem { get; } = new RenderSystem();
        public InputManager Input { get; } = new InputManager();
        public UIManager UI { get; private set; }
        public SpriteManager Sprites { get; } = new SpriteManager();
        public Camera Camera { get; set; }
        public PhysicsSystem Physics { get; } = new PhysicsSystem();
        public Size WorldSize { get; set; }

        private System.Windows.Forms.Timer gameTimer;
        private MainForm mainForm;
        private const int TARGET_FPS = 60;
        private const int MAZE_WIDTH = 41; // 4000/96 округлено вниз
        private const int MAZE_HEIGHT = 41; // 4000/96 округлено вниз

        public GameEngine(MainForm form)
        {
            mainForm = form;
            Camera = new Camera(form.ClientSize);
            CurrentScene = new Scene(this);
            UI = new UIManager(this);

            InitializeGame();
            SetupGameTimer();
        }

        private void InitializeGame()
        {
            Initialize(mainForm.ClientSize);
        }

        private void SetupGameTimer()
        {
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000 / TARGET_FPS;
            gameTimer.Tick += (sender, e) =>
            {
                Update();
                mainForm.Invalidate();
            };
            gameTimer.Start();
        }

        public void Initialize(Size initialViewportSize)
        {
            CurrentScene = new Scene(this);
            Camera = new Camera(initialViewportSize);
            UI = new UIManager(this);
            Sprites.LoadSpritesheets();

            // Устанавливаем размер мира на основе размеров лабиринта
            WorldSize = new Size(MAZE_WIDTH * Scene.CellSize, MAZE_HEIGHT * Scene.CellSize);

            // Генерируем лабиринт
            CurrentScene.GenerateMaze(MAZE_WIDTH, MAZE_HEIGHT);
        }

        public void Update()
        {
            if (State != GameState.Playing) return;

            Physics.Update(CurrentScene);

            foreach (var entity in CurrentScene.Entities)
            {
                foreach (var component in entity.Components)
                {
                    component.Update();
                }
            }

            // Центрируем камеру на игроке
            var player = CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                Camera.CenterOn(player.Position, WorldSize);
            }
        }

        public void Render(Graphics graphics)
        {
            graphics.Clear(Color.Black);
            if (CurrentScene != null)
            {
                RenderSystem.Render(graphics, this, CurrentScene.Entities);
                UI.Draw(graphics);
            }
        }
    }

    // Состояния игры
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
}