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
        private float _attackCooldown = 2.0f; // Кулдаун атаки в секундах
        private float _attackTimer = 0f;
        private float _attackRange = 100f; // Радиус атаки (увеличен для теста)
        private bool _hasCreatedDeathParticles = false; // Флаг для создания частиц смерти

        public EnemyComponent()
        {
            LogThrottler.Log("Создан EnemyComponent", "enemy_debug");
        }

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
            // LogThrottler.Log("EnemyComponent.Update вызван", "enemy_debug");
            
            // Если враг только что умер — создаём кровь
            if (IsDead && !_hasCreatedDeathParticles)
            {
                Game.Particles.CreateBloodSplatter(Owner.Position, 12);
                _hasCreatedDeathParticles = true;
                return;
            }
            
            if (IsDead) return;

            // Находим игрока
            var player = Game.CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                var toPlayer = player.Position - Owner.Position;
                // LogThrottler.Log($"Позиция врага: {Owner.Position.X},{Owner.Position.Y} | Позиция игрока: {player.Position.X},{player.Position.Y}", "enemy_attack");
                float dist = Vector2.Distance(player.Position, Owner.Position);
                // Проверяем прямую видимость (по горизонтали или вертикали, без стен)
                bool straightLine = false;
                int x0 = (int)((Owner.Position.X + 16) / Scene.CellSize);
                int y0 = (int)((Owner.Position.Y + 16) / Scene.CellSize);
                int x1 = (int)((player.Position.X + 16) / Scene.CellSize);
                int y1 = (int)((player.Position.Y + 16) / Scene.CellSize);
                if (x0 == x1)
                {
                    straightLine = true;
                    int minY = Math.Min(y0, y1), maxY = Math.Max(y0, y1);
                    for (int y = minY + 1; y < maxY; y++)
                        if (!Game.CurrentScene.IsWalkable(x0, y)) straightLine = false;
                }
                else if (y0 == y1)
                {
                    straightLine = true;
                    int minX = Math.Min(x0, x1), maxX = Math.Max(x0, x1);
                    for (int x = minX + 1; x < maxX; x++)
                        if (!Game.CurrentScene.IsWalkable(x, y0)) straightLine = false;
                }
                // Если есть прямая видимость — идём к игроку
                if (straightLine && dist > _attackRange)
                {
                    var dir = (player.Position - Owner.Position).Normalize();
                    var newPos = Owner.Position + dir * Speed;
                    int cellX = (int)((newPos.X + 16) / Scene.CellSize);
                    int cellY = (int)((newPos.Y + 16) / Scene.CellSize);
                    if (Game.CurrentScene.IsWalkable(cellX, cellY) && Game.Physics.CanMoveTo(Owner, newPos))
                    {
                        Owner.Position = newPos;
                    }
                }
                // Если рядом с игроком — атакуем
                // LogThrottler.Log($"Враг проверяет атаку: расстояние до игрока = {dist}, радиус атаки = {_attackRange}", "enemy_attack");
                if (dist <= _attackRange)
                {
                    _attackTimer += 1.0f / 60.0f;
                    if (_attackTimer >= _attackCooldown)
                    {
                        _attackTimer = 0f;
                        Player.Health -= Attack;
                        LogThrottler.Log($"Враг атакует игрока на {Attack} урона!", "enemy_attack");
                        LogThrottler.Log($"Текущее здоровье игрока: {Player.Health}", "enemy_attack");
                        
                        // Запускаем тряску экрана при получении урона
                        Game.TriggerScreenShake();
                    }
                }
                else
                {
                    _attackTimer = 0f;
                }
            }
            else
            {
                // Старое поведение (рандомное блуждание)
                _moveTimer += 1.0f / 60.0f;
                if (_moveTimer >= _moveInterval)
                {
                    _moveTimer = 0f;
                    int dir = _random.Next(5);
                    switch (dir)
                    {
                        case 0: _moveDirection = new Vector2(0, -1); break;
                        case 1: _moveDirection = new Vector2(0, 1); break;
                        case 2: _moveDirection = new Vector2(-1, 0); break;
                        case 3: _moveDirection = new Vector2(1, 0); break;
                        default: _moveDirection = new Vector2(0, 0); break;
                    }
                }
                if (_moveDirection.X != 0 || _moveDirection.Y != 0)
                {
                    var newPos = Owner.Position + _moveDirection * Speed;
                    int cellX = (int)((newPos.X + 16) / Scene.CellSize);
                    int cellY = (int)((newPos.Y + 16) / Scene.CellSize);
                    if (Owner.Game.CurrentScene.IsWalkable(cellX, cellY) && Owner.Game.Physics.CanMoveTo(Owner, newPos))
                    {
                        Owner.Position = newPos;
                    }
                }
            }
        }
    }

    // Маркер для врагов
    public class EnemyTag : Component { }
}
