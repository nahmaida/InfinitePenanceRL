using System.Drawing;

namespace InfinitePenanceRL
{
    public enum WallType
    {
        TopLeft,    // Верхний левый угол (0,0)
        Top,        // Верхняя стена (1,0)
        TopRight,   // Верхний правый угол (2,0)
        Left,       // Левая стена (0,1)
        Middle,     // Центр стены (1,1)
        Right,      // Правая стена (2,1)
        BottomLeft, // Нижний левый угол (0,2)
        Bottom,     // Нижняя стена (1,2)
        BottomRight,// Нижний правый угол (2,2)
        DiagonalTL, // Диагональ сверху-слева (5,1)
        DiagonalTR, // Диагональ сверху-справа (6,1)
        DiagonalBL, // Диагональ снизу-слева (5,2)
        DiagonalBR  // Диагональ снизу-справа (6,2)
    }

    // Фабрика для создания всяких игровых объектов
    public static class EntityFactory
    {
        public static Entity CreatePlayer(GameEngine game)
        {
            LogThrottler.Log("Создаем игрока", "player_creation");
            var player = new Entity
            {
                Position = new Vector2(100, 100),
                Game = game
            };

            // Добавляем все нужные компоненты
            var movement = new MovementComponent { Speed = Player.Speed };
            var render = new RenderComponent
            {
                Color = Color.Blue,
                Size = new Size(32, 32),  // 2x от размера спрайта (16px)
                SpriteName = "player",
                Scale = 2.0f
            };

            var collider = new ColliderComponent();
            var playerTag = new PlayerTag();
            var animation = new AnimationComponent();
            var attack = new AttackComponent();

            player.AddComponent(movement);
            player.AddComponent(render);
            player.AddComponent(collider);
            player.AddComponent(playerTag);
            player.AddComponent(animation);
            player.AddComponent(attack);

            LogThrottler.Log("Игрок создан со всеми компонентами", "player_creation");
            return player;
        }

        public static Entity CreateWall(GameEngine game, WallType wallType)
        {
            var wall = new Entity { Game = game };

            // Настраиваем рендер стены
            var render = new RenderComponent
            {
                Color = Color.Gray,
                Size = new Size(Scene.WallSize, Scene.WallSize),
                SpriteName = wallType switch
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
                    WallType.DiagonalTL => "wall_diagonal_tl",
                    WallType.DiagonalTR => "wall_diagonal_tr",
                    WallType.DiagonalBL => "wall_diagonal_bl",
                    WallType.DiagonalBR => "wall_diagonal_br",
                    _ => "wall_middle"
                },
                Scale = 3.0f
            };

            // Добавляем коллайдер чтобы нельзя было пройти сквозь стену
            var collider = new ColliderComponent();

            wall.AddComponent(render);
            wall.AddComponent(collider);

            return wall;
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

        public static Entity CreateEnemy(GameEngine game)
        {
            LogThrottler.Log("Создаем врага", "enemy_creation");
            var enemy = new Entity
            {
                Position = new Vector2(200, 200), // Временная позиция, будет изменена при спавне
                Game = game
            };

            // Добавляем компоненты врага
            var enemyComponent = new EnemyComponent
            {
                Health = 50f,
                MaxHealth = 50f,
                Attack = 15f,
                Speed = 3f
            };

            var render = new RenderComponent
            {
                Color = Color.White,
                Size = new Size(16, 16),
                SpriteName = "enemy", // Используем спрайт
                Scale = 3.0f
            };

            var collider = new ColliderComponent();
            var enemyTag = new EnemyTag();
            var animation = new AnimationComponent(); // Добавляем анимацию

            enemy.AddComponent(enemyComponent);
            enemy.AddComponent(render);
            enemy.AddComponent(collider);
            enemy.AddComponent(enemyTag);
            enemy.AddComponent(animation); // Добавляем компонент анимации

            LogThrottler.Log("Враг создан со всеми компонентами", "enemy_creation");
            return enemy;
        }
    }

    // Маркер для игрока
    public class PlayerTag : Component { }
}