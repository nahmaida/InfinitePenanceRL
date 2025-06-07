namespace InfinitePenanceRL
{
    public static class EntityFactory
    {
        public static Entity CreatePlayer(GameEngine game)
        {
            Console.WriteLine("Creating player...");
            var player = new Entity
            {
                Position = new Vector2(100, 100),
                Game = game
            };

            var movement = new MovementComponent { Speed = 5f };
            var render = new RenderComponent
            {
                Color = Color.Blue,
                Size = new Size(16 * 2, 16 * 2),
                SpriteName = "player",
                Scale = 2.0f
            };
            Console.WriteLine($"Created player sprite: {render.SpriteName}, size: {render.Size}");

            var collider = new ColliderComponent();
            var playerTag = new PlayerTag();

            player.AddComponent(movement);
            player.AddComponent(render);
            player.AddComponent(collider);
            player.AddComponent(playerTag);

            return player;
        }

        public static Entity CreateWall(GameEngine game, int x, int y, int width, int height)
        {
            Console.WriteLine($"Creating wall: position [{x},{y}], size [{width}x{height}]");
            var wall = new Entity
            {
                Position = new Vector2(x, y),
                Game = game
            };

            var render = new RenderComponent
            {
                Color = Color.Gray,
                Size = new Size(width, height),
                SpriteName = "wall",
                Scale = 2.0f
            };
            Console.WriteLine($"Created wall sprite: {render.SpriteName}, size: {render.Size}");

            var collider = new ColliderComponent();

            wall.AddComponent(render);
            wall.AddComponent(collider);

            return wall;
        }
    }

    // Маркер для игрока
    public class PlayerTag : Component { }
}