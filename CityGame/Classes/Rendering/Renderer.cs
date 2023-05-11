using CityGame.Classes.World;
using OrpticonGameHelper.Classes.Elements;

namespace CityGame.Classes.Rendering
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
            string tooltip = x + ":" + y;
            if (type == TileType.Skyscraper || type == TileType.Garage || type == TileType.Helipad)
            {
                string theme = "";
                if (Grid[x, y].BlockID % 2 == 1) theme = "Dark";
                if (Grid[x, y].BlockID % 30 == 1) theme = "Blue";
                if (Grid[x, y].BlockID % 30 == 2) theme = "Red";
                if (Grid[x, y].BlockID % 30 == 3) theme = "Green";
                Pattern pattern = Pattern.Calculate(Grid, x, y, true, TileType.Skyscraper, TileType.Garage, TileType.Helipad);
                if (pattern.PatternCode == "1" && MainWindow.random.Next(0, 3) == 0) return new SourcedImage("ParkingLot" + theme + ".png:" + pattern.Rotation) { ZIndex = 25 };
                if (pattern.PatternCode == "3" && MainWindow.random.Next(0, 12) == 0) pattern.PatternCode = "3a";
                if (pattern.PatternCode == "3" && MainWindow.random.Next(0, 12) == 1) pattern.PatternCode = "3ab";
                OCanvas canvas = new SourcedImage("Building" + theme + pattern.PatternCode + ".png:" + pattern.Rotation, tooltip) { ZIndex = 25 };

                if (theme == "Blue" && pattern.PatternCode == "8" && MainWindow.random.Next(0, 4) == 0) Grid[x, y].Type = TileType.Helipad;
                else if (theme == "Blue" && pattern.PatternCode == "5" && MainWindow.random.Next(0, 2) == 0) Grid[x, y].Type = TileType.Garage;
                else if (theme == "Blue" && pattern.PatternCode == "0") Grid[x, y].Type = TileType.Garage;
                else if (MainWindow.random.Next(0, 10) == 0 && pattern.PatternCode != "3a") canvas.Children.Add(new SourcedImage("Vent" + (MainWindow.random.Next(0, 3) + 1) + ".png:" + MainWindow.random.Next(0, 4) * 90) { ZIndex = 50 });

                if (pattern.PatternCode == "5" && Grid[x, y].Type == TileType.Garage) canvas.Children.Add(new SourcedImage("Garage.png:" + pattern.Rotation) { ZIndex = 50 });
                if (pattern.PatternCode == "0" && Grid[x, y].Type == TileType.Garage) canvas.Children.Add(new SourcedImage("Garage.png:270") { ZIndex = 50 });
                if (Grid[x,y].Type == TileType.Helipad) canvas.Children.Add(new SourcedImage("Helipad.png") { ZIndex = 50 });

                Grid[x, y].Pattern = pattern;
                return canvas;
            }
            if (type == TileType.Lake || type == TileType.River)
            {
                Pattern pattern = Pattern.Calculate(Grid, x, y, true, TileType.Lake, TileType.Bridge, TileType.River, TileType.HighwayBridge);
                Grid[x, y].Pattern = pattern;
                return new SourcedImage("Lake" + pattern.PatternCode + ".png:" + pattern.Rotation, tooltip) { ZIndex = 1 };
            }
            if (type == TileType.Park)
            {
                Pattern pattern = Pattern.Calculate(Grid, x, y, true, TileType.Park, TileType.Path);
                Grid[x, y].Pattern = pattern;
                OCanvas canvas = new SourcedImage("Park" + pattern.PatternCode + ".png:" + pattern.Rotation);
                if (MainWindow.random.Next(0, 4) == 0) canvas.Children.Add(new SourcedImage("Tree.png:" + MainWindow.random.Next(0, 4) * 90, tooltip));
                canvas.ZIndex = 1;
                return canvas;
            }
            if (type == TileType.Road)
            {
                Pattern pattern = Pattern.Calculate(Grid, x, y, false, TileType.Road, TileType.Path, TileType.Bridge, TileType.Highway, TileType.HighwayBridge, TileType.Garage);
                Grid[x, y].Pattern = pattern;
                if (pattern.PatternCode == "2c") pattern.Rotation += 270;
                if (pattern.PatternCode == "1") pattern.Rotation += 180;
                return new SourcedImage("Road" + pattern.PatternCode + ".png:" + pattern.Rotation, tooltip) { ZIndex = 2 };
            }
            if (type == TileType.Highway)
            {
                Pattern pattern = Pattern.Calculate(Grid, x, y, false, TileType.Road, TileType.Path, TileType.Bridge, TileType.Highway, TileType.HighwayBridge, TileType.Garage);
                Grid[x, y].Pattern = pattern;
                if (pattern.PatternCode == "2c") pattern.Rotation += 270;
                if (pattern.PatternCode == "1") pattern.Rotation += 180;
                return new SourcedImage("Highway" + pattern.PatternCode + ".png:" + pattern.Rotation, tooltip) { ZIndex = 2 };
            }
            if (type == TileType.Path)
            {
                Pattern roadpattern = Pattern.Calculate(Grid, x, y, false, TileType.Road, TileType.Path, TileType.Bridge, TileType.Highway, TileType.HighwayBridge, TileType.Garage);
                Pattern parkpattern = Pattern.Calculate(Grid, x, y, true, TileType.Path, TileType.Park);
                Grid[x, y].Pattern = roadpattern;
                if (roadpattern.PatternCode == "2c") roadpattern.Rotation += 270;
                if (roadpattern.PatternCode == "1") roadpattern.Rotation += 180;
                Image path = new SourcedImage("Path" + roadpattern.PatternCode + ".png:" + roadpattern.Rotation, tooltip) { ZIndex = 50 };
                Image park = new SourcedImage("Park" + parkpattern.PatternCode + ".png:" + parkpattern.Rotation) { ZIndex = 25 };

                OCanvas result = new OCanvas();
                result.Children.Add(park);
                result.Children.Add(path);
                return result;
            }
            if (type == TileType.Bridge)
            {
                Pattern roadpattern = Pattern.Calculate(Grid, x, y, false, TileType.Road, TileType.Bridge, TileType.Path, TileType.Highway, TileType.HighwayBridge, TileType.Garage);
                Pattern parkpattern = Pattern.Calculate(Grid, x, y, true, TileType.Bridge, TileType.Lake, TileType.River, TileType.HighwayBridge);
                Grid[x, y].Pattern = roadpattern;
                if (roadpattern.PatternCode == "2c") roadpattern.Rotation += 270;
                if (roadpattern.PatternCode == "1") roadpattern.Rotation += 180;
                Image path = new SourcedImage("Bridge" + roadpattern.PatternCode + ".png:" + roadpattern.Rotation, tooltip) { ZIndex = 50 };
                Image park = new SourcedImage("Lake" + parkpattern.PatternCode + ".png:" + parkpattern.Rotation) { ZIndex = 25 };

                OCanvas result = new OCanvas();
                result.Children.Add(park);
                result.Children.Add(path);
                result.ZIndex = 2;
                return result;
            }
            if (type == TileType.HighwayBridge)
            {
                Pattern roadpattern = Pattern.Calculate(Grid, x, y, false, TileType.Road, TileType.Bridge, TileType.Path, TileType.Highway, TileType.HighwayBridge, TileType.Garage);
                Pattern parkpattern = Pattern.Calculate(Grid, x, y, true, TileType.Bridge, TileType.Lake, TileType.River, TileType.HighwayBridge);
                Grid[x, y].Pattern = roadpattern;
                if (roadpattern.PatternCode == "2c") roadpattern.Rotation += 270;
                if (roadpattern.PatternCode == "1") roadpattern.Rotation += 180;
                Image path = new SourcedImage("HighwayBridge" + roadpattern.PatternCode + ".png:" + roadpattern.Rotation, tooltip) { ZIndex = 50 };
                Image park = new SourcedImage("Lake" + parkpattern.PatternCode + ".png:" + parkpattern.Rotation) { ZIndex = 25 };

                OCanvas result = new OCanvas();
                result.Children.Add(park);
                result.Children.Add(path);
                result.ZIndex = 2;
                return result;
            }
            return new SourcedImage("Error.png", tooltip);
        }
    }
}