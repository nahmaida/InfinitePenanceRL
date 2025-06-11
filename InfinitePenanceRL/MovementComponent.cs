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
            var newPosition = position;

            if (Game.Input.IsKeyDown(Keys.W)) newPosition.Y -= Speed;
            if (Game.Input.IsKeyDown(Keys.S)) newPosition.Y += Speed;
            if (Game.Input.IsKeyDown(Keys.A)) newPosition.X -= Speed;
            if (Game.Input.IsKeyDown(Keys.D)) newPosition.X += Speed;

            if (Game.Physics.CanMoveTo(Owner, newPosition))
            {
                Owner.Position = newPosition;
            }
        }
    }
}