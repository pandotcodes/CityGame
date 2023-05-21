using Microsoft.Xna.Framework;
using System;
using System.Numerics;
using System.Reflection.Metadata;

namespace CityGame
{
    public static class Extensions
    {
        public static System.Drawing.Point Convert(this Point point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }
        public static Point Convert(this System.Drawing.Point point)
        {
            return new Point(point.X, point.Y);
        }
        public static bool CloselyEquals(this double A, double B)
        {
            return Math.Round(A) == Math.Round(B);
        }
        public static bool CloselyEquals(this float A, double B)
        {
            return Math.Round(A) == Math.Round(B);
        }
        public static bool CloselyEquals(this int A, int B)
        {
            return A == B;
        }
        public static Microsoft.Xna.Framework.Vector2 RotateBy(this Microsoft.Xna.Framework.Vector2 v, float angleInDegrees)
        {
            float radians = (float)(angleInDegrees * Math.PI / 180f);
            float sin = (float)Math.Sin(radians);
            float cos = (float)Math.Cos(radians);
            float rotatedX = v.X * cos - v.Y * sin;
            float rotatedY = v.X * sin + v.Y * cos;
            return new Microsoft.Xna.Framework.Vector2(rotatedX, rotatedY);
        }
        public static System.Numerics.Vector2 RotateBy(this System.Numerics.Vector2 v, float angleInDegrees)
        {
            float radians = (float)(angleInDegrees * Math.PI / 180f);
            float sin = (float)Math.Sin(radians);
            float cos = (float)Math.Cos(radians);
            float rotatedX = v.X * cos - v.Y * sin;
            float rotatedY = v.X * sin + v.Y * cos;
            return new System.Numerics.Vector2(rotatedX, rotatedY);
        }
    }
}