using System.Windows.Forms;
using System;

namespace InfinitePenanceRL
{
    // Компонент для движения персонажа (ходьба, бег и все такое)
    public class MovementComponent : Component
    {
        public float Speed { get; set; } = 5f;
        private float _baseSpeed;
        private AnimationComponent _animation;
        private RenderComponent _render;
        private bool _isInitialized = false;

        public override void Update()
        {
            if (!_isInitialized)
            {
                _animation = Owner.GetComponent<AnimationComponent>();
                _render = Owner.GetComponent<RenderComponent>();
                _baseSpeed = Player.Speed;
                Speed = _baseSpeed;
                _isInitialized = true;
            }

            var position = Owner.Position;
            var newPosition = position;
            bool isMoving = false;

            // Проверяем, бежит ли перс (шифт нажат)
            bool isRunning = Game.Input.IsKeyDown(Keys.ShiftKey);
            float currentSpeed = isRunning ? _baseSpeed * 2 : _baseSpeed;

            // Обрабатываем движение
            if (Game.Input.IsKeyDown(Keys.W)) { newPosition.Y -= currentSpeed; isMoving = true; }
            if (Game.Input.IsKeyDown(Keys.S)) { newPosition.Y += currentSpeed; isMoving = true; }
            if (Game.Input.IsKeyDown(Keys.A)) 
            { 
                newPosition.X -= currentSpeed; 
                isMoving = true;
                if (_render != null) _render.FlipHorizontal = true; // Разворачиваем спрайт влево
            }
            if (Game.Input.IsKeyDown(Keys.D)) 
            { 
                newPosition.X += currentSpeed; 
                isMoving = true;
                if (_render != null) _render.FlipHorizontal = false; // Возвращаем спрайт вправо
            }

            // Если игрок двигается — создаём пыль
            if (isMoving && Game.Particles != null)
            {
                Vector2 moveDirection = (newPosition - position).Normalize();
                // Создаём пыль под ногами игрока (внизу по центру спрайта)
                Vector2 dustPosition = position + new Vector2(16, 32); // 16 = половина ширины, 32 = полная высота
                Game.Particles.CreateDust(dustPosition, moveDirection, 2);
            }

            // Анимации движения (ходьба/бег)
            if (isMoving)
            {
                _animation?.PlayAnimation("running"); // должно быть moving
            }
            else
            {
                _animation?.PlayAnimation("walking"); // должно быть idle
            }

            // Проверяем коллизии и двигаем персонажа
            if (Game.Physics.CanMoveTo(Owner, newPosition))
            {
                Owner.Position = newPosition;
            }
        }
    }
}