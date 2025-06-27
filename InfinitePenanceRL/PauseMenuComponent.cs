using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace InfinitePenanceRL
{
    public enum PauseMenuAction { Resume, Save, Load, Exit }

    public class PauseMenuComponent : UIComponent
    {
        public bool IsActive { get; set; } = false;
        private readonly (PauseMenuAction Action, string Label)[] _buttons = {
            (PauseMenuAction.Resume, "Продолжить"),
            (PauseMenuAction.Save, "Сохранить"),
            (PauseMenuAction.Load, "Загрузить"),
            (PauseMenuAction.Exit, "Выход")
        };
        private int _selectedIndex = 0;
        public event Action<PauseMenuAction> OnButtonPressed;
        public event Action<string> SaveRequested;
        public event Action<string> LoadRequested;

        private bool _showSaveInput = false;
        private string _saveInput = "";
        private bool _showLoadList = false;
        private string[] _saveFiles = Array.Empty<string>();
        private int _loadSelected = 0;

        public override void Draw(Graphics g)
        {
            if (!IsActive) return;
            int width = 300, height = 300;
            int x = (int)(g.VisibleClipBounds.Width - width) / 2;
            int y = (int)(g.VisibleClipBounds.Height - height) / 2;
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 30, 30, 30)), x, y, width, height);
            g.DrawRectangle(Pens.White, x, y, width, height);
            using (var font = new Font("Arial", 24, FontStyle.Bold))
            {
                g.DrawString("Меню", font, Brushes.White, x + 70, y + 20);
            }
            using (var font = new Font("Arial", 16))
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    var btnY = y + 80 + i * 50;
                    var brush = i == _selectedIndex ? Brushes.Yellow : Brushes.White;
                    g.DrawString(_buttons[i].Label, font, brush, x + 100, btnY);
                }
            }
            if (_showSaveInput)
            {
                g.FillRectangle(Brushes.Black, x + 20, y + 220, 260, 40);
                g.DrawRectangle(Pens.White, x + 20, y + 220, 260, 40);
                using (var font = new Font("Arial", 14))
                {
                    if (string.IsNullOrEmpty(_saveInput))
                        g.DrawString("Введите название:", font, Brushes.White, x + 30, y + 225);
                    g.DrawString(_saveInput + "|", font, Brushes.Yellow, x + 30, y + 225);
                }
            }
            if (_showLoadList)
            {
                g.FillRectangle(Brushes.Black, x + 20, y + 80, 260, 180);
                g.DrawRectangle(Pens.White, x + 20, y + 80, 260, 180);
                using (var font = new Font("Arial", 14))
                {
                    g.DrawString("Выберите сохранение:", font, Brushes.White, x + 30, y + 85);
                    for (int i = 0; i < _saveFiles.Length; i++)
                    {
                        var brush = i == _loadSelected ? Brushes.Yellow : Brushes.White;
                        g.DrawString(_saveFiles[i], font, brush, x + 40, y + 110 + i * 30);
                    }
                }
            }
        }

        public void NextButton() { if (!_showSaveInput && !_showLoadList) _selectedIndex = (_selectedIndex + 1) % _buttons.Length; else if (_showLoadList && _saveFiles.Length > 0) _loadSelected = (_loadSelected + 1) % _saveFiles.Length; }
        public void PrevButton() { if (!_showSaveInput && !_showLoadList) _selectedIndex = (_selectedIndex - 1 + _buttons.Length) % _buttons.Length; else if (_showLoadList && _saveFiles.Length > 0) _loadSelected = (_loadSelected - 1 + _saveFiles.Length) % _saveFiles.Length; }
        public void PressButton()
        {
            if (_showSaveInput)
            {
                if (!string.IsNullOrWhiteSpace(_saveInput))
                {
                    SaveRequested?.Invoke(_saveInput.EndsWith(".json") ? _saveInput : _saveInput + ".json");
                    _showSaveInput = false;
                    _saveInput = "";
                }
                return;
            }
            if (_showLoadList)
            {
                if (_saveFiles.Length > 0)
                {
                    LoadRequested?.Invoke(_saveFiles[_loadSelected]);
                    _showLoadList = false;
                }
                return;
            }
            var action = _buttons[_selectedIndex].Action;
            if (action == PauseMenuAction.Save)
            {
                _showSaveInput = true;
                _saveInput = "";
            }
            else if (action == PauseMenuAction.Load)
            {
                _showLoadList = true;
                _saveFiles = Directory.Exists("saves") ? Directory.GetFiles("saves", "*.json").Select(Path.GetFileName).ToArray() : Array.Empty<string>();
                _loadSelected = 0;
            }
            else
            {
                OnButtonPressed?.Invoke(action);
            }
        }
        public void InputChar(char c) { if (_showSaveInput && _saveInput.Length < 32 && !char.IsControl(c)) _saveInput += c; }
        public void Backspace() { if (_showSaveInput && _saveInput.Length > 0) _saveInput = _saveInput.Substring(0, _saveInput.Length - 1); }
        public bool IsAwaitingInput() => _showSaveInput || _showLoadList;
        public void CancelInput() { _showSaveInput = false; _showLoadList = false; _saveInput = ""; }

        public void HandleMouseClick(int mouseX, int mouseY, int menuX, int menuY)
        {
            if (_showLoadList && _saveFiles.Length > 0)
            {
                for (int i = 0; i < _saveFiles.Length; i++)
                {
                    int itemX = menuX + 40;
                    int itemY = menuY + 110 + i * 30;
                    var rect = new System.Drawing.Rectangle(itemX, itemY, 200, 24);
                    if (rect.Contains(mouseX, mouseY))
                    {
                        _loadSelected = i;
                        LoadRequested?.Invoke(_saveFiles[_loadSelected]);
                        _showLoadList = false;
                        return;
                    }
                }
            }
        }
    }
} 
