using System;
using System.Drawing;
using System.Windows.Forms;

namespace InfinitePenanceRL
{
    public partial class MainForm : Form
    {
        private readonly GameEngine _engine;
        private System.Windows.Forms.Timer _gameTimer;

        public MainForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);

            _engine = new GameEngine();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _engine.Initialize(this.ClientSize);

            _gameTimer = new System.Windows.Forms.Timer
            {
                Interval = 16 // ~60 фпс
            };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            _engine.Update();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _engine.Render(e.Graphics);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_engine != null && _engine.Camera != null)
            {
                // Обновляем камеру когда меняем размер окна
                _engine.Camera.ViewportSize = this.ClientSize;

                // Центрируем на игроке
                var player = _engine.CurrentScene?.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
                if (player != null)
                {
                    _engine.Camera.CenterOn(player.Position, _engine.WorldSize);
                }
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            _engine.Input.KeyDown(e.KeyCode);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            _engine.Input.KeyUp(e.KeyCode);
        }
    }
}