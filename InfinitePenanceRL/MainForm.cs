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

            LogThrottler.Log("Тест логирования", "test");

            _engine = new GameEngine(this);
            SubscribePauseMenuEvents();
            
            // Отслеживание движения мыши
            this.MouseMove += MainForm_MouseMove;
            this.MouseDown += MainForm_MouseDown;
            this.MouseUp += MainForm_MouseUp;
            this.KeyPreview = true;
            
            // Сохраняем данные игрока при закрытии игры
            this.FormClosing += MainForm_FormClosing;
        }

        private void SubscribePauseMenuEvents()
        {
            var pauseMenu = _engine.UI.GetPauseMenu();
            if (pauseMenu != null)
            {
                // Просто подписываемся на события, не трогаем null
                pauseMenu.OnButtonPressed += (action) =>
                {
                    if (action == PauseMenuAction.Resume) _engine.TogglePause();
                    else if (action == PauseMenuAction.Exit) _engine.ExitGame();
                    Invalidate();
                };
                pauseMenu.SaveRequested += (filename) =>
                {
                    _engine.SaveGameToFile(filename);
                    Invalidate();
                };
                pauseMenu.LoadRequested += (filename) =>
                {
                    _engine.LoadGameFromFile(filename);
                    Invalidate();
                };
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            // проверяем систему прокачки
            var levelingUI = _engine.UI.GetLevelingUI();
            if (levelingUI != null && levelingUI.IsActive)
            {
                levelingUI.HandleMouseClick(e.X, e.Y, this.ClientSize);
                Invalidate();
                return;
            }
            
            var pauseMenu = _engine.UI.GetPauseMenu();
            // Если меню паузы активно, обрабатываем клик по кнопкам меню
            if (_engine.State == GameState.Paused && pauseMenu != null && pauseMenu.IsActive)
            {
                int width = 300, height = 300;
                int x = (this.ClientSize.Width - width) / 2;
                int y = (this.ClientSize.Height - height) / 2;
                
                // Если сейчас открыт список сохранений — кликаем по файлам
                if (typeof(PauseMenuComponent).GetField("_showLoadList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pauseMenu) is bool showLoadList && showLoadList)
                {
                    pauseMenu.HandleMouseClick(e.X, e.Y, x, y);
                    Invalidate();
                    return;
                }
                
                // Проверяем, в каком состоянии меню
                bool showSaveInput = (bool)typeof(PauseMenuComponent).GetField("_showSaveInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pauseMenu);
                bool showMusicControls = (bool)typeof(PauseMenuComponent).GetField("_showMusicControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pauseMenu);
                
                if (showSaveInput || showMusicControls)
                {
                    // Для подменю используем точные позиции
                    if (showMusicControls)
                    {
                        // Музыка: 4 кнопки с точными позициями
                        int[] musicButtonYPositions = { 80, 120, 160, 200 }; // Точные Y позиции из PauseMenuComponent
                        
                for (int i = 0; i < 4; i++)
                {
                            int btnY = y + musicButtonYPositions[i];
                            Rectangle btnRect = new Rectangle(x + 20, btnY, 260, 30); // Высота 30 для точности
                    if (btnRect.Contains(e.Location))
                    {
                                typeof(PauseMenuComponent).GetField("_selectedIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pauseMenu, i);
                                pauseMenu.PressButton();
                                Invalidate();
                                return;
                            }
                        }
                    }
                    // Для сохранения нет кликабельных кнопок, только ввод текста
                }
                else
                {
                    // Основное меню: 5 кнопок с точными позициями
                    int[] buttonYPositions = { 80, 120, 160, 200, 240 }; // Точные Y позиции из PauseMenuComponent
                    
                    for (int i = 0; i < 5; i++)
                    {
                        int btnY = y + buttonYPositions[i];
                        Rectangle btnRect = new Rectangle(x + 20, btnY, 260, 30); // Высота 30 для точности
                        if (btnRect.Contains(e.Location))
                        {
                        typeof(PauseMenuComponent).GetField("_selectedIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pauseMenu, i);
                        pauseMenu.PressButton();
                        Invalidate();
                        return;
                    }
                }
                }
                
                // Если клик вне кнопок — ничего не делаем
                return;
            }
            _engine.Input.MouseDown(e.Button);
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            _engine.Input.MouseUp(e.Button);
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            // проверяем систему прокачки
            var levelingUI = _engine.UI.GetLevelingUI();
            if (levelingUI != null && levelingUI.IsActive)
            {
                levelingUI.HandleMouseMove(e.X, e.Y, this.ClientSize);
                Invalidate();
                return;
            }
            
            // проверяем меню паузы
            var pauseMenu = _engine.UI.GetPauseMenu();
            if (_engine.State == GameState.Paused && pauseMenu != null && pauseMenu.IsActive)
            {
                int width = 300, height = 300;
                int x = (this.ClientSize.Width - width) / 2;
                int y = (this.ClientSize.Height - height) / 2;
                pauseMenu.HandleMouseMove(e.X, e.Y, x, y);
                Invalidate();
                return;
            }
            
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
            SubscribePauseMenuEvents();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            _engine.Render(e.Graphics);
            // Рисуем меню паузы
            var pauseMenu = _engine.UI.GetPauseMenu();
            if (pauseMenu != null && pauseMenu.IsActive)
            {
                pauseMenu.Draw(e.Graphics);
            }
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
            
            // проверяем систему прокачки
            var levelingUI = _engine.UI.GetLevelingUI();
            if (levelingUI != null && levelingUI.IsActive)
            {
                levelingUI.HandleInput(e.KeyCode);
                Invalidate();
                e.Handled = true;
                return;
            }
            
            var pauseMenu = _engine.UI.GetPauseMenu();
            if (_engine.State == GameState.Paused && pauseMenu != null && pauseMenu.IsActive)
            {
                if (pauseMenu.IsAwaitingInput())
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        pauseMenu.CancelInput();
                        Invalidate();
                        e.Handled = true;
                        return;
                    }
                    if (pauseMenu.IsAwaitingInput() && e.KeyCode == Keys.Back)
                    {
                        pauseMenu.Backspace();
                        Invalidate();
                        e.Handled = true;
                        return;
                    }
                    if (pauseMenu.IsAwaitingInput() && e.KeyCode == Keys.Enter)
                    {
                        pauseMenu.PressButton();
                        Invalidate();
                        e.Handled = true;
                        return;
                    }
                    if (pauseMenu.IsAwaitingInput() && pauseMenu.IsAwaitingInput() && e.KeyCode == Keys.Up)
                    {
                        pauseMenu.PrevButton();
                        Invalidate();
                        e.Handled = true;
                        return;
                    }
                    if (pauseMenu.IsAwaitingInput() && pauseMenu.IsAwaitingInput() && e.KeyCode == Keys.Down)
                    {
                        pauseMenu.NextButton();
                        Invalidate();
                        e.Handled = true;
                        return;
                    }
                }
                if (e.KeyCode == Keys.Escape)
                {
                    _engine.TogglePause();
                    e.Handled = true;
                    return;
                }
                if (e.KeyCode == Keys.Up)
                {
                    pauseMenu.PrevButton();
                    Invalidate();
                }
                else if (e.KeyCode == Keys.Down)
                {
                    pauseMenu.NextButton();
                    Invalidate();
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    pauseMenu.PressButton();
                    Invalidate();
                }
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                _engine.TogglePause();
                e.Handled = true;
                return;
            }
            _engine.Input.KeyDown(e.KeyCode);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var pauseMenu = _engine.UI.GetPauseMenu();
            if (_engine.State == GameState.Paused && pauseMenu != null && pauseMenu.IsActive && pauseMenu.IsAwaitingInput())
            {
                if (keyData >= Keys.A && keyData <= Keys.Z)
                {
                    char c = (char)keyData;
                    if (!Control.IsKeyLocked(Keys.CapsLock)) c = char.ToLower(c);
                    pauseMenu.InputChar(c);
                    Invalidate();
                    return true;
                }
                if (keyData >= Keys.D0 && keyData <= Keys.D9)
                {
                    char c = (char)('0' + (keyData - Keys.D0));
                    pauseMenu.InputChar(c);
                    Invalidate();
                    return true;
                }
                if (keyData == Keys.Space)
                {
                    pauseMenu.InputChar(' ');
                    Invalidate();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            _engine.Input.KeyUp(e.KeyCode);
            e.Handled = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Обновляем позицию игрока в статических данных Player перед сохранением
            /*
            var player = _engine.CurrentScene?.Entities.FirstOrDefault(entity => entity.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                Player.Position = player.Position;
            }
            
            // Сохраняем данные игрока
            Player.SaveToFile();
            */
            // Сохранение/загрузка временно отключены для автогенерации карт
        }
    }
}
