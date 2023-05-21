using CityGame.Classes.Rendering;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OrpticonGameHelper;
using OrpticonGameHelper.Classes.Elements;
using System.Threading;
using AStar;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NAudio.Wave;

namespace CityGame.Classes.Entities
{
    public class Car : Entity
    {
        public int grid = 1;
        public bool mightSwitchLane = false;
        public long TimeUntilReroute = 5000;
        protected long RerouteTimePassed = 0;
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
        public static ConcurrentDictionary<Tile, ConcurrentBag<Car>> OccupiedTilesFill = new ConcurrentDictionary<Tile, ConcurrentBag<Car>>();
        public static ConcurrentDictionary<Tile, ConcurrentBag<Car>> OccupiedTiles = new ConcurrentDictionary<Tile, ConcurrentBag<Car>>();
        public static ConcurrentDictionary<Tile, ConcurrentBag<Car>> OccupiedTilesFill2 = new ConcurrentDictionary<Tile, ConcurrentBag<Car>>();
        public static ConcurrentDictionary<Tile, ConcurrentBag<Car>> OccupiedTiles2 = new ConcurrentDictionary<Tile, ConcurrentBag<Car>>();
        protected string PNGFile = "NPCCar.png";
        protected ColoredRectangle debugRect;
        protected Tile lastTile;
        protected List<LightSource> lights = new List<LightSource>();
        protected LightSource PointLight;
        public override OCanvas Render()
        {
            OCanvas canvas = new OCanvas();
            Image car = new SourcedImage(PNGFile);
            car.ZIndex = 99;
            car.Effects.Add(selectedEffect);

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

            PointLight = new LightSource { Type = LightSourceType.PointLight, Color = Color.White, Radius = 24, Angle = 24, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetLeft(PointLight, 43);
            Canvas.SetTop(PointLight, 32);

            canvas.Children.Add(PointLight);

            return canvas;
        }
        public Car()
        {
            Cars.Add(this);
            JourneyFinished += c => { };
            JourneyImpossible += c => { };
            Move += c => { };
        }
        public event CarEvent Move;
        private int curveMode;
        private float curveModePixelDuration;
        private int curveModeStartedAt;
        private bool pathfindingInProgress;
        LaneMode laneMode = LaneMode.Default;
        private long laneSwitchCooldown = 0;
        enum LaneMode
        {
            WrongLane = -1, Parked = 0, Default = 1
        }
        private async Task<Point[]> CalculatePathAsync(Point start, Point target, PathFinder pathfinder)
        {
            return await Task.Run(() =>
            {
                return pathfinder.FindPath(start.Convert(), target.Convert()).Select(x => x.Convert()).ToArray();
            });
        }
        public override async void Tick(long deltaTime)
        {
            visualX = X;
            visualY = Y;
            visualRotation = Rotation;
            UseVisualPosition = true;
            float r = Rotation;
            if (laneMode == LaneMode.WrongLane)
            {
                r = (r + 180) % 360;
                Vector2 offsetVector = new Vector2(0,-1).RotateBy(Rotation - 90) * MainWindow.TileSize / 3;
                visualX += offsetVector.X;
                visualY += offsetVector.Y;
            }
            Tile myTile = MainWindow.Grid[Point.X, Point.Y];
            if (myTile.Type == TileType.Garage)
            {
                Rotation = ((Canvas)myTile.Element).Children[1].Rotation - 90;
                lights.ForEach(x => x.Visible = false);
            }
            else lights.ForEach(x => x.Visible = true);
            if (Target is not null)
            {
                if (pathfindingInProgress) return;
                //if(Object is not null) Object.ToolTip = Target.ToString();
                if (Path is null && !pathfindingInProgress)
                {
                    pathfindingInProgress = true;
                    var pf = MainWindow.pathfinder;
                    if (grid == 2) pf = MainWindow.pathfinderDesperate;

                    Point start = Point;
                    Point target = new Point((int)(Target.X() / MainWindow.TileSize), (int)(Target.Y() / MainWindow.TileSize));

                    Path = await CalculatePathAsync(start, target, pf);
                    NextTarget = 0;
                    pathfindingInProgress = false;
                }
                if (new Point(Target.X() / 64, Target.Y() / 64) != Point) Move(this);
                if (Path is null) return;
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
                    DoLaneBlockades();
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
                Car blockingCar = null;

                DoLaneBlockades();
                if (Math.Round(r) == 0 || Math.Round(r) == 90)
                {
                    if (OccupiedTiles.ContainsKey(targetTile) && OccupiedTiles[targetTile].Any(x => x != this))
                    {
                        SpeedMulti = 0;
                        blockingCar = OccupiedTiles[targetTile].First(x => x != this);
                        if (blockingCar is null) blockingCar = this;
                    }
                }
                if (Math.Round(r) == 180 || Math.Round(r) == 270)
                {
                    if (OccupiedTiles2.ContainsKey(targetTile) && OccupiedTiles2[targetTile].Any(x => x != this))
                    {
                        SpeedMulti = 0;
                        blockingCar = OccupiedTiles2[targetTile].First(x => x != this);
                        if (blockingCar is null) blockingCar = this;
                    }
                }

                laneSwitchCooldown -= (long)Speed * deltaTime;
                if (SpeedMulti == 0 && blockingCar is not null)
                {
                    RerouteTimePassed += deltaTime;
                    if (mightSwitchLane && laneSwitchCooldown <= 0)
                    {
                        laneMode = (LaneMode)((int)laneMode * -1);
                        laneSwitchCooldown = MainWindow.TileSize * 3 * 1000;
                    }
                    if (RerouteTimePassed > TimeUntilReroute)
                    {
                        var resetFunc = MainWindow.UpdatePathfinding(targetTile, 0, 3);
                        blockingCar.Move += resetFunc;
                    }
                }
                else
                {
                    if (laneSwitchCooldown <= 0) laneMode = LaneMode.Default;
                    RerouteTimePassed = 0;
                }

                if (Path is not null && Path.Length > NextTarget + 1 && curveMode == 0)
                {
                    var actPoint = Path[NextTarget + 1];
                    var afterCurveTile = MainWindow.Grid[actPoint.X, actPoint.Y];
                    // North
                    if (Rotation == 0 && afterCurveTile.X == nextTarget.X - 1 && afterCurveTile.Y == nextTarget.Y) curveMode = -1; // Left
                    else if (Rotation == 0 && afterCurveTile.X == nextTarget.X + 1 && afterCurveTile.Y == nextTarget.Y) curveMode = 1; // Right

                    // East
                    else if (Rotation == 90 && afterCurveTile.X == nextTarget.X && afterCurveTile.Y == nextTarget.Y - 1) curveMode = -1; // Left
                    else if (Rotation == 90 && afterCurveTile.X == nextTarget.X && afterCurveTile.Y == nextTarget.Y + 1) curveMode = 1; // Right

                    // South
                    else if (Rotation == 180 && afterCurveTile.X == nextTarget.X + 1 && afterCurveTile.Y == nextTarget.Y) curveMode = -1; // Left
                    else if (Rotation == 180 && afterCurveTile.X == nextTarget.X - 1 && afterCurveTile.Y == nextTarget.Y) curveMode = 1; // Right

                    // West
                    else if (Rotation == 270 && afterCurveTile.X == nextTarget.X && afterCurveTile.Y == nextTarget.Y + 1) curveMode = -1; // Left
                    else if (Rotation == 270 && afterCurveTile.X == nextTarget.X && afterCurveTile.Y == nextTarget.Y - 1) curveMode = 1; // Right

                    if (curveMode != 0)
                    {
                        curveModePixelDuration = MainWindow.TileSize * 2;
                        curveModeStartedAt = NextTarget;
                    }
                }

                if (this is CriminalCar && SpeedMulti > 0 && deltaTime > 0) Debug.WriteLine("running");
                var possibleDistance = Speed * deltaTime / 1000 * SpeedMulti;
                var finalDistance = Math.Min(possibleDistance, travel.Length());
                Vector2 travelFinal = direction * finalDistance;

                if (curveMode != 0)
                {
                    float rotationStart = MainWindow.TileSize * 1.25f;
                    float rotationEnd = MainWindow.TileSize * 0.75f;

                    curveModePixelDuration -= travelFinal.Length();
                    if (curveModePixelDuration <= rotationEnd) curveMode = 0;
                    else
                    {
                        if (curveModePixelDuration < rotationStart && curveModePixelDuration > rotationEnd)
                        {
                            float percentage = (curveModePixelDuration - rotationEnd) / (rotationStart - rotationEnd);
                            if (this == MainWindow.Selected) Debug.WriteLine(percentage);
                            float vRotDeg = 0;
                            if (NextTarget == curveModeStartedAt)
                            {
                                float percentage2 = percentage - 0.5f;
                                percentage2 *= 2;
                                vRotDeg = (45 - 45 * percentage2) * curveMode;
                            }
                            else
                            {
                                float percentage2 = percentage * 2;
                                vRotDeg = (-45 * percentage2) * curveMode;
                            }
                            if (this == MainWindow.Selected) Debug.WriteLine(vRotDeg);
                            visualRotation += vRotDeg;
                        }
                        else
                        {
                        }
                    }
                }

                X += travelFinal.X;
                Y += travelFinal.Y;
            }
            else
            {
                DoLaneBlockades();
            }
        }
        public void DoLaneBlockades()
        {
            Tuple<TileType, string>[] fullBlockTiles = new Tuple<TileType, string>[]
            {
                new Tuple<TileType, string>(TileType.Road, "4c"),
                new Tuple<TileType, string>(TileType.Road, "3c"),
                new Tuple<TileType, string>(TileType.Road, "1"),
                new Tuple<TileType, string>(TileType.Garage, null)
            };
            //if (this == MainWindow.Selected) Debug.WriteLine("Selected.");
            Tile myTile = MainWindow.Grid[Point.X, Point.Y];
            bool fullBlock = fullBlockTiles.Any(x => (x.Item1 == myTile.Type || (x.Item1 == TileType.Road && (myTile.Type == TileType.Path || myTile.Type == TileType.Highway || myTile.Type == TileType.Bridge || myTile.Type == TileType.HighwayBridge))) && (x.Item2 == myTile.Pattern.PatternCode || x.Item2 is null));
            if (laneMode == LaneMode.Parked) return;
            float r = Rotation;
            if (laneMode == LaneMode.WrongLane) r = (r + 180) % 360;
            if (Math.Round(r) == 0 || Math.Round(r) == 90)
            {
                if (!OccupiedTilesFill.ContainsKey(myTile)) OccupiedTilesFill.WeirdAddToList(myTile, this);
                if (!OccupiedTilesFill2.ContainsKey(myTile) && fullBlock) OccupiedTilesFill2.WeirdAddToList(myTile, this);
            }
            if (Math.Round(r) == 180 || Math.Round(r) == 270)
            {
                if (!OccupiedTilesFill2.ContainsKey(myTile)) OccupiedTilesFill2.WeirdAddToList(myTile, this);
                if (!OccupiedTilesFill.ContainsKey(myTile) && fullBlock) OccupiedTilesFill.WeirdAddToList(myTile, this);
            }
        }
    }
}