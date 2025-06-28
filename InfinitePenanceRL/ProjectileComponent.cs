using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace InfinitePenanceRL
{
    // Компонент для снарядов
    public class ProjectileComponent : Component
    {
        private Vector2 _direction;
        private float _speed = 300.0f; // Скорость снаряда (увеличена в 2 раза)
        private float _maxDistance = 300.0f; // Максимальная дистанция полёта
        private float _distanceTraveled = 0.0f;
        private Vector2 _startPosition;
        private int _damage = 4; // Урон снаряда (2x от обычной атаки)
        private float _animationTimer = 0.0f;
        private float _animationSpeed = 8.0f; // Скорость анимации
        private int _currentFrame = 0;
        private const int FRAME_COUNT = 4;
        private bool _shouldDelete = false; // Флаг для отложенного удаления

        public bool IsActive { get; private set; } = true;

        public ProjectileComponent(Vector2 direction, Vector2 startPosition, int damage = 4)
        {
            float length = direction.Length();
            _direction = length > 0 ? direction / length : new Vector2(1, 0);
            _startPosition = startPosition;
            _damage = damage;
        }

        public override void Update()
        {
            if (!IsActive) return;

            // Обновляем анимацию
            _animationTimer += 1.0f / 60.0f; // Предполагаем 60 FPS
            if (_animationTimer >= 1.0f / _animationSpeed)
            {
                _animationTimer = 0.0f;
                _currentFrame = (_currentFrame + 1) % FRAME_COUNT;
            }

            // Двигаем снаряд
            Vector2 movement = _direction * _speed * (1.0f / 60.0f);
            Owner.Position += movement;
            _distanceTraveled += movement.Length();

            // Проверяем, не пролетел ли снаряд максимальную дистанцию
            if (_distanceTraveled >= _maxDistance)
            {
                IsActive = false;
                _shouldDelete = true;
                return;
            }

            // Проверяем коллизию с игроком
            var player = Owner.Game.CurrentScene.Entities.FirstOrDefault(e => e.GetComponent<PlayerTag>() != null);
            if (player != null)
            {
                var playerCollider = player.GetComponent<ColliderComponent>();
                var projectileCollider = Owner.GetComponent<ColliderComponent>();
                
                if (playerCollider != null && projectileCollider != null)
                {
                    if (playerCollider.Bounds.IntersectsWith(projectileCollider.Bounds))
                    {
                        // Попадание в игрока
                        Player.Health -= _damage;
                        LogThrottler.Log($"Снаряд попал в игрока! Урон: {_damage}", "projectile");
                        
                        IsActive = false;
                        _shouldDelete = true;
                        return;
                    }
                }
            }

            // Проверяем коллизию со стенами
            var projectilePos = Owner.Position;
            var scene = Owner.Game.CurrentScene;
            if (scene != null)
            {
                int cellX = (int)(projectilePos.X / Scene.CellSize);
                int cellY = (int)(projectilePos.Y / Scene.CellSize);
                
                // Проверяем границы лабиринта
                if (cellX >= 0 && cellY >= 0)
                {
                    // Используем IsWalkable для проверки стен (инвертируем результат)
                    if (!scene.IsWalkable(cellX, cellY))
                    {
                        // Снаряд попал в стену
                        IsActive = false;
                        _shouldDelete = true;
                        return;
                    }
                }
            }
        }

        public int GetCurrentFrame()
        {
            return _currentFrame;
        }

        // Проверяем, нужно ли удалить снаряд
        public bool ShouldDelete()
        {
            return _shouldDelete;
        }
    }
} 