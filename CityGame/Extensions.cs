using System;
using System.Windows;

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
    }
}