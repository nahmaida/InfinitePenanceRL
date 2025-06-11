using System;
using System.Drawing;
using System.Collections.Generic;

namespace InfinitePenanceRL
{
    public class MazeGenerator
    {
        private bool[,] _maze;
        private Random _random = new Random();
        private Point _startPosition;

        // Генерация лабиринта с помощью алгоритма поиска в глубину
        public bool[,] GenerateMaze(int width, int height)
        {
            _maze = new bool[width, height];
            _startPosition = new Point(1, 1); // Начальная позиция

            // Заполняем лабиринт стенами
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _maze[x, y] = false;

            // Начинаем с начальной точки
            CarvePassages(_startPosition.X, _startPosition.Y);

            return _maze;
        }

        private void CarvePassages(int x, int y)
        {
            // Отмечаем текущую клетку как проход
            _maze[x, y] = true;

            // Направления для прохода: вверх, вправо, вниз, влево
            var directions = new[] { new Point(0, -2), new Point(2, 0), new Point(0, 2), new Point(-2, 0) };
            Shuffle(directions);

            // Проверяем каждое направление
            foreach (var dir in directions)
            {
                int nextX = x + dir.X;
                int nextY = y + dir.Y;

                if (IsInBounds(nextX, nextY) && !_maze[nextX, nextY])
                {
                    // Прорубаем проход между текущей и следующей клетками
                    _maze[x + dir.X / 2, y + dir.Y / 2] = true;
                    CarvePassages(nextX, nextY);
                }
            }
        }

        private void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        private bool IsInBounds(int x, int y)
        {
            return x > 0 && x < _maze.GetLength(0) - 1 && y > 0 && y < _maze.GetLength(1) - 1;
        }

        public Point GetStartPosition() => _startPosition;

        public bool IsWalkable(bool[,] maze, int x, int y)
        {
            if (x < 0 || y < 0 || x >= maze.GetLength(0) || y >= maze.GetLength(1))
                return false;
            return maze[x, y];
        }
    }
} 