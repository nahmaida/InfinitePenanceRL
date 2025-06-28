using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace InfinitePenanceRL
{
    // Компонент для анимаций персонажа (бег, атака и т.д.)
    public class AnimationComponent : Component
    {
        // Словарь с анимациями (название -> массив кадров)
        private Dictionary<string, (int X, int Y)[]> _animations = new Dictionary<string, (int X, int Y)[]>();
        private string _currentAnimation = "idle";
        private int _currentFrame = 0;
        private float _frameTimer = 0;
        private const float FRAME_TIME = 0.1f; // Время между кадрами (чем больше, тем медленнее)
        private const float ATTACK_FRAME_TIME = 0.05f; // Быстрые кадры для атаки
        private const int SPRITE_SIZE = 16;
        private bool _isFirstUpdate = true;
        private string _lastLoggedAnimation = "";
        private RenderComponent _render;
        private bool _isAttacking = false;
        private float _attackDuration = 0f;
        private const float ATTACK_ANIMATION_DURATION = 0.2f; // 4 кадра * 0.05s = 0.2s (быстрее)
        private float _attackCooldown = 0f;
        private const float ATTACK_COOLDOWN_DURATION = 0.3f; // Кулдаун чуть больше анимации

        public AnimationComponent()
        {
            // Задаем координаты кадров для каждой анимации на спрайтшите
            _animations["idle"] = new[] { (1, 2) };                        // Стоим на месте
            _animations["walking"] = new[] { (1, 2), (4, 2), (7, 2), (10, 2) };  // Ходим
            _animations["attacking"] = new[] { (1, 5), (4, 5), (7, 5), (10, 5) }; // Атакуем
            _animations["running"] = new[] { (1, 8), (4, 8), (7, 8), (10, 8) };   // Бежим
        }

        public void PlayAnimation(string animationName)
        {
            // Не прерываем анимацию до конца
            if (_isAttacking && animationName != "attacking")
            {
                return;
            }

            if (_currentAnimation != animationName && _animations.ContainsKey(animationName))
            {
                if (_lastLoggedAnimation != animationName)
                {
                    using (StreamWriter writer = File.AppendText("game.log"))
                    {
                        writer.WriteLine($"[Animation] Changing from {_currentAnimation} to {animationName}");
                    }
                    _lastLoggedAnimation = animationName;
                }
                _currentAnimation = animationName;
                _currentFrame = 0;
                _frameTimer = 0;

                if (animationName == "attacking")
                {
                    _isAttacking = true;
                    _attackDuration = 0f;
                }

                UpdateSpriteRegion(); // Обновляем регион при смене анимации
            }
        }

        private void UpdateSpriteRegion()
        {
            if (!_animations.ContainsKey(_currentAnimation))
                return;

            if (_render == null)
            {
                _render = Owner.GetComponent<RenderComponent>();
            }

            if (_render != null)
            {
                var (x, y) = _animations[_currentAnimation][_currentFrame];
                _render.SpriteRegion = new Rectangle(x * SPRITE_SIZE, y * SPRITE_SIZE, SPRITE_SIZE, SPRITE_SIZE);
            }
        }

        public override void Update()
        {
            if (_isFirstUpdate)
            {
                _render = Owner.GetComponent<RenderComponent>();
                using (StreamWriter writer = File.AppendText("game.log"))
                {
                    writer.WriteLine($"[Animation] Initial update, setting sprite region for {_currentAnimation}");
                }
                UpdateSpriteRegion();
                _isFirstUpdate = false;
                return;
            }

            if (!_animations.ContainsKey(_currentAnimation))
                return;

            float deltaTime = 1.0f / 60; // При 60 фпс
            
            // Обновляем кулдаун атаки
            if (_attackCooldown > 0)
            {
                _attackCooldown -= deltaTime;
            }

            _frameTimer += deltaTime;

            // Update attack duration
            if (_isAttacking)
            {
                _attackDuration += deltaTime;
                if (_attackDuration >= ATTACK_ANIMATION_DURATION)
                {
                    _isAttacking = false;
                    _attackDuration = 0f;
                    PlayAnimation("idle");
                }
            }

            // Выбираем время между кадрами в зависимости от анимации
            float currentFrameTime = (_currentAnimation == "attacking") ? ATTACK_FRAME_TIME : FRAME_TIME;

            if (_frameTimer >= currentFrameTime)
            {
                _frameTimer = 0;
                _currentFrame = (_currentFrame + 1) % _animations[_currentAnimation].Length;
                UpdateSpriteRegion();
            }
        }

        // Проверяем, можно ли атаковать (нет кулдауна)
        public bool CanAttack()
        {
            return _attackCooldown <= 0;
        }

        // Запускаем атаку (если нет кулдауна)
        public void StartAttack()
        {
            if (CanAttack())
            {
                PlayAnimation("attacking");
                _attackCooldown = ATTACK_COOLDOWN_DURATION;
            }
        }
    }
} 