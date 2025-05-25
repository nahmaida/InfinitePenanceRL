using System.Windows.Forms;

namespace InfinitePenanceRL
{
    // Логика движения
    public class MovementComponent : Component
    {
        // Скорость перса
        public float Speed { get; set; } = 5f;

        public override void Update()
        {
            var position = Owner.Position;

            if (Game.Input.IsKeyDown(Keys.W)) position.Y -= Speed;
            if (Game.Input.IsKeyDown(Keys.S)) position.Y += Speed;
            if (Game.Input.IsKeyDown(Keys.A)) position.X -= Speed;
            if (Game.Input.IsKeyDown(Keys.D)) position.X += Speed;

            Owner.Position = position;
        }
    }
}