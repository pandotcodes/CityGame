﻿using AStar;
using AStar.Options;
using CityGame.Classes.Entities;
using CityGame.Classes.Rendering;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using SimplexNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using CityGame.Classes.Rendering.Particles;
using CityGame.Classes.Rendering.Shaders;
using OrpticonGameHelper;
using OrpticonGameHelper.Classes.Elements;
using static CityGame.Classes.Entities.Car;
using OrpticonGameHelper.Classes.Misc;
using System.Diagnostics;

namespace CityGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ISelectable GetSelectableFromClick(MouseState click)
        {
            Vector2 point = new Vector2(click.X, click.Y);

            point = Vector2.Transform(point, InvCamera);

            foreach (Entity entity in Entities)
            {
                double diff = new Vector2((int)entity.X + TileSize / 2 - point.X, (int)entity.Y + TileSize / 2 - point.Y).Length();
                if (diff < TileSize / 2) return entity;
            }
            int x = (int)(point.X / TileSize);
            int y = (int)(point.Y / TileSize);
            if (x < 0 || y < 0) return null;
            if (x > Grid.GetLength(0) - 1 || y > Grid.GetLength(1) - 1) return null;

            //Debug.WriteLine(Grid[x, y]);
            return Grid[x, y];
        }
        public static Entity GetEntityFromImage(Image image)
        {
            foreach (Entity entity in Entities)
            {
                if (entity.Object == image.Parent) return entity;
            }
            return null;
        }
        public static Tile? GetTileFromImage(Image image)
        {
            double x = Canvas.GetLeft(image.Parent) / TileSize;
            double y = Canvas.GetTop(image.Parent) / TileSize;
            if (x % 1 != 0 || y % 1 != 0) return null;
            return Grid[(int)x, (int)y];
        }
        public static Random random;
        public static bool MouseIsDown = false;
        public static Vector2 MousePos = new Vector2(0, 0);
        public static PathFinder pathfinder;
        public static PathFinder pathfinderDesperate;
        public static Dictionary<int, short[,]> Grids = new Dictionary<int, short[,]>();
        public static Dictionary<int, WorldGrid> WorldGrids = new Dictionary<int, WorldGrid>();
        public static List<Tile> npcWalkable = new List<Tile>();
        public static List<Entity> Entities { get; set; }
        public const int TileSize = 64;
        public static Tile[,] Grid;
        public static ISelectable Selected;
        public static OCanvas[,] ImageGrid;
        public static Vector2 Wind = new Vector2(0.5f, -0.5f);

        Canvas MainCanvas = new OCanvas();
        Canvas BGCanvas = new OCanvas();
        internal static Canvas GameCanvas = new OCanvas();
        Canvas CameraCanvas = new OCanvas();
        Canvas UICanvas = new OCanvas();
        public static Tile FindNearestTile(Tile startPoint, short[,] pfGrid, params Tile[] targets)
        {
            var t = FindNearestTarget(new IntPoint(startPoint.X, startPoint.Y), pfGrid, targets.Select(x => new IntPoint(x.X, x.Y)).ToArray());
            if (t.X == -1 && t.Y == -1) return new Tile { Type = TileType.Invalid };
            return Grid[t.X, t.Y];
        }
        public static Tile FindNearestTile(Tile startPoint, short[,] pfGrid, TileType targetTile)
        {
            var targets = new List<Tile>();
            for (int i = 0; i < Grid.Length; i++)
            {
                if (Grid[i % Grid.GetLength(0), i / Grid.GetLength(0)].Type == targetTile) targets.Add(Grid[i % Grid.GetLength(0), i / Grid.GetLength(0)]);
            }
            return FindNearestTile(startPoint, pfGrid, targets.ToArray());
        }
        public static IntPoint FindNearestTarget(IntPoint startPoint, short[,] pfGrid, params IntPoint[] targets)
        {
            var directions = new Vector2[] { new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0) };
            IntPoint result = new IntPoint(0, 0);
            bool resultFound = false;
            var searchers = new List<IntPoint>();
            var processed = new List<IntPoint>();
            searchers.Add(startPoint);
            while (!resultFound)
            {
                if (searchers.Count == 0) return new IntPoint(-1, -1);
                var s = searchers.ToList();
                searchers = new List<IntPoint>();
                foreach (var searcher in s)
                {
                    if (targets.Contains(searcher))
                    {
                        result = searcher;
                        resultFound = true;
                        break;
                    }
                    foreach (var direction in directions)
                    {
                        var p = searcher + direction;
                        if (p.X < 0 || p.Y < 0) continue;
                        if (p.X >= pfGrid.GetLength(1)) continue;
                        if (p.Y >= pfGrid.GetLength(0)) continue;
                        if (processed.Contains(p)) continue;
                        if (pfGrid[p.Y, p.X] == 0) continue;
                        processed.Add(p);
                        searchers.Add(p);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Updates the pathfinding grids of all grid numbers included in the "grids" parameter
        /// </summary>
        /// <param name="tile">The tile to update</param>
        /// <param name="value">The value to update the tile to</param>
        /// <param name="grids">The grids to perform the update for</param>
        /// <returns>A function that should be called once the reason for the update has resolved</returns>
        public static CarEvent UpdatePathfinding(Tile tile, short value, int grids = 3, params Car[] exclude)
        {
            Dictionary<int, short> resetValues = new Dictionary<int, short>();
            foreach (var kvp in Grids)
            {
                if ((grids & kvp.Key) == kvp.Key)
                {
                    resetValues[kvp.Key] = kvp.Value[tile.Y, tile.X];
                    kvp.Value[tile.Y, tile.X] = value;
                    WorldGrids[kvp.Key][tile.Y, tile.X] = value;
                }
            }

            bool hasExecuted = false;
            object lockObject = new object();
            CarEvent resetFunc = (Car exclude) =>
            {
                lock (lockObject)
                {
                    if (!hasExecuted)
                    {
                        hasExecuted = true;

                        foreach (var kvp in Grids)
                        {
                            if ((grids & kvp.Key) == kvp.Key)
                            {
                                UpdatePathfinding(tile, resetValues[kvp.Key], kvp.Key, exclude);
                            }
                        }
                    }
                }
            };

            InstantiatePathfinders();
            Car.Cars.ForEach(car => car.Path = !exclude.Contains(car) && (grids & car.grid) == car.grid ? null : car.Path);

            return resetFunc;
        }
        public static void InstantiatePathfinders()
        {
            pathfinder = new PathFinder(WorldGrids[1], new PathFinderOptions { PunishChangeDirection = true, UseDiagonals = false, SearchLimit = int.MaxValue, HeuristicFormula = AStar.Heuristics.HeuristicFormula.Euclidean });
            pathfinderDesperate = new PathFinder(WorldGrids[2], new PathFinderOptions { PunishChangeDirection = true, UseDiagonals = false, SearchLimit = int.MaxValue, HeuristicFormula = AStar.Heuristics.HeuristicFormula.Euclidean });
        }

        public static AudioListener SoundEffectListener;
        public MainWindow()
        {
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            AddPenumbra();

            #region | Texture Conversions |
            string[] patternCodes = new[] { "0", "1", "2", "2c", "3", "3a", "3ab", "3c", "4", "4m", "4c", "5", "7", "8" };

            foreach (var code in patternCodes)
            {
                ImageConverter.ChangeColor("Building" + code, "Lake" + code, ColorConversionMaps.HouseToLakeMap);
                ImageConverter.ChangeColor("Lake" + code, "River" + code, ColorConversionMaps.LakeToRiver);
                ImageConverter.ChangeColor("Building" + code, "BuildingDark" + code, ColorConversionMaps.HouseToBuildingDarkMap);
                ImageConverter.ChangeColor("Building" + code, "BuildingBlue" + code, ColorConversionMaps.HouseToBuildingBlueMap);
                ImageConverter.ChangeColor("Building" + code, "BuildingRed" + code, ColorConversionMaps.HouseToBuildingRedMap);
                ImageConverter.ChangeColor("Building" + code, "BuildingGreen" + code, ColorConversionMaps.HouseToBuildingGreenMap);
                ImageConverter.ChangeColor("Building" + code, "Park" + code, ColorConversionMaps.HouseToParkMap);
                ImageConverter.ChangeColor("Road" + code, "Path" + code, ColorConversionMaps.RoadToPathMap);
                ImageConverter.ChangeColor("Road" + code, "Bridge" + code, ColorConversionMaps.RoadToBridgeMap);
                ImageConverter.ChangeColor("Road" + code, "Highway" + code, ColorConversionMaps.RoadToHighwayMap);
                ImageConverter.ChangeColor("Road" + code, "HighwayBridge" + code, ColorConversionMaps.RoadToHighwayMap);
            }
            ImageConverter.ChangeColor("ParkingLot", "ParkingLotDark", ColorConversionMaps.HouseToBuildingDarkMap);
            ImageConverter.ChangeColor("ParkingLot", "ParkingLotBlue", ColorConversionMaps.HouseToBuildingBlueMap);
            ImageConverter.ChangeColor("ParkingLot", "ParkingLotRed", ColorConversionMaps.HouseToBuildingRedMap);
            ImageConverter.ChangeColor("ParkingLot", "ParkingLotGreen", ColorConversionMaps.HouseToBuildingGreenMap);

            ImageConverter.ChangeColor("Error", "ErrorRed", new Dictionary<string, string> { { "#000000", "#ff0000" } });
            ImageConverter.ChangeColor("Car", "NPCCar", ColorConversionMaps.CarToNPCCar);
            ImageConverter.ChangeColor("Car", "PoliceCar", ColorConversionMaps.CarToPoliceCar);
            ImageConverter.ChangeColor("Car", "CaughtCar", ColorConversionMaps.CarToCaughtCar);
            ImageConverter.ChangeColor("Car", "CriminalCar", new Dictionary<string, string>());
            #endregion

            LoadableContent.Add(ShaderLoader.Create());

            int seed = (int)(DateTime.Now.Ticks % int.MaxValue);

            Noise.Seed = seed;

            int mapHeight = 50;
            int mapWidth = 50;

            float[,] lakeMap = Noise.Calc2D(mapWidth, mapHeight, 0.05f);

            Noise.Seed = seed * 24 + 1;
            float[,] parkMap = Noise.Calc2D(mapWidth, mapHeight, 0.05f);

            Noise.Seed = Noise.Seed * 24 + 1;
            float[,] riverMap = Noise.Calc2D(mapWidth, mapHeight, 0.01f);

            mapHeight /= 2;
            mapWidth /= 2;

            int doubleHeight = mapHeight * 2;
            int doubleWidth = mapWidth * 2;

            int maxBlockHeight = 5;
            int maxBlockWidth = 5;
            int minBlockHeight = 3;
            int minBlockWidth = 3;

            int NPCCount = (int)Math.Ceiling(mapHeight * mapWidth / 100f);
            int CriminalCount = 1;
            //NPCCount = 1;
            //NPCCount = 0;
            //NPCCount /= 2;
            //NPCCount *= 8;

            random = new Random(seed);

            #region | Map Initialization |
            Tile[,] InitialGrid = new Tile[mapWidth, mapHeight];

            List<Tuple<int, int>> coords = new List<Tuple<int, int>>();
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    coords.Add(new Tuple<int, int>(x, y));
                }
            }

            coords = coords.OrderBy(x => random.Next(0, coords.Count)).ToList();

            #endregion

            #region | Main Algorithm |

            int i = 0;
            foreach (var coord in coords)
            {
                int x = coord.Item1;
                int y = coord.Item2;

                int height = random.Next(minBlockHeight, maxBlockHeight + 1);
                int width = random.Next(minBlockWidth, maxBlockWidth + 1);
                int yOffset = random.Next(0, height);
                int xOffset = random.Next(0, width);

                TileType BlockType = TileType.Skyscraper;

                for (int y2 = y - yOffset; y2 < y - yOffset + height; y2++)
                {
                    for (int x2 = x - xOffset; x2 < x - xOffset + width; x2++)
                    {
                        if (x2 < 0) continue;
                        if (y2 < 0) continue;
                        if (x2 >= mapWidth) continue;
                        if (y2 >= mapHeight) continue;
                        InitialGrid[x2, y2].BlockID = i;
                        InitialGrid[x2, y2].Type = BlockType;
                        if (lakeMap[x2, y2] > 214) InitialGrid[x2, y2].Type = TileType.Park;
                        if (lakeMap[x2, y2] > 219) InitialGrid[x2, y2].Type = TileType.Lake;
                        if (parkMap[x2, y2] > 214 && parkMap[x2, y2] > lakeMap[x2, y2]) InitialGrid[x2, y2].Type = TileType.Park;
                        if (riverMap[x2, y2] > 128 && riverMap[x2, y2] < 148) InitialGrid[x2, y2].Type = TileType.River;


                    }
                }

                i++;
            }

            #endregion

            #region | Doubling |

            Tile[,] IntermediateGrid = new Tile[doubleWidth, doubleHeight];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    IntermediateGrid[x * 2, y * 2] = InitialGrid[x, y];
                    IntermediateGrid[x * 2, y * 2 + 1] = InitialGrid[x, y];
                    IntermediateGrid[x * 2 + 1, y * 2 + 1] = InitialGrid[x, y];
                    IntermediateGrid[x * 2 + 1, y * 2] = InitialGrid[x, y];

                    IntermediateGrid[x * 2, y * 2].X = x * 2;
                    IntermediateGrid[x * 2, y * 2].Y = y * 2;

                    IntermediateGrid[x * 2, y * 2 + 1].X = x * 2;
                    IntermediateGrid[x * 2, y * 2 + 1].Y = y * 2 + 1;

                    IntermediateGrid[x * 2 + 1, y * 2 + 1].X = x * 2 + 1;
                    IntermediateGrid[x * 2 + 1, y * 2 + 1].Y = y * 2 + 1;

                    IntermediateGrid[x * 2 + 1, y * 2].X = x * 2 + 1;
                    IntermediateGrid[x * 2 + 1, y * 2].Y = y * 2;
                }
            }
            Dictionary<int, bool> decidedBridges = new Dictionary<int, bool>();
            Grids.Add(1, new short[doubleHeight, doubleWidth]);
            Grids.Add(2, new short[doubleHeight, doubleWidth]);
            List<Tile> bridgeTiles = new List<Tile>();
            List<Tile> roadTiles = new List<Tile>();
            for (int y = 0; y < doubleHeight; y++)
            {
                for (int x = 0; x < doubleWidth; x++)
                {
                    TileType changeTo = TileType.Road;
                    int myID = IntermediateGrid[x, y].BlockID;
                    if (IntermediateGrid[x, y].Type == TileType.Lake || IntermediateGrid[x, y].Type == TileType.River)
                    {
                        if (!decidedBridges.ContainsKey(myID))
                        {
                            decidedBridges[myID] = random.Next(0, 2) == 0;
                        }
                        if (decidedBridges[myID])
                        {
                            changeTo = TileType.Bridge;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (IntermediateGrid[x, y].Type == TileType.Park) changeTo = TileType.Path;
                    if (x < doubleWidth - 1 && IntermediateGrid[x + 1, y].BlockID != myID) IntermediateGrid[x, y].Type = changeTo;
                    if (y < doubleHeight - 1 && IntermediateGrid[x, y + 1].BlockID != myID) IntermediateGrid[x, y].Type = changeTo;
                    var type = IntermediateGrid[x, y].Type;
                    if (type == TileType.Bridge) bridgeTiles.Add(IntermediateGrid[x, y]);
                    if (type == TileType.Road) roadTiles.Add(IntermediateGrid[x, y]);
                    bool walkable = ((int)type) / 100 == 4;
                    Grids[2][y, x] = (short)(walkable ? 1 : 0);
                    if (type == TileType.Path) walkable = false;
                    Grids[1][y, x] = (short)(walkable ? 1 : 0);
                }
            }
            bool checkBridges = true;
            if (checkBridges)
            {
                foreach (var tile in bridgeTiles)
                {
                    IntPoint road = FindNearestTarget(new IntPoint(tile.X, tile.Y), Grids[1], roadTiles.Select(x => new IntPoint(x.X, x.Y)).ToArray());
                    if (road.X == -1 && road.Y == -1) IntermediateGrid[tile.X, tile.Y].Type = TileType.Lake;
                }
            }

            int mainRoadCount = random.Next(1, 5);
            for (int m = 0; m < mainRoadCount; m++)
            {
                int variant = random.Next(0, 2);
                IntPoint startPoint = new IntPoint(0, 0);
                IntPoint endPoint = new IntPoint(0, 0);
                IntPoint step = new IntPoint(0, 0);
                if (variant == 0)
                {
                    int x = 0;
                    int y = random.Next(0 + 2, doubleHeight - 2);
                    startPoint = new IntPoint(x, y);
                    endPoint = new IntPoint(doubleWidth, y);
                    step = new IntPoint(1, 0);
                }
                else
                {
                    int x = random.Next(0 + 2, doubleWidth - 2);
                    int y = 0;
                    startPoint = new IntPoint(x, y);
                    endPoint = new IntPoint(x, doubleHeight);
                    step = new IntPoint(0, 1);
                }
                while (startPoint != endPoint)
                {
                    int x = (int)startPoint.X;
                    int y = (int)startPoint.Y;
                    var tile = IntermediateGrid[x, y];
                    IntermediateGrid[x, y].Type = tile.Type == TileType.Lake || tile.Type == TileType.River ? TileType.HighwayBridge : TileType.Highway;
                    var topRoad = IntermediateGrid[x - step.Y, y - step.X];
                    var checkTopRoad = IntermediateGrid[x - step.Y * 2, y - step.X * 2];
                    var bottomRoad = IntermediateGrid[x + step.Y, y + step.X];
                    var checkBottomRoad = IntermediateGrid[x + step.Y * 2, y + step.X * 2];

                    if (((int)topRoad.Type) / 100 == 4 && ((int)checkTopRoad.Type) / 100 != 4) IntermediateGrid[x - step.Y, y - step.X].Type = topRoad.Type == TileType.Road ? TileType.Skyscraper : topRoad.Type == TileType.Path ? TileType.Park : TileType.Lake;
                    if (((int)bottomRoad.Type) / 100 == 4 && ((int)checkBottomRoad.Type) / 100 != 4) IntermediateGrid[x + step.Y, y + step.X].Type = TileType.Skyscraper;

                    if (((int)topRoad.Type) % 100 / 10 == 1 && ((int)bottomRoad.Type) % 100 / 10 == 1) IntermediateGrid[x, y].Type = TileType.HighwayBridge;

                    startPoint += step;
                }
            }
            for (int y = 0; y < doubleHeight; y++)
            {
                for (int x = 0; x < doubleWidth; x++)
                {
                    var type = IntermediateGrid[x, y].Type;
                    bool walkable = ((int)type) / 100 == 4;
                    Grids[2][y, x] = (short)(walkable ? 1 : 0);
                    if (type == TileType.Path) walkable = false;
                    Grids[1][y, x] = (short)(walkable ? 1 : 0);
                    if (type == TileType.Road || type == TileType.Bridge) npcWalkable.Add(IntermediateGrid[x, y]);
                }
            }

            Grid = IntermediateGrid;
            #endregion
            //for(int y = 0; y < mapHeight; y++)
            //{
            //    for(int x = 0; x < mapWidth; x++)
            //    {
            //        Grid[x, y] = IntermediateGrid[x * 2 + 1, y * 2 + 1];
            //    }
            //}

            mapHeight *= 2;
            mapWidth *= 2;

            ImageGrid = new OCanvas[doubleWidth, doubleHeight];

            Canvas.SetLeft(CameraCanvas, 0);
            Canvas.SetTop(CameraCanvas, 0);

            MainCanvas.Children.Add(CameraCanvas);
            MainCanvas.Children.Add(UICanvas);

            CameraCanvas.Children.Add(BGCanvas);
            CameraCanvas.Children.Add(GameCanvas);

            ContentCanvas = MainCanvas;

            int tileSize = TileSize;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Canvas image = Renderer.Render(Grid[x, y].Type, x, y, Grid);
                    Grid[x, y].Element = image;

                    Canvas.SetLeft(image, x * tileSize);
                    Canvas.SetTop(image, y * tileSize);

                    ImageGrid[x, y] = (OCanvas)image;

                    //image.Opacity = pathfindingGrid[y, x];

                    GameCanvas.Children.Add(image);
                }
            }

            foreach (var kvp in Grids) WorldGrids.Add(kvp.Key, new WorldGrid(kvp.Value));
            InstantiatePathfinders();

            Entities = new List<Entity>();

            foreach (Image image in SourcedImage.GetObjectsBySourceFile("Helipad.png"))
            {
                float x = (float)Canvas.GetLeft(image.Parent);
                float y = (float)Canvas.GetTop(image.Parent);

                Entities.Add(new Helicopter { X = x, Y = y });
            }

            foreach (Image image in SourcedImage.GetObjectsBySourceFile("Garage.png"))
            {
                float x = (float)Canvas.GetLeft(image.Parent);
                float y = (float)Canvas.GetTop(image.Parent);

                Entities.Add(new PoliceCar { X = x, Y = y });
            }
            var roads = SourcedImage.GetObjectsBySourceFile("Road1.png", "Road2.png", "Road2c.png", "Road3c.png", "Road4c.png");
            var pipeCount = NPCCount * 2;
            var pipeRoads = roads.OrderBy(x => random.Next(0, roads.Count)).Take(pipeCount).ToArray();
            foreach (Image image in pipeRoads)
            {
                Entities.Add(new GasPipe { X = (float)Canvas.GetLeft(image.Parent), Y = (float)Canvas.GetTop(image.Parent), Rotation = image.Rotation });
            }

            for (int n = 0; n < NPCCount; n++)
            {
                Car car = new Car();
                Tile startTile = npcWalkable[random.Next(0, npcWalkable.Count)];
                Tile targetTile = npcWalkable[random.Next(0, npcWalkable.Count)];

                car.Point = new Point(startTile.X, startTile.Y);

                car.Target = targetTile;

                Car.CarEvent reset = car =>
                {
                    Tile targetTile = npcWalkable[random.Next(0, npcWalkable.Count)];
                    car.Target = targetTile;
                };

                car.JourneyFinished += reset;
                car.JourneyImpossible += reset;

                Entities.Add(car);
            }
            for (int n = 0; n < CriminalCount; n++)
            {
                CriminalCar car = new CriminalCar();
                bool foundStartPoint = false;
                Tile startTile = Grid[0, 0];
                while(!foundStartPoint)
                {
                    startTile = npcWalkable[random.Next(0, npcWalkable.Count)];
                    if (startTile.X > doubleWidth / 3 && startTile.Y > doubleHeight / 3 && startTile.X < doubleWidth / 3 * 2 && startTile.Y < doubleHeight / 3 * 2) foundStartPoint = true;
                }

                car.Point = new Point(startTile.X, startTile.Y);
                Entities.Add(car);
            }

            SoundEffectListener = new AudioListener { Position = new Vector3(0, 0, 100) };

            Show();
        }
        int swv;
        public bool PauseEntities = true;
        protected override Color SkyColor(long SpeedFactor)
        {
            return Color.Gray;
        }
        protected override void Update(GameTime time)
        {
            MouseState state = Mouse.GetState();
            KeyboardState kstate = Keyboard.GetState();

            if (state.MiddleButton == ButtonState.Pressed)
            {
                var newpos = new Vector2(state.X, state.Y);
                var diff = newpos - MousePos;
                diff /= CameraZoom;
                CameraPosition += diff;
                SoundEffectListener.Position = new Vector3(CameraPosition, SoundEffectListener.Position.Z);
            }
            MousePos = new Vector2(state.X, state.Y);


            float delta = state.ScrollWheelValue - swv;
            swv = state.ScrollWheelValue;
            if (delta != 0)
            {
                //Debug.WriteLine(delta);
            }
            float multi = 1.05f;
            if (delta < 0) multi = 1 / multi;

            if (delta != 0)
            {
                if (multi > 1) SoundEffectListener.Position = new Vector3(SoundEffectListener.Position.X, SoundEffectListener.Position.Y, SoundEffectListener.Position.Z - 10);
                else SoundEffectListener.Position = new Vector3(SoundEffectListener.Position.X, SoundEffectListener.Position.Y, SoundEffectListener.Position.Z + 10);
                CameraZoom *= multi;
            }

            if (state.LeftButton == ButtonState.Pressed)
            {
                ISelectable select = GetSelectableFromClick(state);
                if (select is not null)
                {
                    if (select.IsSingleSelect()) select.RunAction(select);
                    else Selected = select;
                }
            }
            else if (state.RightButton == ButtonState.Pressed)
            {
                ISelectable select = GetSelectableFromClick(state);
                if (select is not null)
                {
                    Selected.RunAction(select);
                }
            }
            if (kstate.IsKeyDown(Keys.Space)) PauseEntities = !PauseEntities;
            PauseParticles = PauseEntities;

            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            foreach (Entity entity in Entities)
            {
                long deltaTime = milliseconds - entity.Time;
                deltaTime = (long)time.ElapsedGameTime.TotalMilliseconds;
                if (PauseEntities) deltaTime = 0;
                entity.Time = milliseconds;
                entity.Tick(deltaTime);
                entity.BaseTick(deltaTime);

                if (entity.Object is null)
                {
                    entity.Object = entity.Render();
                    GameCanvas.Children.Add(entity.Object);
                }
                entity.Object.Rotation = (int)entity.VisualRotation;
                Canvas.SetLeft(entity.Object, entity.VisualX);
                Canvas.SetTop(entity.Object, entity.VisualY);
            }
            Car.OccupiedTiles = Car.OccupiedTilesFill;
            Car.OccupiedTilesFill = new System.Collections.Concurrent.ConcurrentDictionary<Tile, System.Collections.Concurrent.ConcurrentBag<Car>>();
            Car.OccupiedTiles2 = Car.OccupiedTilesFill2;
            Car.OccupiedTilesFill2 = new System.Collections.Concurrent.ConcurrentDictionary<Tile, System.Collections.Concurrent.ConcurrentBag<Car>>();
        }
    }
}