using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace InfinitePenanceRL
{
    // Обработка всего ввода (клавиши, мышка и т.д.)
    public class InputManager
    {
        private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        private bool _isLeftMouseDown;
        private bool _isRightMouseDown;
        private bool _debugLogging = true;

        private void Log(string message)
        {
            if (_debugLogging)
            {
                using (StreamWriter writer = File.AppendText("game.log"))
                {
                    writer.WriteLine($"[Input] {message}");
                }
            }
        }

        // Обработка нажатия клавиши
        public void KeyDown(Keys key)
        {
            if (!_pressedKeys.Contains(key))
            {
                _pressedKeys.Add(key);
                Log($"Нажата клавиша: {key}");
            }
        }

        // Обработка отпускания клавиши
        public void KeyUp(Keys key)
        {
            if (_pressedKeys.Contains(key))
            {
                _pressedKeys.Remove(key);
                Log($"Отпущена клавиша: {key}");
            }
        }

        // Проверка, нажата ли сейчас клавиша
        public bool IsKeyDown(Keys key)
        {
            bool isDown = _pressedKeys.Contains(key);
            if (isDown)
            {
                Log($"Клавиша нажата: {key}");
            }
            return isDown;
        }

        // Обработка нажатия кнопок мыши
        public void MouseDown(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                _isLeftMouseDown = true;
                Log("Нажата левая кнопка мыши");
            }
            else if (button == MouseButtons.Right)
            {
                _isRightMouseDown = true;
                Log("Нажата правая кнопка мыши");
            }
        }

        // Обработка отпускания кнопок мыши
        public void MouseUp(MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                _isLeftMouseDown = false;
                Log("Отпущена левая кнопка мыши");
            }
            else if (button == MouseButtons.Right)
            {
                _isRightMouseDown = false;
                Log("Отпущена правая кнопка мыши");
            }
        }

        public bool IsLeftMouseDown() => _isLeftMouseDown;
        public bool IsRightMouseDown() => _isRightMouseDown;
    }
}