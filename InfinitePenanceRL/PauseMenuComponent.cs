using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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
        private bool _showLoadList = false;
        private List<string> _saveFiles = new List<string>();
        private int _selectedSaveIndex = 0;
        private string _saveInput = "";
        private bool _isTyping = false;
        private bool _showMusicControls = false;

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
                // Основные кнопки меню
                if (!_showSaveInput && !_showLoadList && !_showMusicControls)
                {
                    g.DrawString("Продолжить", font, _selectedIndex == 0 ? Brushes.Yellow : Brushes.White, x + 20, y + 80);
                    g.DrawString("Сохранить", font, _selectedIndex == 1 ? Brushes.Yellow : Brushes.White, x + 20, y + 120);
                    g.DrawString("Загрузить", font, _selectedIndex == 2 ? Brushes.Yellow : Brushes.White, x + 20, y + 160);
                    g.DrawString("Музыка", font, _selectedIndex == 3 ? Brushes.Yellow : Brushes.White, x + 20, y + 200);
                    g.DrawString("Выход", font, _selectedIndex == 4 ? Brushes.Yellow : Brushes.White, x + 20, y + 240);
                }
                // Управление музыкой
                else if (_showMusicControls)
                {
                    g.DrawString("Пауза/Играть", font, _selectedIndex == 0 ? Brushes.Yellow : Brushes.White, x + 20, y + 80);
                    g.DrawString("Следующий трек", font, _selectedIndex == 1 ? Brushes.Yellow : Brushes.White, x + 20, y + 120);
                    g.DrawString("Предыдущий трек", font, _selectedIndex == 2 ? Brushes.Yellow : Brushes.White, x + 20, y + 160);
                    g.DrawString("Назад", font, _selectedIndex == 3 ? Brushes.Yellow : Brushes.White, x + 20, y + 200);
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
                    if (_saveFiles.Count == 0)
                    {
                        g.DrawString("Нет сохранений", font, Brushes.Gray, x + 40, y + 130);
                    }
                    else
                    {
                        for (int i = 0; i < _saveFiles.Count; i++)
                        {
                            var brush = i == _selectedSaveIndex ? Brushes.Yellow : Brushes.White;
                            string filename = Path.GetFileNameWithoutExtension(_saveFiles[i]);
                            g.DrawString(filename, font, brush, x + 40, y + 110 + i * 30);
                        }
                    }
                }
            }
        }

        public void NextButton()
        {
            if (_showLoadList && _saveFiles.Count > 0)
            {
                _selectedSaveIndex = (_selectedSaveIndex + 1) % _saveFiles.Count;
            }
            else if (_showMusicControls)
            {
                _selectedIndex = (_selectedIndex + 1) % 4; // 4 кнопки в музыке
            }
            else if (!_showSaveInput && !_showLoadList && !_showMusicControls)
            {
                _selectedIndex = (_selectedIndex + 1) % 5; // 5 кнопок в главном меню
            }
        }

        public void PrevButton()
        {
            if (_showLoadList && _saveFiles.Count > 0)
            {
                _selectedSaveIndex = (_selectedSaveIndex - 1 + _saveFiles.Count) % _saveFiles.Count;
            }
            else if (_showMusicControls)
            {
                _selectedIndex = (_selectedIndex - 1 + 4) % 4; // 4 кнопки в музыке
            }
            else if (!_showSaveInput && !_showLoadList && !_showMusicControls)
            {
                _selectedIndex = (_selectedIndex - 1 + 5) % 5; // 5 кнопок в главном меню
            }
        }

        public void PressButton()
        {
            if (!IsActive) return;

            // Воспроизводим звук клика кнопки
            Owner.Game.Sounds.PlayButtonClick();

            if (_showSaveInput)
            {
                if (!string.IsNullOrEmpty(_saveInput))
                {
                    SaveRequested?.Invoke(_saveInput);
                    _showSaveInput = false;
                    _saveInput = "";
                    _isTyping = false;
                }
            }
            else if (_showLoadList)
            {
                if (_saveFiles.Count > 0 && _selectedSaveIndex < _saveFiles.Count)
                {
                    string filename = Path.GetFileNameWithoutExtension(_saveFiles[_selectedSaveIndex]);
                    LogThrottler.Log($"Загружаем сохранение: {filename}", "pause_menu");
                    LoadRequested?.Invoke(filename);
                    _showLoadList = false;
                }
                else
                {
                    LogThrottler.Log($"Нет файлов для загрузки. Count: {_saveFiles.Count}, Selected: {_selectedSaveIndex}", "pause_menu");
                }
            }
            else if (_showMusicControls)
            {
                switch (_selectedIndex)
                {
                    case 0:
                        Owner.Game.Music.TogglePause();
                        break;
                    case 1:
                        Owner.Game.Music.PlayNextTrack();
                        break;
                    case 2:
                        Owner.Game.Music.PlayPreviousTrack();
                        break;
                    case 3:
                        _showMusicControls = false;
                        _selectedIndex = 0;
                        break;
                }
            }
            else
            {
                switch (_selectedIndex)
                {
                    case 0:
                        OnButtonPressed?.Invoke(PauseMenuAction.Resume);
                        break;
                    case 1:
                        _showSaveInput = true;
                        _isTyping = true;
                        _saveInput = "";
                        break;
                    case 2:
                        LoadSaveFiles();
                        _showLoadList = true;
                        _selectedSaveIndex = 0;
                        LogThrottler.Log("Открыт список сохранений", "pause_menu");
                        break;
                    case 3:
                        _showMusicControls = true;
                        _selectedIndex = 0;
                        break;
                    case 4:
                        OnButtonPressed?.Invoke(PauseMenuAction.Exit);
                        break;
                }
            }
        }

        private void LoadSaveFiles()
        {
            _saveFiles.Clear();
            if (Directory.Exists("saves"))
            {
                var files = Directory.GetFiles("saves", "*.json");
                _saveFiles.AddRange(files);
                LogThrottler.Log($"Загружено {_saveFiles.Count} файлов сохранений", "pause_menu");
                foreach (var file in _saveFiles)
                {
                    LogThrottler.Log($"Файл сохранения: {Path.GetFileName(file)}", "pause_menu");
                }
            }
            else
            {
                LogThrottler.Log("Папка saves не найдена", "pause_menu");
            }
        }

        public void InputChar(char c)
        {
            if (_showSaveInput && _saveInput.Length < 32 && !char.IsControl(c))
                _saveInput += c;
        }

        public void Backspace()
        {
            if (_showSaveInput && _saveInput.Length > 0)
                _saveInput = _saveInput.Substring(0, _saveInput.Length - 1);
        }

        public bool IsAwaitingInput() => _showSaveInput || _showLoadList;

        public void CancelInput()
        {
            _showSaveInput = false;
            _showLoadList = false;
            _showMusicControls = false;
            _saveInput = "";
        }

        public void HandleMouseClick(int mouseX, int mouseY, int menuX, int menuY)
        {
            if (_showLoadList && _saveFiles.Count > 0)
            {
                for (int i = 0; i < _saveFiles.Count; i++)
                {
                    int itemX = menuX + 40;
                    int itemY = menuY + 110 + i * 30;
                    var rect = new System.Drawing.Rectangle(itemX, itemY, 200, 24);
                    if (rect.Contains(mouseX, mouseY))
                    {
                        _selectedSaveIndex = i;
                        string filename = Path.GetFileNameWithoutExtension(_saveFiles[_selectedSaveIndex]);
                        LoadRequested?.Invoke(filename);
                        _showLoadList = false;
                        return;
                    }
                }
            }
        }
    }
} 
