using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    // Карта игры
    public class Scene
    {
        public List<Entity> Entities { get; } = new List<Entity>();
        private readonly GameEngine _game;
        private bool[,] _mazeLayout;
        private readonly MazeGenerator _mazeGenerator;
        public const int CellSize = 96;  // Размер клетки лабиринта
        public const int WallSize = 48;  // Размер стены (половина клетки)

        public Scene(GameEngine game)
        {
            _game = game;
            _mazeGenerator = new MazeGenerator();
        }

        public void GenerateMaze(int width = 15, int height = 15)
        {
            _mazeLayout = _mazeGenerator.GenerateMaze(width, height);
            CreateMazeEntities();
        }

        private bool IsWall(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _mazeLayout.GetLength(0) || y >= _mazeLayout.GetLength(1))
                return true;
            return !_mazeLayout[x, y];
        }

        private bool IsPath(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _mazeLayout.GetLength(0) || y >= _mazeLayout.GetLength(1))
                return false;
            return _mazeLayout[x, y];
        }

        private WallType DetermineWallType(int cellX, int cellY, bool isTopHalf, bool isLeftHalf)
        {
            // Проверяем все соседние клетки - true означает проход, false означает стену
            bool hasPathN = IsPath(cellX, cellY - 1);
            bool hasPathS = IsPath(cellX, cellY + 1);
            bool hasPathW = IsPath(cellX - 1, cellY);
            bool hasPathE = IsPath(cellX + 1, cellY);
            bool hasPathNW = IsPath(cellX - 1, cellY - 1);
            bool hasPathNE = IsPath(cellX + 1, cellY - 1);
            bool hasPathSW = IsPath(cellX - 1, cellY + 1);
            bool hasPathSE = IsPath(cellX + 1, cellY + 1);

            // Преобразуем в наличие стен
            bool hasWallN = !hasPathN;
            bool hasWallS = !hasPathS;
            bool hasWallW = !hasPathW;
            bool hasWallE = !hasPathE;

            if (isTopHalf)
            {
                if (isLeftHalf)
                {
                    // Верхний левый квадрант
                    // Сначала проверяем диагональ
                    if (hasPathSE && !hasPathS && !hasPathE && !hasPathN && !hasPathW)
                        return WallType.DiagonalBR;
                    
                    if (hasWallN && hasWallS && hasWallE && hasWallW)
                        return WallType.Middle;
                    if (hasPathN && hasPathW)
                        return WallType.TopLeft;
                    if (hasPathN && !hasPathW)
                        return WallType.Top;
                    if (!hasPathN && hasPathW)
                        return WallType.Left;
                    return WallType.TopLeft;
                }
                else
                {
                    // Верхний правый квадрант
                    // Сначала проверяем диагональ
                    if (hasPathSW && !hasPathS && !hasPathW && !hasPathN && !hasPathE)
                        return WallType.DiagonalBL;
                    
                    if (hasWallN && hasWallS && hasWallE && hasWallW)
                        return WallType.Middle;
                    if (hasPathN && hasPathE)
                        return WallType.TopRight;
                    if (hasPathN && !hasPathE)
                        return WallType.Top;
                    if (!hasPathN && hasPathE)
                        return WallType.Right;
                    return WallType.TopRight;
                }
            }
            else
            {
                if (isLeftHalf)
                {
                    // Нижний левый квадрант
                    // Сначала проверяем диагональ
                    if (hasPathNE && !hasPathN && !hasPathE && !hasPathS && !hasPathW)
                        return WallType.DiagonalBR;
                    
                    if (hasWallN && hasWallS && hasWallE && hasWallW)
                        return WallType.Middle;
                    if (hasPathS && hasPathW)
                        return WallType.BottomLeft;
                    if (hasPathS && !hasPathW)
                        return WallType.Bottom;
                    if (!hasPathS && hasPathW)
                        return WallType.Left;
                    return WallType.BottomLeft;
                }
                else
                {
                    // Нижний правый квадрант
                    // Сначала проверяем диагональ
                    if (hasPathNW && !hasPathN && !hasPathW && !hasPathS && !hasPathE)
                        return WallType.DiagonalBL;
                    
                    if (hasWallN && hasWallS && hasWallE && hasWallW)
                        return WallType.Middle;
                    if (hasPathS && hasPathE)
                        return WallType.BottomRight;
                    if (hasPathS && !hasPathE)
                        return WallType.Bottom;
                    if (!hasPathS && hasPathE)
                        return WallType.Right;
                    return WallType.BottomRight;
                }
            }
        }

        private void CreateMazeEntities()
        {
            Entities.Clear();

            // Создаем стены
            for (int x = 0; x < _mazeLayout.GetLength(0); x++)
            {
                for (int y = 0; y < _mazeLayout.GetLength(1); y++)
                {
                    if (!_mazeLayout[x, y])
                    {
                        // Создаем 2x2 стены для этой клетки
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

            // Создаем игрока в начальной позиции
            Point startPos = _mazeGenerator.GetStartPosition();
            var player = EntityFactory.CreatePlayer(_game);
            player.Position = new Vector2(
                startPos.X * CellSize + (CellSize - 32) / 2,
                startPos.Y * CellSize + (CellSize - 32) / 2
            );
            AddEntity(player);
        }

        public void AddEntity(Entity entity)
        {
            entity.Game = _game;
            Entities.Add(entity);
        }

        public bool IsWalkable(int x, int y)
        {
            return _mazeGenerator.IsWalkable(_mazeLayout, x, y);
        }
    }
}