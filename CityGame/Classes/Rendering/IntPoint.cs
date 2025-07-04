﻿using Microsoft.Xna.Framework;

namespace CityGame.Classes.Rendering
{
    public struct IntPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public IntPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
        public static IntPoint operator +(IntPoint a, IntPoint b)
        {
            return new IntPoint(a.X + b.X, a.Y + b.Y);
        }
        public static IntPoint operator -(IntPoint a, Vector2 b)
        {
            return new IntPoint(a.X - (int)b.X, a.Y - (int)b.Y);
        }
        public static IntPoint operator +(IntPoint a, Vector2 b)
        {
            return new IntPoint(a.X + (int)b.X, a.Y + (int)b.Y);
        }
        public static IntPoint operator *(IntPoint a, int b)
        {
            return new IntPoint(a.X * b, a.Y * b);
        }
        public static bool operator !=(IntPoint a, IntPoint b)
        {
            return a.X != b.X || a.Y != b.Y;
        }
        public static bool operator ==(IntPoint a, IntPoint b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
    }
}