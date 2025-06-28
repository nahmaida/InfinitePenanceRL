using System;
using System.Collections.Generic;
using System.Drawing;

namespace InfinitePenanceRL
{
    // Простая система частиц для визуальных эффектов
    public class ParticleSystem
    {
        private List<Particle> _particles = new List<Particle>();
        private Random _random = new Random();

        // Класс одной частицы
        public class Particle
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public Color Color { get; set; }
            public float Life { get; set; }
            public float MaxLife { get; set; }
            public float Size { get; set; }
            public float Alpha { get; set; } = 1.0f;
        }

        // Создаём кровь при смерти врага
        public void CreateBloodSplatter(Vector2 position, int count = 8)
        {
            for (int i = 0; i < count; i++)
            {
                var particle = new Particle
                {
                    Position = position,
                    Velocity = new Vector2(
                        (float)(_random.NextDouble() - 0.5) * 100f, // Случайная скорость по X
                        (float)(_random.NextDouble() - 0.5) * 100f  // Случайная скорость по Y
                    ),
                    Color = Color.DarkRed,
                    Life = 1.0f,
                    MaxLife = 1.0f,
                    Size = (float)(_random.NextDouble() * 3 + 2) // Размер от 2 до 5
                };
                _particles.Add(particle);
            }
        }

        // Создаём пыль при движении
        public void CreateDust(Vector2 position, Vector2 direction, int count = 3)
        {
            for (int i = 0; i < count; i++)
            {
                var particle = new Particle
                {
                    Position = position + new Vector2(
                        (float)(_random.NextDouble() - 0.5) * 10f,
                        (float)(_random.NextDouble() - 0.5) * 10f
                    ),
                    Velocity = direction * 20f + new Vector2(
                        (float)(_random.NextDouble() - 0.5) * 30f,
                        (float)(_random.NextDouble() - 0.5) * 30f
                    ),
                    Color = Color.LightGray,
                    Life = 0.5f,
                    MaxLife = 0.5f,
                    Size = (float)(_random.NextDouble() * 2 + 1) // Размер от 1 до 3
                };
                _particles.Add(particle);
            }
        }

        // Обновляем все частицы
        public void Update(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                
                // Обновляем позицию
                particle.Position += particle.Velocity * deltaTime;
                
                // Замедляем частицу
                particle.Velocity *= 0.95f;
                
                // Уменьшаем время жизни
                particle.Life -= deltaTime;
                
                // Вычисляем прозрачность
                particle.Alpha = particle.Life / particle.MaxLife;
                
                // Удаляем мёртвые частицы
                if (particle.Life <= 0)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        // Рисуем все частицы
        public void Draw(Graphics g, Camera camera)
        {
            foreach (var particle in _particles)
            {
                var screenPos = camera.WorldToScreen(particle.Position);
                
                // Создаём цвет с прозрачностью
                var color = Color.FromArgb(
                    (int)(particle.Alpha * 255),
                    particle.Color.R,
                    particle.Color.G,
                    particle.Color.B
                );
                
                using (var brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, 
                        screenPos.X - particle.Size / 2,
                        screenPos.Y - particle.Size / 2,
                        particle.Size,
                        particle.Size
                    );
                }
            }
        }
    }
} 