using System.Collections.Generic;
using System.Windows.Forms;

namespace InfinitePenanceRL
{
    // Тут обработка ввода
    public class InputManager
    {
        private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>();

        public void KeyDown(Keys key)
        {
            if (!_pressedKeys.Contains(key))
            {
                _pressedKeys.Add(key);
            }
        }

        public void KeyUp(Keys key)
        {
            if (_pressedKeys.Contains(key))
            {
                _pressedKeys.Remove(key);
            }
        }

        public bool IsKeyDown(Keys key)
        {
            return _pressedKeys.Contains(key);
        }
    }
}