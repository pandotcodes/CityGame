namespace CityGame
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