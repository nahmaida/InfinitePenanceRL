using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace InfinitePenanceRL
{
    public enum TileType
    {
        Wall = 0,
        Floor = 1,
        ClosedDoor = 2,
        OpenDoor = 3
    }

    public class MazeGenerator
    {
        private TileType[,] _maze;
        private Random _random = new Random();
        private Point _startPosition;
        private List<Point> _doors = new List<Point>(); // список позиций дверей

        // константы для регионов
        private const int CELL_SOLID = 0;
        private const int CELL_MERGED = -1;
        private const int CELL_DOOR = -2;
        
        // параметры генерации
        private int _numRoomTries = 200;
        private int _extraConnectorChance = 20;
        private int _roomExtraSize = 0;
        private int _windingPercent = 0;
        
        // данные для генерации
        private int[,] _regions;
        private int _currentRegion = -1;
        private List<Rectangle> _rooms = new List<Rectangle>();

        // Генерация подземелья с комнатами и лабиринтами
        public TileType[,] GenerateMaze(int width, int height)
        {
            // убеждаемся что размеры нечетные
            if (width % 2 == 0) width++;
            if (height % 2 == 0) height++;
            
            _maze = new TileType[width, height];
            _regions = new int[width, height];
            _rooms.Clear();
            _doors.Clear();
            _currentRegion = -1;
            
            // заполняем стенами
            FillWalls();
            
            // добавляем комнаты
            AddRooms();
            
            // заполняем оставшееся пространство лабиринтами
            for (int y = 1; y < height; y += 2)
            {
                for (int x = 1; x < width; x += 2)
                {
                    Point pos = new Point(x, y);
                    if (!IsWall(pos)) continue;
                    GrowMaze(pos);
                }
            }
            
            // соединяем регионы
            ConnectRegions();
            
            // убираем тупики
            RemoveDeadEnds();
            
            // устанавливаем стартовую позицию в первой комнате
            if (_rooms.Count > 0)
            {
                var firstRoom = _rooms[0];
                _startPosition = new Point(
                    firstRoom.X + firstRoom.Width / 2,
                    firstRoom.Y + firstRoom.Height / 2
                );
            }
            else
            {
                _startPosition = new Point(1, 1);
            }

            return _maze;
        }

        private void FillWalls()
        {
            for (int x = 0; x < _maze.GetLength(0); x++)
            {
                for (int y = 0; y < _maze.GetLength(1); y++)
                {
                    _maze[x, y] = TileType.Wall; // стена
                    _regions[x, y] = -1; // инициализируем регионы как -1 (как null в Dart)
                }
            }
        }
        
        private void AddRooms()
        {
            for (int i = 0; i < _numRoomTries; i++)
            {
                // выбираем случайный размер комнаты
                int size = _random.Next(1, 3 + _roomExtraSize) * 2 + 1;
                int rectangularity = _random.Next(0, 1 + size / 2) * 2;
                int width = size;
                int height = size;
                
                if (_random.Next(2) == 0)
                {
                    width += rectangularity;
                }
                else
                {
                    height += rectangularity;
                }
                
                // проверяем что комната помещается в границы
                if (width >= _maze.GetLength(0) - 2 || height >= _maze.GetLength(1) - 2) continue;
                
                // размещаем комнаты в центральной половине как в оригинале
                int x = _random.Next((_maze.GetLength(0) - width) / 2) * 2 + 1;
                int y = _random.Next((_maze.GetLength(1) - height) / 2) * 2 + 1;
                
                Rectangle room = new Rectangle(x, y, width, height);
                
                // проверяем пересечения с существующими комнатами (как в оригинале)
                // distanceTo <= 0 означает что комнаты могут касаться но не пересекаться
                bool overlaps = false;
                foreach (var other in _rooms)
                {
                    // проверяем что комнаты не пересекаются (но могут касаться)
                    if (room.X < other.X + other.Width && room.X + room.Width > other.X &&
                        room.Y < other.Y + other.Height && room.Y + room.Height > other.Y)
                    {
                        overlaps = true;
                        break;
                    }
                }
                
                if (overlaps) continue;
                
                _rooms.Add(room);
                
                StartRegion();
                for (int rx = x; rx < x + width; rx++)
                {
                    for (int ry = y; ry < y + height; ry++)
                    {
                        Carve(new Point(rx, ry));
                    }
                }
            }
        }
        
        // алгоритм "растущего дерева" для генерации лабиринта
        private void GrowMaze(Point start)
        {
            List<Point> cells = new List<Point>();
            Point? lastDir = null;
            
            StartRegion();
            Carve(start);
            
            cells.Add(start);
            while (cells.Count > 0)
            {
                Point cell = cells[cells.Count - 1];
                
                // смотрим какие соседние клетки доступны
                List<Point> unmadeCells = new List<Point>();
                
                Point[] directions = { new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point(-1, 0) };
                foreach (Point dir in directions)
                {
                    if (CanCarve(cell, dir))
                        unmadeCells.Add(dir);
                }
                
                if (unmadeCells.Count > 0)
                {
                    // предпочитаем идти в том же направлении для извилистых проходов
                    Point dir;
                    if (lastDir.HasValue && unmadeCells.Contains(lastDir.Value) && _random.Next(100) > _windingPercent)
                    {
                        dir = lastDir.Value;
                    }
                    else
                    {
                        dir = unmadeCells[_random.Next(unmadeCells.Count)];
                    }
                    
                    Carve(new Point(cell.X + dir.X, cell.Y + dir.Y));
                    Carve(new Point(cell.X + dir.X * 2, cell.Y + dir.Y * 2));
                    
                    cells.Add(new Point(cell.X + dir.X * 2, cell.Y + dir.Y * 2));
                    lastDir = dir;
                }
                else
                {
                    // нет соседних невырезанных клеток
                    cells.RemoveAt(cells.Count - 1);
                    lastDir = null;
                }
            }
        }
        
        private void ConnectRegions()
        {
            // находим все тайлы которые могут соединить два или более региона
            Dictionary<Point, HashSet<int>> connectorRegions = new Dictionary<Point, HashSet<int>>();
            
            // используем bounds.inflate(-1) как в оригинале - избегаем краев
            for (int x = 1; x < _maze.GetLength(0) - 1; x++)
            {
                for (int y = 1; y < _maze.GetLength(1) - 1; y++)
                {
                    Point pos = new Point(x, y);
                    
                    // не может уже быть частью региона
                    if (!IsWall(pos)) continue;
                    
                    HashSet<int> regions = new HashSet<int>();
                    Point[] directions = { new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point(-1, 0) };
                    
                    foreach (Point dir in directions)
                    {
                        Point checkPos = new Point(pos.X + dir.X, pos.Y + dir.Y);
                        if (IsInBounds(checkPos))
                        {
                            int region = _regions[checkPos.X, checkPos.Y];
                            if (region != -1) regions.Add(region); // проверяем что регион не -1 (как null в Dart)
                        }
                    }
                    
                    if (regions.Count < 2) continue;
                    
                    connectorRegions[pos] = regions;
                }
            }
            
            List<Point> connectors = connectorRegions.Keys.ToList();
            
            // отслеживаем какие регионы были объединены
            Dictionary<int, int> merged = new Dictionary<int, int>();
            HashSet<int> openRegions = new HashSet<int>();
            
            for (int i = 0; i <= _currentRegion; i++)
            {
                merged[i] = i;
                openRegions.Add(i);
            }
            
            // продолжаем соединять регионы пока не останется один (как в оригинале)
            while (openRegions.Count > 1 && connectors.Count > 0)
            {
                Point connector = connectors[_random.Next(connectors.Count)];
                
                // вырезаем соединение
                AddJunction(connector);
                
                // объединяем соединенные регионы
                var regions = connectorRegions[connector].Select(r => merged[r]).ToList();
                int dest = regions[0];
                var sources = regions.Skip(1).ToList();
                
                // объединяем все затронутые регионы
                for (int i = 0; i <= _currentRegion; i++)
                {
                    if (sources.Contains(merged[i]))
                    {
                        merged[i] = dest;
                    }
                }
                
                // источники больше не используются
                foreach (int source in sources)
                {
                    openRegions.Remove(source);
                }
                
                // убираем ненужные коннекторы
                connectors.RemoveAll(pos =>
                {
                    // не позволяем коннекторы рядом друг с другом (как в оригинале)
                    // connector - pos < 2 в Dart означает Manhattan distance < 2
                    int distance = Math.Abs(connector.X - pos.X) + Math.Abs(connector.Y - pos.Y);
                    if (distance < 2) return true;
                    
                    // если коннектор больше не соединяет разные регионы
                    var posRegions = connectorRegions[pos].Select(r => merged[r]).ToHashSet();
                    
                    if (posRegions.Count > 1) return false;
                    
                    // этот коннектор не нужен, но иногда соединяем чтобы подземелье не было просто связанным
                    // rng.oneIn(extraConnectorChance) означает 1 из N шанс
                    if (_random.Next(_extraConnectorChance) == 0) AddJunction(pos);
                    
                    return true;
                });
            }
            
            // если остались несоединенные регионы, принудительно соединяем их
            if (openRegions.Count > 1)
            {
                // находим любые оставшиеся коннекторы которые соединяют разные регионы
                for (int x = 1; x < _maze.GetLength(0) - 1; x++)
                {
                    for (int y = 1; y < _maze.GetLength(1) - 1; y++)
                    {
                        Point pos = new Point(x, y);
                        if (!IsWall(pos)) continue;
                        
                        HashSet<int> regions = new HashSet<int>();
                        Point[] directions = { new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point(-1, 0) };
                        
                        foreach (Point dir in directions)
                        {
                            Point checkPos = new Point(pos.X + dir.X, pos.Y + dir.Y);
                            if (IsInBounds(checkPos))
                            {
                                int region = _regions[checkPos.X, checkPos.Y];
                                if (region != -1) regions.Add(region);
                            }
                        }
                        
                        // ищем коннекторы которые соединяют разные регионы
                        if (regions.Count >= 2)
                        {
                            AddJunction(pos);
                            break;
                        }
                    }
                }
            }
        }
        
        private void AddJunction(Point pos)
        {
            if (_random.Next(4) == 0)
            {
                // дверь или пол
                if (_random.Next(3) == 0)
                {
                    // открытая дверь
                    _maze[pos.X, pos.Y] = TileType.OpenDoor;
                    _doors.Add(pos);
                }
                else
                {
                    // пол
                    _maze[pos.X, pos.Y] = TileType.Floor;
                }
            }
            else
            {
                // закрытая дверь
                _maze[pos.X, pos.Y] = TileType.ClosedDoor;
                _doors.Add(pos);
            }
        }
        
        private void RemoveDeadEnds()
        {
            bool done = false;
            
            while (!done)
            {
                done = true;
                
                // используем bounds.inflate(-1) как в оригинале
                for (int x = 1; x < _maze.GetLength(0) - 1; x++)
                {
                    for (int y = 1; y < _maze.GetLength(1) - 1; y++)
                    {
                        Point pos = new Point(x, y);
                        if (IsWall(pos)) continue;
                        
                        // если у неё только один выход, это тупик
                        int exits = 0;
                        Point[] directions = { new Point(0, -1), new Point(1, 0), new Point(0, 1), new Point(-1, 0) };
                        
                        foreach (Point dir in directions)
                        {
                            Point checkPos = new Point(pos.X + dir.X, pos.Y + dir.Y);
                            if (IsInBounds(checkPos) && !IsWall(checkPos)) exits++;
                        }
                        
                        if (exits != 1) continue;
                        
                        done = false;
                        _maze[pos.X, pos.Y] = TileType.Wall; // замуровываем тупик
                    }
                }
            }
        }
        
        private bool CanCarve(Point pos, Point direction)
        {
            // должен заканчиваться в границах
            Point endPos = new Point(pos.X + direction.X * 3, pos.Y + direction.Y * 3);
            if (!IsInBounds(endPos)) return false;
            
            // назначение не должно быть открытым
            Point destPos = new Point(pos.X + direction.X * 2, pos.Y + direction.Y * 2);
            return IsWall(destPos);
        }
        
        private void StartRegion()
        {
            _currentRegion++;
        }
        
        private void Carve(Point pos)
        {
            _maze[pos.X, pos.Y] = TileType.Floor; // пол
            _regions[pos.X, pos.Y] = _currentRegion;
        }
        
        private bool IsWall(Point pos)
        {
            if (!IsInBounds(pos)) return true;
            return _maze[pos.X, pos.Y] == TileType.Wall;
        }
        
        private bool IsInBounds(Point pos)
        {
            return pos.X >= 0 && pos.X < _maze.GetLength(0) && pos.Y >= 0 && pos.Y < _maze.GetLength(1);
        }

        public Point GetStartPosition() => _startPosition;

        public bool IsWalkable(TileType[,] maze, int x, int y)
        {
            if (x < 0 || y < 0 || x >= maze.GetLength(0) || y >= maze.GetLength(1))
                return false;
            return maze[x, y] == TileType.Floor || maze[x, y] == TileType.OpenDoor;
        }

        public List<Point> GetDoors() => _doors;
    }
} 