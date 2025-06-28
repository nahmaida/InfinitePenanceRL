using System;
using System.Drawing;
using System.Windows.Forms;

namespace InfinitePenanceRL
{
    public class LevelingUIComponent : UIComponent
    {
        private bool _showLevelUpMenu = false;
        private int _selectedOption = 0;
        private int _hoveredOption = -1; // -1 означает, что ничего не подсвечено
        private readonly string[] _upgradeOptions = { "Здоровье +20", "Урон +5", "Скорость +1" };
        private readonly StatUpgrade[] _upgrades = { StatUpgrade.Health, StatUpgrade.Damage, StatUpgrade.Speed };

        public bool IsActive => _showLevelUpMenu;

        public override void Update()
        {
            // проверяем, можно ли повысить уровень
            if (Player.CanLevelUp && !_showLevelUpMenu)
            {
                _showLevelUpMenu = true;
                _selectedOption = 0;
                _hoveredOption = -1;
            }
        }

        public void HandleInput(Keys key)
        {
            if (!_showLevelUpMenu) return;

            switch (key)
            {
                case Keys.Escape:
                    _showLevelUpMenu = false;
                    break;
            }
        }

        public void HandleMouseMove(int mouseX, int mouseY, Size viewportSize)
        {
            if (!_showLevelUpMenu) return;

            // размеры меню
            int menuWidth = 300;
            int menuHeight = 200;
            int x = (viewportSize.Width - menuWidth) / 2;
            int y = (viewportSize.Height - menuHeight) / 2;

            // проверяем, над какой опцией находится мышь
            _hoveredOption = -1;
            for (int i = 0; i < _upgradeOptions.Length; i++)
            {
                int optionY = y + 70 + i * 30;
                Rectangle optionRect = new Rectangle(x + 20, optionY, menuWidth - 40, 25);
                
                if (optionRect.Contains(mouseX, mouseY))
                {
                    _hoveredOption = i;
                    break;
                }
            }
        }

        public void HandleMouseClick(int mouseX, int mouseY, Size viewportSize)
        {
            if (!_showLevelUpMenu) return;

            // размеры меню
            int menuWidth = 300;
            int menuHeight = 200;
            int x = (viewportSize.Width - menuWidth) / 2;
            int y = (viewportSize.Height - menuHeight) / 2;

            // проверяем, кликнули ли по опциям улучшений
            for (int i = 0; i < _upgradeOptions.Length; i++)
            {
                int optionY = y + 70 + i * 30;
                Rectangle optionRect = new Rectangle(x + 20, optionY, menuWidth - 40, 25);
                
                if (optionRect.Contains(mouseX, mouseY))
                {
                    _selectedOption = i;
                    ApplyUpgrade();
                    return;
                }
            }
        }

        private void ApplyUpgrade()
        {
            Player.LevelUp(_upgrades[_selectedOption]);
            _showLevelUpMenu = false;
        }

        public override void Draw(Graphics g)
        {
            // этот метод не используется, так как мы рисуем через Draw(g, viewportSize)
        }

        public void Draw(Graphics g, Size viewportSize)
        {
            // рисуем информацию об уровне в правом верхнем углу
            DrawLevelInfo(g, viewportSize);

            // рисуем меню выбора улучшения, если активно
            if (_showLevelUpMenu)
            {
                DrawLevelUpMenu(g, viewportSize);
            }
        }

        private void DrawLevelInfo(Graphics g, Size viewportSize)
        {
            var font = new Font("Arial", 12, FontStyle.Bold);
            var brush = new SolidBrush(Color.White);
            var outlineBrush = new SolidBrush(Color.Black);

            // информация об уровне
            string levelText = $"Уровень: {Player.Level}";
            string expText = $"Опыт: {Player.CurrentKills}/{Player.KillsForNextLevel}";

            // позиция в правом верхнем углу
            int x = viewportSize.Width - 150;
            int y = 10;

            // рисуем обводку
            g.DrawString(levelText, font, outlineBrush, x + 1, y + 1);
            g.DrawString(expText, font, outlineBrush, x + 1, y + 25);

            // рисуем основной текст
            g.DrawString(levelText, font, brush, x, y);
            g.DrawString(expText, font, brush, x, y + 25);

            // рисуем прогресс-бар опыта
            DrawExperienceBar(g, x, y + 50, 140, 10);

            font.Dispose();
            brush.Dispose();
            outlineBrush.Dispose();
        }

