namespace InfinitePenanceRL
{
    public static class EntityFactory
    {
        public static Entity CreatePlayer(GameEngine game)
        {
            var player = new Entity
            {
                Position = new Vector2(100, 100),
                Game = game
            };

            var movement = new MovementComponent { Speed = 5f };
            var render = new RenderComponent
            {
                Color = Color.Blue,
                Size = new Size(32, 32)
            };
            var collider = new ColliderComponent();
            var playerTag = new PlayerTag();

            player.Components.Add(movement);
            player.Components.Add(render);
            player.Components.Add(collider);
            player.Components.Add(playerTag);

            movement.Owner = player;
            render.Owner = player;
            collider.Owner = player;
            playerTag.Owner = player;

            return player;
        }

        public static Entity CreateWall(GameEngine game, int x, int y, int width, int height)
        {
            var wall = new Entity
            {
                Position = new Vector2(x, y),
                Game = game
            };

            wall.Components.Add(new RenderComponent
            {
                Color = Color.Gray,
                Size = new Size(width, height)
            });
            wall.Components.Add(new ColliderComponent());

            return wall;
        }
    }

    // Маркер для игрока
    public class PlayerTag : Component { }
}