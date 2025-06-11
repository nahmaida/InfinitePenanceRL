using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    // Основная сцена игры
    public class Scene
    {
        public List<Entity> Entities { get; } = new List<Entity>();
        private readonly GameEngine _game;
        private bool[,] _mazeLayout;
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
            return !_mazeLayout[x, y];
        }

        // Проверяем, можно ли тут ходить
        private bool IsPath(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _mazeLayout.GetLength(0) || y >= _mazeLayout.GetLength(1))
                return false; // За пределами карты ходить нельзя
            return _mazeLayout[x, y];
        }

        // Определяем тип стены на основе соседних клеток
        // isTopHalf и isLeftHalf определяют, какую четверть клетки мы проверяем
        private WallType DetermineWallType(int x, int y, bool isTopHalf, bool isLeftHalf)
        {
            // Проверяем наличие стен с каждой стороны
            bool hasWallN = y > 0 && !_mazeLayout[x, y - 1];
            bool hasWallS = y < _mazeLayout.GetLength(1) - 1 && !_mazeLayout[x, y + 1];
            bool hasWallW = x > 0 && !_mazeLayout[x - 1, y];
            bool hasWallE = x < _mazeLayout.GetLength(0) - 1 && !_mazeLayout[x + 1, y];

            // Проверяем наличие проходов с каждой стороны
            bool hasPathN = y > 0 && _mazeLayout[x, y - 1];
            bool hasPathS = y < _mazeLayout.GetLength(1) - 1 && _mazeLayout[x, y + 1];
            bool hasPathW = x > 0 && _mazeLayout[x - 1, y];
            bool hasPathE = x < _mazeLayout.GetLength(0) - 1 && _mazeLayout[x + 1, y];

            // Проверяем диагональные проходы
            bool hasPathNW = x > 0 && y > 0 && _mazeLayout[x - 1, y - 1];
            bool hasPathNE = x < _mazeLayout.GetLength(0) - 1 && y > 0 && _mazeLayout[x + 1, y - 1];
            bool hasPathSW = x > 0 && y < _mazeLayout.GetLength(1) - 1 && _mazeLayout[x - 1, y + 1];
            bool hasPathSE = x < _mazeLayout.GetLength(0) - 1 && y < _mazeLayout.GetLength(1) - 1 && _mazeLayout[x + 1, y + 1];

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

        // Создаём все сущности для лабиринта: стены и игрока
        private void CreateMazeEntities()
        {
            Entities.Clear();

            // Лепим стены лабиринта
            for (int x = 0; x < _mazeLayout.GetLength(0); x++)
            {
                for (int y = 0; y < _mazeLayout.GetLength(1); y++)
                {
                    if (!_mazeLayout[x, y])
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
                }
            }

            // Создаём игрока в стартовой точке
            Point startPos = _mazeGenerator.GetStartPosition();
            var player = EntityFactory.CreatePlayer(_game);
            
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
            AddEntity(player);
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
    }
}