using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MazeGame
{
    public partial class MazeForm : Form
    {
        private const int CellSize = 20;
        private const int MazeWidth = 50;
        private const int MazeHeight = 50;
        private bool[,] maze;
        private Point playerPosition;
        private Random random = new Random();
        public MazeForm()
        {
            InitializeComponent();
            SetupForm();
            GenerateMaze();
            PlacePlayer();
        }
        private void SetupForm()
        {
            this.Text = "Лабиринт";
            this.ClientSize = new Size(MazeWidth * CellSize, MazeHeight * CellSize);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
        }
        private void GenerateMaze()
        {
            maze = new bool[MazeWidth, MazeHeight];
            for (int x = 0; x < MazeWidth; x++)
                for (int y = 0; y < MazeHeight; y++)
                    maze[x, y] = false;
            for (int y = 1; y < MazeHeight - 1; y += 2)
            {
                for (int x = 1; x < MazeWidth - 1; x += 2)
                {
                    maze[x, y] = true;
                    if (x < MazeWidth - 2 && (y == MazeHeight - 2 || random.Next(2) == 0))
                    {
                        maze[x + 1, y] = true;
                    }
                    else if (y < MazeHeight - 2)
                    {
                        maze[x, y + 1] = true;
                    }
                }
            }
            maze[0, 1] = true;
        }
        private void PlacePlayer()
        {
            playerPosition = new Point(0, 1);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            Point newPosition = playerPosition;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    newPosition.Y--;
                    break;
                case Keys.Down:
                    newPosition.Y++;
                    break;
                case Keys.Left:
                    newPosition.X--;
                    break;
                case Keys.Right:
                    newPosition.X++;
                    break;
                default:
                    return;
            }
            if (newPosition.X >= 0 && newPosition.X < MazeWidth &&
                newPosition.Y >= 0 && newPosition.Y < MazeHeight &&
                maze[newPosition.X, newPosition.Y])
            {
                playerPosition = newPosition;
                this.Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            for (int x = 0; x < MazeWidth; x++)
            {
                for (int y = 0; y < MazeHeight; y++)
                {
                    if (maze[x, y])
                    {
                        g.FillRectangle(Brushes.White, x * CellSize, y * CellSize, CellSize, CellSize);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.Black, x * CellSize, y * CellSize, CellSize, CellSize);
                    }

                    g.DrawRectangle(Pens.Gray, x * CellSize, y * CellSize, CellSize, CellSize);
                }
            }
            g.FillEllipse(Brushes.Red,
                playerPosition.X * CellSize + CellSize / 4,
                playerPosition.Y * CellSize + CellSize / 4,
                CellSize / 2,
                CellSize / 2);
        }
    }
}
