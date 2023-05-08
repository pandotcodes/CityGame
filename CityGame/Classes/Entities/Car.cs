using CityGame.Classes.Rendering;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using WPFGame;

namespace CityGame.Classes.Entities
{
    public class Car : Entity
    {
        public bool desperate = false;
        public static List<Car> Cars = new List<Car>();
        public delegate void CarEvent(Car car);
        public event CarEvent JourneyFinished;
        public event CarEvent JourneyImpossible;
        ISelectable target = null;
        public ISelectable Target { get { return target; } set { target = value; Path = null; } }
        public int NextTarget { get; set; } = 0;
        public Point[]? Path { get; set; } = null;
        public Point Point
        {
            get
            {
                return new Point((int)X / MainWindow.TileSize, (int)Y / MainWindow.TileSize);
            }
            set
            {
                X = (float)value.X * MainWindow.TileSize;
                Y = (float)value.Y * MainWindow.TileSize;
            }
        }
        public float Speed { get; set; } = 128;
        float currentSpeed = 0;
        public static Dictionary<Tile, Car> OccupiedTilesFill = new Dictionary<Tile, Car>();
        public static Dictionary<Tile, Car> OccupiedTiles = new Dictionary<Tile, Car>();
        public static Dictionary<Tile, Car> OccupiedTilesFill2 = new Dictionary<Tile, Car>();
        public static Dictionary<Tile, Car> OccupiedTiles2 = new Dictionary<Tile, Car>();
        protected string PNGFile = "NPCCar.png";
        protected ColoredRectangle debugRect;
        protected Tile lastTile;
        protected List<LightSource> lights = new List<LightSource>();
        public override OCanvas Render()
        {
            OCanvas canvas = new OCanvas();
            Image car = new SourcedImage(PNGFile);

            canvas.Children.Add(car);
            var light = new LightSource { Radius = 128, Angle = 64, Intensity = 2, Color = Color.White, Type = LightSourceType.Spotlight, Rotation = -90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            var light2 = new LightSource { Radius = 128, Angle = 64, Intensity = 2, Color = Color.White, Type = LightSourceType.Spotlight, Rotation = -90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetLeft(light, 39);
            Canvas.SetTop(light, 19);
            Canvas.SetLeft(light2, 46);
            Canvas.SetTop(light2, 19);
            canvas.Children.Add(light);
            canvas.Children.Add(light2);

            var blight = new LightSource { Radius = 12, Angle = 12, Intensity = 0.5f, Color = Color.Red, Type = LightSourceType.PointLight, Rotation = 90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            var blight2 = new LightSource { Radius = 12, Angle = 12, Intensity = 0.5f, Color = Color.Red, Type = LightSourceType.PointLight, Rotation = 90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetLeft(blight, 39);
            Canvas.SetTop(blight, 46);
            Canvas.SetLeft(blight2, 46);
            Canvas.SetTop(blight2, 46);
            canvas.Children.Add(blight);
            canvas.Children.Add(blight2);

            lights.Add(light);
            lights.Add(light2);
            lights.Add(blight);
            lights.Add(blight2);

            return canvas;
        }
        public Car()
        {
            Cars.Add(this);
            JourneyFinished += c => { };
            JourneyImpossible += c => { };
        }

        public override void Tick(long deltaTime)
        {
            //deltaTime *= 500;
            Tuple<TileType, string>[] fullBlockTiles = new Tuple<TileType, string>[]
            {
                new Tuple<TileType, string>(TileType.Road, "4c"),
                new Tuple<TileType, string>(TileType.Road, "3c"),
                new Tuple<TileType, string>(TileType.Road, "1"),
                new Tuple<TileType, string>(TileType.Garage, null)
            };
            Tile myTile = MainWindow.Grid[Point.X, Point.Y];
            bool fullBlock = fullBlockTiles.Any(x => (x.Item1 == myTile.Type || (x.Item1 == TileType.Road && (myTile.Type == TileType.Path || myTile.Type == TileType.Highway || myTile.Type == TileType.Bridge || myTile.Type == TileType.HighwayBridge))) && (x.Item2 == myTile.Pattern.PatternCode || x.Item2 is null));
            if (myTile.Type == TileType.Garage)
            {
                Rotation = ((Canvas)myTile.Element).Children[1].Rotation-90;
                lights.ForEach(x => x.Visible = false);
            }
            else lights.ForEach(x => x.Visible = true);
            if (Target is not null)
            {
                //if(Object is not null) Object.ToolTip = Target.ToString();
                if (Path is null)
                {
                    var pf = MainWindow.pathfinder;
                    if (desperate) pf = MainWindow.pathfinderDesperate;
                    Path = pf.FindPath(Point.Convert(), new Point((int)(Target.X() / MainWindow.TileSize), (int)(Target.Y() / MainWindow.TileSize)).Convert()).Select(x => x.Convert()).ToArray();
                    NextTarget = 0;
                }
                if (Path.Length == 0)
                {
                    JourneyImpossible(this);
                    return;
                }
                Point nextTarget = Path[NextTarget];
                if (X.CloselyEquals(nextTarget.X * MainWindow.TileSize) && Y.CloselyEquals(nextTarget.Y * MainWindow.TileSize))
                {
                    lastTile = myTile;
                    NextTarget++;
                }
                if (NextTarget == Path.Length)
                {
                    Path = null;
                    NextTarget = 0;
                    JourneyFinished(this);
                    if (Math.Round(Rotation) == 0 || Math.Round(Rotation) == 90)
                    {
                        if (!OccupiedTilesFill.ContainsKey(myTile)) OccupiedTilesFill.Add(myTile, this);
                        if (!OccupiedTilesFill2.ContainsKey(myTile) && fullBlock) OccupiedTilesFill2.Add(myTile, this);
                    }
                    if (Math.Round(Rotation) == 180 || Math.Round(Rotation) == 270)
                    {
                        if (!OccupiedTilesFill2.ContainsKey(myTile)) OccupiedTilesFill2.Add(myTile, this);
                        if (!OccupiedTilesFill.ContainsKey(myTile) && fullBlock) OccupiedTilesFill.Add(myTile, this);
                    }
                    return;
                }
                if (X.CloselyEquals(nextTarget.X * MainWindow.TileSize) && Y.CloselyEquals(nextTarget.Y * MainWindow.TileSize))
                    return;
                float SpeedMulti = 1;

                if (myTile.Type == TileType.Highway || myTile.Type == TileType.HighwayBridge) SpeedMulti = 2;

                Vector2 travel = new Vector2((float)nextTarget.X * 64 - X, (float)nextTarget.Y * 64 - Y);
                Vector2 direction = Vector2.Normalize(travel);

                float degrees = (float)(Math.Atan2(direction.Y, direction.X) * (180 / Math.PI)) + 90;
                Rotation = degrees;
                Tile targetTile = MainWindow.Grid[nextTarget.X, nextTarget.Y];

                if (Math.Round(Rotation) == 0 || Math.Round(Rotation) == 90)
                {
                    if (!OccupiedTilesFill.ContainsKey(myTile)) OccupiedTilesFill.Add(myTile, this);
                    if (!OccupiedTilesFill2.ContainsKey(myTile) && fullBlock) OccupiedTilesFill2.Add(myTile, this);
                    if (OccupiedTiles.ContainsKey(targetTile) && OccupiedTiles[targetTile] != this) SpeedMulti = 0;
                }
                if (Math.Round(Rotation) == 180 || Math.Round(Rotation) == 270)
                {
                    if (!OccupiedTilesFill2.ContainsKey(myTile)) OccupiedTilesFill2.Add(myTile, this);
                    if (OccupiedTiles2.ContainsKey(targetTile) && OccupiedTiles2[targetTile] != this) SpeedMulti = 0;
                    if (!OccupiedTilesFill.ContainsKey(myTile) && fullBlock) OccupiedTilesFill.Add(myTile, this);
                }

                var possibleDistance = Speed * deltaTime / 1000 * SpeedMulti;
                var finalDistance = Math.Min(possibleDistance, travel.Length());
                Vector2 travelFinal = direction * finalDistance;
                X += travelFinal.X;
                Y += travelFinal.Y;
            } else
            {
                if (Math.Round(Rotation) == 0 || Math.Round(Rotation) == 90)
                {
                    if (!OccupiedTilesFill.ContainsKey(myTile)) OccupiedTilesFill.Add(myTile, this);
                    if (!OccupiedTilesFill2.ContainsKey(myTile) && fullBlock) OccupiedTilesFill2.Add(myTile, this);
                }
                if (Math.Round(Rotation) == 180 || Math.Round(Rotation) == 270)
                {
                    if (!OccupiedTilesFill2.ContainsKey(myTile)) OccupiedTilesFill2.Add(myTile, this);
                    if (!OccupiedTilesFill.ContainsKey(myTile) && fullBlock) OccupiedTilesFill.Add(myTile, this);
                }
            }
        }
    }
}