        private void DrawExperienceBar(Graphics g, int x, int y, int width, int height)
        {
            // фон прогресс-бара
            var backgroundBrush = new SolidBrush(Color.DarkGray);
            g.FillRectangle(backgroundBrush, x, y, width, height);

            // прогресс
            float progress = (float)Player.CurrentKills / Player.KillsForNextLevel;
            int progressWidth = (int)(width * progress);
            
            if (progressWidth > 0)
            {
                var progressBrush = new SolidBrush(Color.Yellow);
                g.FillRectangle(progressBrush, x, y, progressWidth, height);
                progressBrush.Dispose();
            }

            // рамка
            g.DrawRectangle(Pens.White, x, y, width, height);

            backgroundBrush.Dispose();
        }

        private void DrawLevelUpMenu(Graphics g, Size viewportSize)
        {
            // полупрозрачный фон
            var backgroundBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            g.FillRectangle(backgroundBrush, 0, 0, viewportSize.Width, viewportSize.Height);

            // размеры меню
            int menuWidth = 300;
            int menuHeight = 200;
            int x = (viewportSize.Width - menuWidth) / 2;
            int y = (viewportSize.Height - menuHeight) / 2;

            // фон меню
            var menuBrush = new SolidBrush(Color.FromArgb(200, 50, 50, 50));
            g.FillRectangle(menuBrush, x, y, menuWidth, menuHeight);
            g.DrawRectangle(Pens.White, x, y, menuWidth, menuHeight);

            // заголовок
            var titleFont = new Font("Arial", 16, FontStyle.Bold);
            var titleBrush = new SolidBrush(Color.Yellow);
            string title = "ПОВЫШЕНИЕ УРОВНЯ!";
            var titleSize = g.MeasureString(title, titleFont);
            int titleX = x + (menuWidth - (int)titleSize.Width) / 2;
            g.DrawString(title, titleFont, titleBrush, titleX, y + 20);

            // опции улучшений
            var optionFont = new Font("Arial", 12, FontStyle.Regular);
            var normalBrush = new SolidBrush(Color.White);
            var selectedBrush = new SolidBrush(Color.Yellow);

            for (int i = 0; i < _upgradeOptions.Length; i++)
            {
                // определяем, какую опцию подсвечивать (только при наведении мыши)
                bool isHighlighted = (i == _hoveredOption);
                var brush = isHighlighted ? selectedBrush : normalBrush;
                int optionY = y + 70 + i * 30;
                
                // рисуем фон для кликабельной области
                var clickableBrush = new SolidBrush(Color.FromArgb(isHighlighted ? 100 : 50, 255, 255, 255));
                g.FillRectangle(clickableBrush, x + 20, optionY, menuWidth - 40, 25);
                clickableBrush.Dispose();
                
                // индикатор выбора
                if (isHighlighted)
                {
                    g.DrawString(">", optionFont, selectedBrush, x + 20, optionY);
                }
                
                g.DrawString(_upgradeOptions[i], optionFont, brush, x + 40, optionY);
            }

            // подсказка
            var hintFont = new Font("Arial", 10, FontStyle.Italic);
            var hintBrush = new SolidBrush(Color.LightGray);
            string hint = "Кликните по опции для выбора";
            var hintSize = g.MeasureString(hint, hintFont);
            int hintX = x + (menuWidth - (int)hintSize.Width) / 2;
            g.DrawString(hint, hintFont, hintBrush, hintX, y + menuHeight - 30);

            // очистка ресурсов
            backgroundBrush.Dispose();
            menuBrush.Dispose();
            titleFont.Dispose();
            titleBrush.Dispose();
            optionFont.Dispose();
            normalBrush.Dispose();
            selectedBrush.Dispose();
            hintFont.Dispose();
            hintBrush.Dispose();
        }
    }
} 