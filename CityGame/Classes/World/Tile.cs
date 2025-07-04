﻿using CityGame.Classes.Rendering;
using OrpticonGameHelper.Classes.Elements;

namespace CityGame.Classes.World
{
    public struct Tile : ISelectable
    {
        public bool IsSingleSelect() => false;
        public int BlockID;
        public TileType Type;
        public Pattern Pattern;
        public int X;
        public int Y;
        public UIElement Element;

        public OCanvas GetImage()
        {
            return MainWindow.ImageGrid[X, Y];
        }

        public bool RunAction(ISelectable target)
        {
            return false;
        }

        int ISelectable.X()
        {
            return X * MainWindow.TileSize;
        }

        int ISelectable.Y()
        {
            return Y * MainWindow.TileSize;
        }
        public override string ToString()
        {
            return Type.ToString() + " at " + X + ";" + Y;
        }
    }
}