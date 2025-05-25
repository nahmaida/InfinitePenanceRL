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

        public void Initialize()
        {
            CurrentScene = new Scene(this);
            CurrentScene.AddEntity(EntityFactory.CreatePlayer(this));
        }

        public void Update()
        {
            if (State != GameState.Playing) return;

            foreach (var entity in CurrentScene.Entities)
            {
                foreach (var component in entity.Components)
                {
                    component.Update();
                }
            }
        }

        public void Render(Graphics graphics)
        {
            graphics.Clear(Color.Black);
            if (CurrentScene != null)
            {
                RenderSystem.Render(graphics, CurrentScene.Entities);
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