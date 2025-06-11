using System;

namespace InfinitePenanceRL
{
    // работа с текстурками
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);

        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b)
            => new Vector2(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator *(Vector2 v, float scalar)
            => new Vector2(v.X * scalar, v.Y * scalar);

        public float Length()
            => (float)Math.Sqrt(X * X + Y * Y);

        public Vector2 Normalize()
        {
            float length = Length();
            if (length > 0)
                return new Vector2(X / length, Y / length);
            return Zero;
        }
    }
}