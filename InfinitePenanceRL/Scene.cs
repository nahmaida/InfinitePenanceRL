using System.Collections.Generic;

namespace InfinitePenanceRL
{
    // Карта игры
    public class Scene
    {
        public List<Entity> Entities { get; } = new List<Entity>();
        private readonly GameEngine _game;

        public Scene(GameEngine game)
        {
            _game = game;
        }

        public void AddEntity(Entity entity)
        {
            entity.Game = _game;
            Entities.Add(entity);
        }
    }
}