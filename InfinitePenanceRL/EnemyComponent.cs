using System;
using System.Drawing;
using System.Numerics;

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
        private float _attackRange = 50f; // Радиус атаки
        private bool _hasCreatedDeathParticles = false; // Флаг для создания частиц смерти
        private float _projectileCooldown = 3.0f; // Кулдаун снарядов в секундах
        private float _projectileTimer = 0f;
        private float _projectileRange = 400f; // Дальность стрельбы снарядами

        public EnemyComponent()
        {
            LogThrottler.Log("Создан EnemyComponent", "enemy_debug");
        }

        // Метод для получения урона
        public void TakeDamage(float damage)
        {
            if (IsDead) return;
            
            // Воспроизводим звук боли
            Game.Sounds.PlayEnemyPain();
            
            // Уменьшаем здоровье
            Health -= damage;
            LogThrottler.Log($"Enemy took {damage} damage, health: {Health}/{MaxHealth}", "enemy_damage");
            
            // Если здоровье упало до 0 или ниже — враг умирает
            if (Health <= 0)
            {
                Owner.MarkForDeletion();
            }
        }

        public override void Update()
        {
            LogThrottler.Log("EnemyComponent.Update вызван", "enemy_debug");
            
            // Если враг только что умер — создаём кровь
            if (IsDead && !_hasCreatedDeathParticles)
            {
                Game.Particles.CreateBloodSplatter(Owner.Position, 12);
                Game.Sounds.PlayEnemyDeath(); // Звук смерти врага
                _hasCreatedDeathParticles = true;
                return;
            }
            
            if (IsDead) return;

            // Находим игрока
            var player = Game.CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                var toPlayer = player.Position - Owner.Position;
                LogThrottler.Log($"Позиция врага: {Owner.Position.X},{Owner.Position.Y} | Позиция игрока: {player.Position.X},{player.Position.Y}", "enemy_attack");
                float dist = Vector2.Distance(player.Position, Owner.Position);
                
                // Иногда воспроизводим звук ворчания, если враг рядом
                if (dist < 200 && _random.Next(1000) < 2) // 0.2% шанс каждый кадр
                {
                    Game.Sounds.PlayEnemyGrunt();
                }
                
                // Проверяем прямую видимость (по горизонтали, вертикали или диагонали, без стен)
                bool straightLine = HasLineOfSight(Owner.Position, player.Position);

                // Проверяем, можно ли запустить снаряд
                _projectileTimer -= 1.0f / 60.0f;
                if (straightLine && dist <= _projectileRange && dist > _attackRange && _projectileTimer <= 0)
                {
                    LogThrottler.Log($"Условия для запуска снаряда выполнены: straightLine={straightLine}, dist={dist}, range={_projectileRange}", "projectile");
                    _projectileTimer = _projectileCooldown;
                    LaunchProjectile(player.Position);
                }

                LogThrottler.Log($"Враг проверяет атаку: расстояние до игрока = {dist}, радиус атаки = {_attackRange}", "enemy_attack");
                if (dist <= _attackRange)
                {
                    // Если рядом с игроком — атакуем
                    _attackTimer -= 1.0f / 60; // Уменьшаем таймер
                    if (_attackTimer <= 0)
                    {
                        _attackTimer = _attackCooldown;
                        Player.Health -= Attack;
                        LogThrottler.Log($"Враг атакует игрока на {Attack} урона!", "enemy_attack");
                        LogThrottler.Log($"Текущее здоровье игрока: {Player.Health}", "enemy_attack");
                        
                        // Запускаем тряску экрана при получении урона
                        Game.TriggerScreenShake();
                    }
                }
                else if (straightLine && dist > _attackRange)
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

        // Запускаем снаряд в игрока
        private void LaunchProjectile(Vector2 targetPosition)
        {
            try
            {
                var direction = targetPosition - Owner.Position;
                var projectile = new Entity();
                projectile.Game = Owner.Game;
                projectile.Position = Owner.Position;
                
                // Добавляем компоненты снаряда
                projectile.AddComponent(new ProjectileComponent(direction, Owner.Position, (int)Attack * 2));
                projectile.AddComponent(new ColliderComponent());
                projectile.AddComponent(new RenderComponent());
                
                // Настраиваем RenderComponent
                var render = projectile.GetComponent<RenderComponent>();
                render.SpriteName = "projectiles_16px";
                render.Size = new Size(16, 16);
                render.Scale = 2.0f; // Увеличиваем размер для видимости
                render.SpriteRegion = new Rectangle(0, 0, 16, 16); // Первый кадр снаряда
                
                // Вычисляем и устанавливаем поворот снаряда
                float angle = (float)Math.Atan2(direction.Y, direction.X);
                render.Rotation = angle;
                
                // Добавляем снаряд на сцену
                Owner.Game.Projectiles.Add(projectile);
                
                LogThrottler.Log($"Враг запустил снаряд в игрока! Урон: {Attack * 2}, Позиция: {Owner.Position.X}, {Owner.Position.Y}", "projectile");
            }
            catch (Exception ex)
            {
                LogThrottler.Log($"Ошибка запуска снаряда: {ex.Message}", "projectile");
            }
        }

        // Проверяем прямую видимость между двумя точками
        public static bool HasLineOfSight(Vector2 from, Vector2 to, GameEngine game)
        {
            var scene = game.CurrentScene;
            if (scene == null) return false;

            // Определяем направление
            Vector2 direction = to - from;
            float distance = direction.Length();
            
            if (distance < 1) return true; // Очень близко
            
            direction = direction / distance; // Нормализуем
            
            // Проверяем каждые 16 пикселей на пути
            float stepSize = 16.0f;
            int steps = (int)(distance / stepSize) + 1;
            
            for (int i = 1; i < steps; i++) // Начинаем с 1, чтобы не проверять начальную точку
            {
                Vector2 checkPoint = from + direction * (i * stepSize);
                int cellX = (int)(checkPoint.X / Scene.CellSize);
                int cellY = (int)(checkPoint.Y / Scene.CellSize);
                
                // Если точка в стене, нет прямой видимости
                if (!scene.IsWalkable(cellX, cellY))
                {
                    return false;
                }
            }
            
            return true;
        }

        // Проверяем прямую видимость между двумя точками (для использования внутри класса)
        private bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            return HasLineOfSight(from, to, Owner.Game);
        }
    }

    // Маркер для врагов
    public class EnemyTag : Component { }
}
