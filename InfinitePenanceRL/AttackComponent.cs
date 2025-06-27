using System;
using System.Linq;
using System.Windows.Forms;

namespace InfinitePenanceRL
{
    public class AttackComponent : Component
    {
        public float AttackRange { get; set; } = 60f; // Дальность атаки в пикселях
        private DateTime _lastAttackTime = DateTime.MinValue;
        private readonly TimeSpan _attackCooldown = TimeSpan.FromMilliseconds(500); // Кулдаун между атаками
        private AnimationComponent _animation;

        public override void Update()
        {
            // Инициализируем анимацию, если ещё не сделали
            if (_animation == null)
            {
                _animation = Owner.GetComponent<AnimationComponent>();
            }

            // Проверяем клик мыши для атаки
            if (Game.Input.IsLeftMouseDown() && CanAttack())
            {
                PerformAttack();
                _lastAttackTime = DateTime.Now;
            }
        }

        private bool CanAttack()
        {
            return DateTime.Now - _lastAttackTime >= _attackCooldown;
        }

        private void PerformAttack()
        {
            // Находим всех врагов в радиусе атаки
            var enemies = Owner.Game.CurrentScene.Entities
                .Where(e => e.GetComponent<EnemyTag>() != null)
                .Where(e => !e.IsMarkedForDeletion)
                .ToList();

            Entity closestEnemy = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float distance = Vector2.Distance(Owner.Position, enemy.Position);
                if (distance <= AttackRange && distance < closestDistance)
                {
                    closestEnemy = enemy;
                    closestDistance = distance;
                }
            }

            if (closestEnemy != null)
            {
                // Атакуем ближайшего врага
                var enemyComponent = closestEnemy.GetComponent<EnemyComponent>();
                if (enemyComponent != null)
                {
                    enemyComponent.TakeDamage(Player.Damage);
                    LogThrottler.Log($"Player attacked enemy for {Player.Damage} damage", "player_attack");
                }
            }
            else
            {
                LogThrottler.Log("Player attacked but no enemy in range", "player_attack");
            }
            // Играем анимацию
            _animation?.PlayAnimation("attacking");
        }
    }
}
