using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    // Класс для движка игры (рендеринг и прочее)
    public class GameEngine
    {
        public GameState State { get; private set; } = GameState.Playing;
        public InputManager Input { get; } = new InputManager();
        public Scene CurrentScene { get; private set; }
        public RenderSystem RenderSystem { get; } = new RenderSystem();
        public UIManager UI { get; private set; }
        public SpriteManager Sprites { get; } = new SpriteManager();

        public Camera Camera { get; } = new Camera();
        public PhysicsSystem Physics { get; } = new PhysicsSystem();
        public Size WorldSize { get; set; } = new Size(4000, 4000); // Размер мира

        public void Initialize(Size initialViewportSize)
        {
            CurrentScene = new Scene(this);
            Camera.ViewportSize = initialViewportSize;
            UI = new UIManager(this);
            Sprites.LoadSpritesheets();

            var player = EntityFactory.CreatePlayer(this);
            CurrentScene.AddEntity(player);

            // Тестовые стены
            CurrentScene.AddEntity(EntityFactory.CreateWall(this, 300, 200, 32, 64)); // 2x4 tiles
            CurrentScene.AddEntity(EntityFactory.CreateWall(this, 500, 400, 64, 32)); // 4x2 tiles
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

            // Камера следует за игроком
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