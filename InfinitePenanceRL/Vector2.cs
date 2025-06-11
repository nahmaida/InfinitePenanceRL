using System;

namespace InfinitePenanceRL
{
    // Двумерный вектор - для работы с координатами и направлениями
    public struct Vector2
    {
        public float X { get; set; }  // Координата по горизонтали
        public float Y { get; set; }  // Координата по вертикали

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Нулевой вектор (точка 0,0)
        public static Vector2 Zero => new Vector2(0, 0);

        // Сложение векторов (например, позиция + смещение)
        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new Vector2(a.X + b.X, a.Y + b.Y);

        // Вычитание векторов (например, найти направление от одной точки к другой)
        public static Vector2 operator -(Vector2 a, Vector2 b)
            => new Vector2(a.X - b.X, a.Y - b.Y);

        // Умножение вектора на число (например, увеличить скорость)
        public static Vector2 operator *(Vector2 v, float scalar)
            => new Vector2(v.X * scalar, v.Y * scalar);

        // Деление вектора на число
        public static Vector2 operator /(Vector2 v, float scalar)
            => new Vector2(v.X / scalar, v.Y / scalar);

        // Длина вектора (расстояние от начала координат)
        public float Length()
            => (float)Math.Sqrt(X * X + Y * Y);

        // Нормализация вектора (делаем длину = 1, сохраняя направление)
        public Vector2 Normalize()
        {
            float length = Length();
            if (length > 0)
                return new Vector2(X / length, Y / length);
            return Zero;
        }

        // Расстояние между двумя точками
        public static float Distance(Vector2 a, Vector2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}