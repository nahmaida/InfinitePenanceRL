namespace InfinitePenanceRL
{
    public class EnemyComponent : Component
    {
        public float Health { get; set; } = 50f;
        public float MaxHealth { get; set; } = 50f;
        public float Attack { get; set; } = 15f;
        public float Speed { get; set; } = 3f;
        public bool IsDead => Health <= 0;

        private static readonly System.Random _random = new System.Random();
        private float _moveTimer = 0f;
        private float _moveInterval = 1.0f; // секунды
        private Vector2 _moveDirection = new Vector2(0, 0);

        public void TakeDamage(float damage)
        {
            Health -= damage;
            LogThrottler.Log($"Enemy took {damage} damage, health: {Health}/{MaxHealth}", "enemy_damage");
            
            if (IsDead)
            {
                LogThrottler.Log("Enemy defeated!", "enemy_defeat");
                // Удаляем врага
                Owner.MarkForDeletion();
            }
        }

        public override void Update()
        {
            if (IsDead) return;

            // Обновляем таймер
            _moveTimer += 1.0f / 60.0f; // При 60 фпс
            if (_moveTimer >= _moveInterval)
            {
                _moveTimer = 0f;
                // Выбираем рандомное направление
                int dir = _random.Next(5);
                switch (dir)
                {
                    case 0: _moveDirection = new Vector2(0, -1); break; // Вверх
                    case 1: _moveDirection = new Vector2(0, 1); break;  // Вниз
                    case 2: _moveDirection = new Vector2(-1, 0); break; // Влево
                    case 3: _moveDirection = new Vector2(1, 0); break;  // Вправо
                    default: _moveDirection = new Vector2(0, 0); break; // Никуда
                }
            }

            if (_moveDirection.X != 0 || _moveDirection.Y != 0)
            {
                var newPos = Owner.Position + _moveDirection * Speed;
                // Конвертируем в координаты
                int cellX = (int)((newPos.X + 16) / Scene.CellSize); // 16 (половина тайла)
                int cellY = (int)((newPos.Y + 16) / Scene.CellSize);
                if (Owner.Game.CurrentScene.IsWalkable(cellX, cellY) && Owner.Game.Physics.CanMoveTo(Owner, newPos))
                {
                    Owner.Position = newPos;
                }
            }
        }
    }

    // Маркер для врагов
    public class EnemyTag : Component { }
}
