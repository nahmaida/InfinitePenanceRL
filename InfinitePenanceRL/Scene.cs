using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InfinitePenanceRL
{
    // Основная сцена игры
    public class Scene
    {
        public List<Entity> Entities { get; } = new List<Entity>();
        private readonly GameEngine _game;
        private TileType[,] _mazeLayout;
        private readonly MazeGenerator _mazeGenerator;
        public const int CellSize = 96;  // Размер одной клетки лабиринта в пикселях
        public const int WallSize = 48;  // Размер куска стены (половина клетки)

        public Scene(GameEngine game)
        {
            _game = game;
            _mazeGenerator = new MazeGenerator();
        }

        // Создаём новый лабиринт заданного размера
        public void GenerateMaze(int width = 15, int height = 15)
        {
            _mazeLayout = _mazeGenerator.GenerateMaze(width, height);
            CreateMazeEntities();
        }

        // Проверяем, есть ли стена в данной точке
        private bool IsWall(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _mazeLayout.GetLength(0) || y >= _mazeLayout.GetLength(1))
                return true; // За пределами карты всё - стены
            return _mazeLayout[x, y] == TileType.Wall;
        }

        // Проверяем, можно ли тут ходить
        private bool IsPath(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _mazeLayout.GetLength(0) || y >= _mazeLayout.GetLength(1))
                return false; // За пределами карты ходить нельзя
            return _mazeLayout[x, y] == TileType.Floor || _mazeLayout[x, y] == TileType.OpenDoor;
        }

        // Определяем тип стены на основе соседних клеток
        // isTopHalf и isLeftHalf определяют, какую четверть клетки мы проверяем
        private WallType DetermineWallType(int x, int y, bool isTopHalf, bool isLeftHalf)
        {
            // Проверяем наличие стен с каждой стороны
            bool hasWallN = y > 0 && _mazeLayout[x, y - 1] == TileType.Wall;
            bool hasWallS = y < _mazeLayout.GetLength(1) - 1 && _mazeLayout[x, y + 1] == TileType.Wall;
            bool hasWallW = x > 0 && _mazeLayout[x - 1, y] == TileType.Wall;
            bool hasWallE = x < _mazeLayout.GetLength(0) - 1 && _mazeLayout[x + 1, y] == TileType.Wall;

            // Проверяем наличие проходов с каждой стороны (включая двери)
            bool hasPathN = y > 0 && (_mazeLayout[x, y - 1] == TileType.Floor || _mazeLayout[x, y - 1] == TileType.OpenDoor || _mazeLayout[x, y - 1] == TileType.ClosedDoor);
            bool hasPathS = y < _mazeLayout.GetLength(1) - 1 && (_mazeLayout[x, y + 1] == TileType.Floor || _mazeLayout[x, y + 1] == TileType.OpenDoor || _mazeLayout[x, y + 1] == TileType.ClosedDoor);
            bool hasPathW = x > 0 && (_mazeLayout[x - 1, y] == TileType.Floor || _mazeLayout[x - 1, y] == TileType.OpenDoor || _mazeLayout[x - 1, y] == TileType.ClosedDoor);
            bool hasPathE = x < _mazeLayout.GetLength(0) - 1 && (_mazeLayout[x + 1, y] == TileType.Floor || _mazeLayout[x + 1, y] == TileType.OpenDoor || _mazeLayout[x + 1, y] == TileType.ClosedDoor);

            // Проверяем диагональные проходы (включая двери)
            bool hasPathNW = x > 0 && y > 0 && (_mazeLayout[x - 1, y - 1] == TileType.Floor || _mazeLayout[x - 1, y - 1] == TileType.OpenDoor || _mazeLayout[x - 1, y - 1] == TileType.ClosedDoor);
            bool hasPathNE = x < _mazeLayout.GetLength(0) - 1 && y > 0 && (_mazeLayout[x + 1, y - 1] == TileType.Floor || _mazeLayout[x + 1, y - 1] == TileType.OpenDoor || _mazeLayout[x + 1, y - 1] == TileType.ClosedDoor);
            bool hasPathSW = x > 0 && y < _mazeLayout.GetLength(1) - 1 && (_mazeLayout[x - 1, y + 1] == TileType.Floor || _mazeLayout[x - 1, y + 1] == TileType.OpenDoor || _mazeLayout[x - 1, y + 1] == TileType.ClosedDoor);
            bool hasPathSE = x < _mazeLayout.GetLength(0) - 1 && y < _mazeLayout.GetLength(1) - 1 && (_mazeLayout[x + 1, y + 1] == TileType.Floor || _mazeLayout[x + 1, y + 1] == TileType.OpenDoor || _mazeLayout[x + 1, y + 1] == TileType.ClosedDoor);

            if (isTopHalf)
            {
                if (isLeftHalf)
                {
                    // Верхний левый угол
                    if (hasWallN && hasWallW && hasPathNW)
                        return WallType.DiagonalTL;
                    if (hasPathN && hasPathW)
                        return WallType.TopLeft;
                    if (hasPathN && !hasPathW)
                        return WallType.Top;
                    if (!hasPathN && hasPathW)
                        return WallType.Left;
                    return WallType.Middle;
                }
                else
                {
                    // Верхний правый угол
                    if (hasWallN && hasWallE && hasPathNE)
                        return WallType.DiagonalTR;
                    if (hasPathN && hasPathE)
                        return WallType.TopRight;
                    if (hasPathN && !hasPathE)
                        return WallType.Top;
                    if (!hasPathN && hasPathE)
                        return WallType.Right;
                    return WallType.Middle;
                }
            }
            else
            {
                if (isLeftHalf)
                {
                    // Нижний левый угол
                    if (hasWallS && hasWallW && hasPathSW)
                        return WallType.DiagonalBL;
                    if (hasPathS && hasPathW)
                        return WallType.BottomLeft;
                    if (hasPathS && !hasPathW)
                        return WallType.Bottom;
                    if (!hasPathS && hasPathW)
                        return WallType.Left;
                    return WallType.Middle;
                }
                else
                {
                    // Нижний правый угол
                    if (hasWallS && hasWallE && hasPathSE)
                        return WallType.DiagonalBR;
                    if (hasPathS && hasPathE)
                        return WallType.BottomRight;
                    if (hasPathS && !hasPathE)
                        return WallType.Bottom;
                    if (!hasPathS && hasPathE)
                        return WallType.Right;
                    return WallType.Middle;
                }
            }
        }

        // Создаём все сущности для лабиринта: стены, двери и игрока
        private void CreateMazeEntities()
        {
            Entities.Clear();

            // Лепим стены лабиринта
            for (int x = 0; x < _mazeLayout.GetLength(0); x++)
            {
                for (int y = 0; y < _mazeLayout.GetLength(1); y++)
                {
                    var tileType = _mazeLayout[x, y];
                    
                    if (tileType == TileType.Wall)
                    {
                        // Каждая клетка-стена состоит из 4 кусочков (2x2)
                        for (int quadX = 0; quadX < 2; quadX++)
                        {
                            for (int quadY = 0; quadY < 2; quadY++)
                            {
                                var wallType = DetermineWallType(x, y, quadY == 0, quadX == 0);
                                var wall = EntityFactory.CreateWall(_game, wallType);
                                wall.Position = new Vector2(
                                    x * CellSize + quadX * WallSize,
                                    y * CellSize + quadY * WallSize
                                );
                                wall.GetComponent<RenderComponent>().Size = new Size(WallSize, WallSize);
                                AddEntity(wall);
                            }
                        }
                    }
                    else if (tileType == TileType.ClosedDoor || tileType == TileType.OpenDoor)
                    {
                        // Проверяем горизонтальный проход
                        bool isOpen = tileType == TileType.OpenDoor;
                        if (x + 1 < _mazeLayout.GetLength(0) && _mazeLayout[x + 1, y] == tileType)
                        {
                            // Горизонтальный проход шириной 2 клетки
                            var door1 = EntityFactory.CreateDoor(_game, isOpen);
                            door1.Position = new Vector2(x * CellSize, y * CellSize);
                            AddEntity(door1);
                            var door2 = EntityFactory.CreateDoor(_game, isOpen);
                            door2.Position = new Vector2((x + 1) * CellSize, y * CellSize);
                            AddEntity(door2);
                            _mazeLayout[x + 1, y] = TileType.Floor; // чтобы не спавнить дверь повторно
                            x++; // пропускаем вторую клетку
                        }
                        else if (y + 1 < _mazeLayout.GetLength(1) && _mazeLayout[x, y + 1] == tileType)
                        {
                            // Вертикальный проход шириной 2 клетки
                            var door1 = EntityFactory.CreateDoor(_game, isOpen);
                            door1.Position = new Vector2(x * CellSize, y * CellSize);
                            AddEntity(door1);
                            var door2 = EntityFactory.CreateDoor(_game, isOpen);
                            door2.Position = new Vector2(x * CellSize, (y + 1) * CellSize);
                            AddEntity(door2);
                            // y++ не нужен, внешний цикл
                        }
                        else
                        {
                            // Одиночная дверь (на всякий случай)
                            var door = EntityFactory.CreateDoor(_game, isOpen);
                            door.Position = new Vector2(x * CellSize, y * CellSize);
                            AddEntity(door);
                        }
                    }
                }
            }

            // Создаём игрока в стартовой точке
            Point startPos = _mazeGenerator.GetStartPosition();
            var player = EntityFactory.CreatePlayer(_game);
            
            // Используем сохранённую позицию игрока, если она есть
            /*
            if (Player.Position.X > 0 && Player.Position.Y > 0)
            {
                player.Position = Player.Position;
            }
            else
            */
            {
                // Вычисляем центр клетки с учётом размера игрока
                var playerRender = player.GetComponent<RenderComponent>();
                float playerWidth = playerRender.Size.Width;
                float playerHeight = playerRender.Size.Height;
                
                // Добавляем небольшой отступ от стен
                float offsetX = CellSize * 0.1f; // Сдвигаем на 10% клетки вправо
                float offsetY = CellSize * 0.1f; // Сдвигаем на 10% клетки вниз
                
                player.Position = new Vector2(
                    startPos.X * CellSize + (CellSize - playerWidth) / 2 + offsetX,  // Центрируем + отступ
                    startPos.Y * CellSize + (CellSize - playerHeight) / 2 + offsetY  // Центрируем + отступ
                );
            }
            
            AddEntity(player);

            // Спавним врагов в случайных проходах
            SpawnEnemies();
        }

        // Добавляем новую сущность на сцену
        public void AddEntity(Entity entity)
        {
            entity.Game = _game;
            Entities.Add(entity);
        }

        // Проверяем, можно ли ходить по указанной клетке
        public bool IsWalkable(int x, int y)
        {
            return _mazeGenerator.IsWalkable(_mazeLayout, x, y);
        }

        // Удаляем все объекты, помеченные для удаления
        public void CleanupMarkedEntities()
        {
            var entitiesToRemove = Entities.Where(e => e.IsMarkedForDeletion).ToList();
            foreach (var entity in entitiesToRemove)
            {
                Entities.Remove(entity);
                LogThrottler.Log($"Removed entity at position {entity.Position.X}, {entity.Position.Y}", "entity_cleanup");
            }
        }

        // Спавним врагов в случайных проходах лабиринта
        private void SpawnEnemies()
        {
            var random = new System.Random();
            var walkableCells = new List<Point>();

            // Находим все проходимые клетки
            for (int x = 0; x < _mazeLayout.GetLength(0); x++)
            {
                for (int y = 0; y < _mazeLayout.GetLength(1); y++)
                {
                    if (_mazeLayout[x, y] == TileType.Floor || _mazeLayout[x, y] == TileType.OpenDoor) // Если клетка проходима
                    {
                        walkableCells.Add(new Point(x, y));
                    }
                }
            }

            // Спавним врагов (примерно 1 враг на 10 проходимых клеток - увеличили частоту)
            int enemyCount = walkableCells.Count / 10;
            enemyCount = Math.Max(1, Math.Min(enemyCount, 15)); // Минимум 1, максимум 15 врагов

            for (int i = 0; i < enemyCount; i++)
            {
                if (walkableCells.Count == 0) break;

                // Выбираем случайную клетку
                int randomIndex = random.Next(walkableCells.Count);
                var cellPos = walkableCells[randomIndex];
                walkableCells.RemoveAt(randomIndex); // Убираем из списка, чтобы не спавнить дважды в одном месте

                // Создаём врага
                var enemy = EntityFactory.CreateEnemy(_game);
                
                // Размещаем врага в центре клетки с небольшим случайным смещением
                float offsetX = (random.NextSingle() - 0.5f) * CellSize * 0.3f; // Случайное смещение до 30% размера клетки
                float offsetY = (random.NextSingle() - 0.5f) * CellSize * 0.3f;
                
                enemy.Position = new Vector2(
                    cellPos.X * CellSize + CellSize / 2 + offsetX,
                    cellPos.Y * CellSize + CellSize / 2 + offsetY
                );

                AddEntity(enemy);
                LogThrottler.Log($"Spawned enemy at {enemy.Position.X}, {enemy.Position.Y}", "enemy_spawn");
            }
        }

        public class MazeSaveData
        {
            public TileType[][] MazeLayout { get; set; } // теперь это массив массивов TileType
        }

        // Преобразуем TileType[,] в TileType[][]
        private static TileType[][] ToJaggedArray(TileType[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            var jagged = new TileType[rows][];
            for (int i = 0; i < rows; i++)
            {
                jagged[i] = new TileType[cols];
                for (int j = 0; j < cols; j++)
                    jagged[i][j] = array[i, j];
            }
            return jagged;
        }

        // Преобразуем TileType[][] обратно в TileType[,]
        private static TileType[,] To2DArray(TileType[][] jagged)
        {
            int rows = jagged.Length;
            int cols = jagged[0].Length;
            var array = new TileType[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    array[i, j] = jagged[i][j];
            return array;
        }

        public MazeSaveData GetMazeSaveData()
        {
            return new MazeSaveData { MazeLayout = ToJaggedArray(_mazeLayout) };
        }

        public void LoadMazeSaveData(MazeSaveData data)
        {
            if (data == null || data.MazeLayout == null) return;
            _mazeLayout = To2DArray(data.MazeLayout);
            CreateMazeEntities();
        }
    }
}
