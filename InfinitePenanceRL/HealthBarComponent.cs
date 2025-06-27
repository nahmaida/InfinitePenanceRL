using System.Drawing;

namespace InfinitePenanceRL
{
    public class HealthBarComponent : UIComponent
    {
        public int MaxHealth { get; set; } = 100;
        public int CurrentHealth { get; set; } = 100;
        public Size Size { get; set; } = new Size(200, 20);
        
        private static bool gameOverShown = false; // чтобы не спамить Game Over

        public override void Draw(Graphics g)
        {
            if (!IsVisible) return;

            // Фон
            g.FillRectangle(new SolidBrush(Color.DarkGray),
                ScreenPosition.X, ScreenPosition.Y,
                Size.Width, Size.Height);

            // Строка здоровья
            float healthPercentage = (float)CurrentHealth / MaxHealth;
            int healthWidth = (int)(Size.Width * healthPercentage);
            
            g.FillRectangle(new SolidBrush(Color.Red),
                ScreenPosition.X, ScreenPosition.Y,
                healthWidth, Size.Height);

            // Границы
            g.DrawRectangle(Pens.White,
                ScreenPosition.X, ScreenPosition.Y,
                Size.Width, Size.Height);

            // Текст
            string healthText = $"{CurrentHealth}/{MaxHealth}";
            using (var font = new Font("Arial", 10)) // шрифт
            {
                var textSize = g.MeasureString(healthText, font);
                g.DrawString(healthText, font, Brushes.White,
                    ScreenPosition.X + (Size.Width - textSize.Width) / 2,
                    ScreenPosition.Y + (Size.Height - textSize.Height) / 2);
            }
        }

        public override void Update()
        {
            // Синхронизируем здоровье с Player.Health
            CurrentHealth = (int)Player.Health;
            MaxHealth = 100; // Можно заменить на Player.MaxHealth если появится

            // Проверяем на проигрыш
            if (Player.Health <= 0 && !gameOverShown)
            {
                gameOverShown = true;
                System.Windows.Forms.MessageBox.Show("Game Over", "Game Over", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                System.Windows.Forms.Application.Exit();
            }

            // LogThrottler.Log($"HealthBarComponent: Player.Health={Player.Health}, CurrentHealth={CurrentHealth}", "healthbar");
        }
    }
} 