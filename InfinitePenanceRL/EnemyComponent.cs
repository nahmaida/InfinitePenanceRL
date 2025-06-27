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
        private float _moveInterval = 1.0f; // seconds
        private Vector2 _moveDirection = new Vector2(0, 0);

        public void TakeDamage(float damage)
        {
            Health -= damage;
            LogThrottler.Log($"Enemy took {damage} damage, health: {Health}/{MaxHealth}", "enemy_damage");
            
            if (IsDead)
            {
                LogThrottler.Log("Enemy defeated!", "enemy_defeat");
                // Mark entity for deletion
                Owner.MarkForDeletion();
            }
        }

        public override void Update()
        {
            if (IsDead) return;

            // Update timer
            _moveTimer += 1.0f / 60.0f; // Assuming 60 FPS
            if (_moveTimer >= _moveInterval)
            {
                _moveTimer = 0f;
                // Pick a random direction: up, down, left, right, or stay
                int dir = _random.Next(5);
                switch (dir)
                {
                    case 0: _moveDirection = new Vector2(0, -1); break; // Up
                    case 1: _moveDirection = new Vector2(0, 1); break;  // Down
                    case 2: _moveDirection = new Vector2(-1, 0); break; // Left
                    case 3: _moveDirection = new Vector2(1, 0); break;  // Right
                    default: _moveDirection = new Vector2(0, 0); break; // Stay
                }
            }

            if (_moveDirection.X != 0 || _moveDirection.Y != 0)
            {
                var newPos = Owner.Position + _moveDirection * Speed;
                // Convert to maze cell coordinates
                int cellX = (int)((newPos.X + 16) / Scene.CellSize); // 16: half tile offset
                int cellY = (int)((newPos.Y + 16) / Scene.CellSize);
                if (Owner.Game.CurrentScene.IsWalkable(cellX, cellY) && Owner.Game.Physics.CanMoveTo(Owner, newPos))
                {
                    Owner.Position = newPos;
                }
            }
        }
    }

    // Marker component to identify enemies
    public class EnemyTag : Component { }
}
