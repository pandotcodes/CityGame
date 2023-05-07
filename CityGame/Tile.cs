namespace CityGame
{
    public struct Tile : ISelectable
    {
        public int BlockID;
        public TileType Type;
        public int X;
        public int Y;

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
    }
}