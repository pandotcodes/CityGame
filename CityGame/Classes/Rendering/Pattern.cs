using System;
using System.Linq;
using CityGame.Classes.World;

namespace CityGame.Classes.Rendering
{
    public struct Pattern
    {
        public string PatternCode { get; set; }
        public int Rotation { get; set; }
        public Pattern(string pattern)
        {
            PatternCode = pattern;
            Rotation = 0;
        }
        public Pattern(string pattern, int rotation)
        {
            PatternCode = pattern;
            Rotation = rotation;
        }
        public Pattern(string pattern, string rotation)
        {
            PatternCode = pattern;
            int.TryParse(rotation, out int rot);
            Rotation = rot;
        }
        public static Pattern Calculate(Tile[,] Grid, int x, int y, bool allowDiagonal = true, params TileType[] allowed)
        {
            Func<int, int, bool> IsAdjacent = (X, Y) =>
            {
                if (!Renderer.IsInGrid(Grid, X, Y)) return false;
                if (Grid[x, y].BlockID != Grid[X, Y].BlockID && Grid[x, y].Type == TileType.Skyscraper) return false;
                return allowed.Contains(Grid[X, Y].Type);
            };

            if (allowDiagonal)
            {
                if (IsAdjacent(x - 1, y - 1) && IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y) && IsAdjacent(x - 1, y + 1) && IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y + 1)) return new Pattern("8");

                if (IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y) && IsAdjacent(x - 1, y + 1) && IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y + 1)) return new Pattern("7");
                if (IsAdjacent(x - 1, y - 1) && IsAdjacent(x, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y) && IsAdjacent(x - 1, y + 1) && IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y + 1)) return new Pattern("7", "90");
                if (IsAdjacent(x - 1, y - 1) && IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y) && IsAdjacent(x - 1, y + 1) && IsAdjacent(x, y + 1)) return new Pattern("7", "180");
                if (IsAdjacent(x - 1, y - 1) && IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y) && IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y + 1)) return new Pattern("7", "270");

                if (IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x + 1, y) && IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y + 1)) return new Pattern("5");
                if (IsAdjacent(x, y - 1) && IsAdjacent(x - 1, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x, y + 1) && IsAdjacent(x - 1, y + 1)) return new Pattern("5", "180");
                if (IsAdjacent(x - 1, y - 1) && IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y)) return new Pattern("5", "270");
                if (IsAdjacent(x - 1, y + 1) && IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y + 1) && IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y)) return new Pattern("5", "90");

                if (IsAdjacent(x - 1, y) && IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x + 1, y)) return new Pattern("4");
                if (IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y) && IsAdjacent(x + 1, y + 1) && IsAdjacent(x, y + 1)) return new Pattern("4", "90");
                if (IsAdjacent(x + 1, y) && IsAdjacent(x - 1, y) && IsAdjacent(x - 1, y + 1) && IsAdjacent(x, y + 1)) return new Pattern("4", "180");
                if (IsAdjacent(x, y + 1) && IsAdjacent(x - 1, y) && IsAdjacent(x - 1, y - 1) && IsAdjacent(x, y - 1)) return new Pattern("4", "270");

                if (IsAdjacent(x + 1, y) && IsAdjacent(x, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x - 1, y - 1)) return new Pattern("4m");
                if (IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y) && IsAdjacent(x, y - 1) && IsAdjacent(x + 1, y - 1)) return new Pattern("4m", "90");
                if (IsAdjacent(x - 1, y) && IsAdjacent(x, y + 1) && IsAdjacent(x + 1, y) && IsAdjacent(x + 1, y + 1)) return new Pattern("4m", "180");
                if (IsAdjacent(x, y - 1) && IsAdjacent(x - 1, y) && IsAdjacent(x, y + 1) && IsAdjacent(x - 1, y + 1)) return new Pattern("4m", "270");
            }

            if (IsAdjacent(x + 1, y) && IsAdjacent(x, y + 1) && IsAdjacent(x - 1, y) && IsAdjacent(x, y - 1)) return new Pattern("4c");

            if (allowDiagonal)
            {
                if (IsAdjacent(x + 1, y) && IsAdjacent(x + 1, y + 1) && IsAdjacent(x, y + 1)) return new Pattern("3");
                if (IsAdjacent(x - 1, y) && IsAdjacent(x - 1, y + 1) && IsAdjacent(x, y + 1)) return new Pattern("3", "90");
                if (IsAdjacent(x - 1, y) && IsAdjacent(x - 1, y - 1) && IsAdjacent(x, y - 1)) return new Pattern("3", "180");
                if (IsAdjacent(x + 1, y) && IsAdjacent(x + 1, y - 1) && IsAdjacent(x, y - 1)) return new Pattern("3", "270");
            }

            if (IsAdjacent(x, y - 1) && IsAdjacent(x, y + 1) && IsAdjacent(x - 1, y)) return new Pattern("3c");
            if (IsAdjacent(x + 1, y) && IsAdjacent(x, y - 1) && IsAdjacent(x - 1, y)) return new Pattern("3c", "90");
            if (IsAdjacent(x + 1, y) && IsAdjacent(x, y - 1) && IsAdjacent(x, y + 1)) return new Pattern("3c", "180");
            if (IsAdjacent(x + 1, y) && IsAdjacent(x, y + 1) && IsAdjacent(x - 1, y)) return new Pattern("3c", "270");

            if (IsAdjacent(x - 1, y) && IsAdjacent(x + 1, y)) return new Pattern("2", "90");
            if (IsAdjacent(x, y - 1) && IsAdjacent(x, y + 1)) return new Pattern("2");

            if (IsAdjacent(x + 1, y) && IsAdjacent(x, y + 1)) return new Pattern("2c");
            if (IsAdjacent(x - 1, y) && IsAdjacent(x, y + 1)) return new Pattern("2c", "90");
            if (IsAdjacent(x - 1, y) && IsAdjacent(x, y - 1)) return new Pattern("2c", "180");
            if (IsAdjacent(x + 1, y) && IsAdjacent(x, y - 1)) return new Pattern("2c", "270");

            if (IsAdjacent(x + 1, y)) return new Pattern("1");
            if (IsAdjacent(x - 1, y)) return new Pattern("1", "180");
            if (IsAdjacent(x, y + 1)) return new Pattern("1", "90");
            if (IsAdjacent(x, y - 1)) return new Pattern("1", "270");

            return new Pattern("0");
        }
    }
}