using System.Windows.Forms;

namespace InfinitePenanceRL
{
    // Компонент для движения персонажа (ходьба, бег и все такое)
    public class MovementComponent : Component
    {
        // Базовая скорость перса
        public float Speed { get; set; } = 5f;
        private float _baseSpeed;
        private AnimationComponent _animation;
        private RenderComponent _render;

        public override void Update()
        {
            if (_animation == null)
            {
                _animation = Owner.GetComponent<AnimationComponent>();
                _render = Owner.GetComponent<RenderComponent>();
                _baseSpeed = Speed;
            }

            var position = Owner.Position;
            var newPosition = position;
            bool isMoving = false;

            // Проверяем, бежит ли перс (шифт нажат)
            bool isRunning = Game.Input.IsKeyDown(Keys.ShiftKey);
            Speed = isRunning ? _baseSpeed * 2 : _baseSpeed;

            // Обрабатываем движение
            if (Game.Input.IsKeyDown(Keys.W)) { newPosition.Y -= Speed; isMoving = true; }
            if (Game.Input.IsKeyDown(Keys.S)) { newPosition.Y += Speed; isMoving = true; }
            if (Game.Input.IsKeyDown(Keys.A)) 
            { 
                newPosition.X -= Speed; 
                isMoving = true;
                if (_render != null) _render.FlipHorizontal = true; // Разворачиваем спрайт влево
            }
            if (Game.Input.IsKeyDown(Keys.D)) 
            { 
                newPosition.X += Speed; 
                isMoving = true;
                if (_render != null) _render.FlipHorizontal = false; // Возвращаем спрайт вправо
            }

            // Анимация атаки по клику мыши
            if (Game.Input.IsLeftMouseDown())
            {
                _animation?.PlayAnimation("attacking");
            }
            // Анимации движения (ходьба/бег)
            else if (isMoving)
            {
                _animation?.PlayAnimation(isRunning ? "running" : "walking");
            }
            else
            {
                _animation?.PlayAnimation("idle");
            }

            // Проверяем коллизии и двигаем персонажа
            if (Game.Physics.CanMoveTo(Owner, newPosition))
            {
                Owner.Position = newPosition;
            }
        }
    }
}