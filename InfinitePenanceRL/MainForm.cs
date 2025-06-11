using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace InfinitePenanceRL
{
    public partial class MainForm : Form
    {
        private readonly GameEngine _engine;

        public MainForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);

            _engine = new GameEngine(this);
            
            // Отслеживание движения мыши
            this.MouseMove += MainForm_MouseMove;
            this.KeyPreview = true;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            var inventory = _engine.UI.GetComponent<InventoryComponent>();
            if (inventory != null)
            {
                inventory.UpdateMousePosition(e.Location);
                Invalidate(); // Перерисовываем форму для обновления подсказок
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _engine.Initialize(this.ClientSize);
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
                // Обновляем камеру при изменении размера окна
                _engine.Camera = new Camera(this.ClientSize);

                // Центрируем на игроке
                var player = _engine.CurrentScene?.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
                if (player != null)
                {
                    _engine.Camera.CenterOn(player.Position, _engine.WorldSize);
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _engine.Input.KeyDown(e.KeyCode);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _engine.Input.KeyUp(e.KeyCode);
            e.Handled = true;
        }
    }
}