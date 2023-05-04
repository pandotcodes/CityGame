using System.Windows.Controls;

namespace CityGame
{
    public class Renderer
    {
        public static bool IsRoad(Tile[,] Grid, int x, int y)
        {
            return IsInGrid(Grid, x, y) && (Grid[x, y].Type == TileType.Road || Grid[x, y].Type == TileType.Bridge);
        }
        public static bool IsInGrid(Tile[,] Grid, int x, int y)
        {
            return !(x < 0 || y < 0 || x >= Grid.GetLength(0) || y >= Grid.GetLength(1));
        }
        public static bool IsBuilding(Tile[,] Grid, int x, int y)
        {
            return IsInGrid(Grid, x, y) && (Grid[x, y].Type == TileType.Skyscraper || Grid[x, y].Type == TileType.House);
        }
        public static OCanvas Render(TileType type, int x, int y, Tile[,] Grid)
        {
            if (type == TileType.Skyscraper)
            {
                string theme = "";
                if (Grid[x, y].BlockID % 2 == 1) theme = "Dark";
                if (Grid[x, y].BlockID % 30 == 1) theme = "Blue";
                if (Grid[x, y].BlockID % 30 == 2) theme = "Red";
                if (Grid[x, y].BlockID % 30 == 3) theme = "Green";
                Pattern pattern = Pattern.Calculate(Grid, x, y, TileType.Skyscraper);
                if (pattern.PatternCode == "1" && MainWindow.random.Next(0, 3) == 0) return new SourcedImage("ParkingLot"+theme+".png:" + pattern.Rotation);
                if (pattern.PatternCode == "3" && MainWindow.random.Next(0, 12) == 0) pattern.PatternCode = "3a";
                if (pattern.PatternCode == "3" && MainWindow.random.Next(0, 12) == 1) pattern.PatternCode = "3ab";
                OCanvas canvas = new SourcedImage("Building"+ theme + pattern.PatternCode + ".png:" + pattern.Rotation);

                if (MainWindow.random.Next(0, 10) == 0 && pattern.PatternCode != "3a") canvas.Children.Add(new SourcedImage("Vent" + (MainWindow.random.Next(0, 3) + 1) + ".png:" + (MainWindow.random.Next(0, 4) * 90)));

                return canvas;
            }
            if (type == TileType.Lake || type == TileType.River)
            {
                Pattern pattern = Pattern.Calculate(Grid, x, y, TileType.Lake, TileType.Bridge, TileType.River);
                return new SourcedImage("Lake" + pattern.PatternCode + ".png:" + pattern.Rotation);
            }
            if (type == TileType.Park)
            {
                Pattern pattern = Pattern.Calculate(Grid, x, y, TileType.Park, TileType.Path);
                OCanvas canvas = new SourcedImage("Park" + pattern.PatternCode + ".png:" + pattern.Rotation);
                if (MainWindow.random.Next(0, 4) == 0) canvas.Children.Add(new SourcedImage("Tree.png:" + MainWindow.random.Next(0,4) * 90));
                return canvas;
            }
            if (type == TileType.Road)
            {
                Pattern pattern = Pattern.Calculate(Grid, x, y, TileType.Road, TileType.Path, TileType.Bridge);
                if (pattern.PatternCode == "2c") pattern.Rotation += 270;
                if (pattern.PatternCode == "1") pattern.Rotation += 180;
                return new SourcedImage("Road" + pattern.PatternCode + ".png:" + pattern.Rotation);
            }
            if (type == TileType.Path)
            {
                Pattern roadpattern = Pattern.Calculate(Grid, x, y, TileType.Road, TileType.Path, TileType.Bridge);
                Pattern parkpattern = Pattern.Calculate(Grid, x, y, TileType.Path, TileType.Park);
                if (roadpattern.PatternCode == "2c") roadpattern.Rotation += 270;
                if (roadpattern.PatternCode == "1") roadpattern.Rotation += 180;
                Image path = new SourcedImage("Path" + roadpattern.PatternCode + ".png:" + roadpattern.Rotation);
                Image park = new SourcedImage("Park" + parkpattern.PatternCode + ".png:" + parkpattern.Rotation);

                OCanvas result = new OCanvas();
                result.Children.Add(park);
                result.Children.Add(path);
                return result;
            }
            if (type == TileType.Bridge)
            {
                Pattern roadpattern = Pattern.Calculate(Grid, x, y, TileType.Road, TileType.Bridge, TileType.Path);
                Pattern parkpattern = Pattern.Calculate(Grid, x, y, TileType.Bridge, TileType.Lake, TileType.River);
                if (roadpattern.PatternCode == "2c") roadpattern.Rotation += 270;
                if (roadpattern.PatternCode == "1") roadpattern.Rotation += 180;
                Image path = new SourcedImage("Bridge" + roadpattern.PatternCode + ".png:" + roadpattern.Rotation);
                Image park = new SourcedImage("Lake" + parkpattern.PatternCode + ".png:" + parkpattern.Rotation);

                OCanvas result = new OCanvas();
                result.Children.Add(park);
                result.Children.Add(path);
                return result;
            }
            return new SourcedImage("Error.png");
        }
    }
}