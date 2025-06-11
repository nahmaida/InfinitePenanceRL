namespace InfinitePenanceRL
{
    public enum WallType
    {
        TopLeft,    // 0,0
        Top,        // 1,0
        TopRight,   // 2,0
        Left,       // 0,1
        Middle,     // 1,1
        Right,      // 2,1
        BottomLeft, // 0,2
        Bottom,     // 1,2
        BottomRight,// 2,2
        DiagonalBR, // 5,1 
        DiagonalBL  // 6,1
    }

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
                Size = new Size(16 * 2, 16 * 2),
                SpriteName = "player",
                Scale = 2.0f
            };

            var collider = new ColliderComponent();
            var playerTag = new PlayerTag();

            player.AddComponent(movement);
            player.AddComponent(render);
            player.AddComponent(collider);
            player.AddComponent(playerTag);

            return player;
        }

        public static Entity CreateWall(GameEngine game, WallType wallType)
        {
            var wall = new Entity { Game = game };

            var render = new RenderComponent
            {
                Color = Color.Gray,
                Size = new Size(Scene.WallSize, Scene.WallSize),
                SpriteName = GetWallSpriteName(wallType),
                Scale = 3.0f
            };

            var collider = new ColliderComponent();

            wall.AddComponent(render);
            wall.AddComponent(collider);

            return wall;
        }

        private static string GetWallSpriteName(WallType wallType)
        {
            return wallType switch
            {
                WallType.TopLeft => "wall_top_left",
                WallType.Top => "wall_top",
                WallType.TopRight => "wall_top_right",
                WallType.Left => "wall_left",
                WallType.Middle => "wall_middle",
                WallType.Right => "wall_right",
                WallType.BottomLeft => "wall_bottom_left",
                WallType.Bottom => "wall_bottom",
                WallType.BottomRight => "wall_bottom_right",
                WallType.DiagonalBR => "wall_diagonal_br",
                WallType.DiagonalBL => "wall_diagonal_bl",
                _ => "wall_middle"
            };
        }

        // Старый метод
        public static Entity CreateWall(GameEngine game, int x, int y, int width, int height)
        {
            var wall = CreateWall(game, WallType.Middle);
            wall.Position = new Vector2(x, y);
            wall.GetComponent<RenderComponent>().Size = new Size(width, height);
            return wall;
        }

        public static Entity CreateWall(GameEngine game)
        {
            return CreateWall(game, WallType.Middle);
        }
    }

    // Маркер для игрока
    public class PlayerTag : Component { }
}