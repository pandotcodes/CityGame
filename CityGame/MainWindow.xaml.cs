using AStar;
using AStar.Options;
using SimplexNoise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CityGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Random random;
        public static bool MouseIsDown = false;
        public static Point MousePos = new Point(0, 0);
        public static PathFinder pathfinder;
        public static short[,] pathfindingGrid;
        public static short[,] pathfindingGridDesperate;
        public static List<Tile> npcWalkable = new List<Tile>();
        public static List<Entity> Entities { get; set; } = new List<Entity>();
        public const int TileSize = 64;
        public MainWindow()
        {
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
            #endregion

            InitializeComponent();

            #region | Map Generation |
            #region | Map Generation Constants |
            int seed = 3;

            Noise.Seed = seed;

            int mapHeight = 100;
            int mapWidth = 100;

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

            int NPCCount = 1;

            random = new Random(seed);

            #endregion

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
            #endregion

            #region | Roads and Bridges |
            Dictionary<int, bool> decidedBridges = new Dictionary<int, bool>();
            pathfindingGrid = new short[doubleWidth, doubleHeight];
            pathfindingGridDesperate = new short[doubleWidth, doubleHeight];
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
                }
            }

            #endregion

            #region | Pathfinding |

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
                    int y = random.Next(0, doubleHeight);
                    startPoint = new IntPoint(x, y);
                    endPoint = new IntPoint(doubleWidth, y);
                    step = new IntPoint(1, 0);
                }
                else
                {
                    int x = random.Next(0, doubleWidth);
                    int y = 0;
                    startPoint = new IntPoint(x, y);
                    endPoint = new IntPoint(x, doubleHeight);
                    step = new IntPoint(0, 1);
                }
                while(startPoint != endPoint)
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
                    pathfindingGridDesperate[y, x] = (short)(walkable ? 1 : 0);
                    if (type == TileType.Path) walkable = false;
                    pathfindingGrid[y, x] = (short)(walkable ? 1 : 0);
                    if (type == TileType.Bridge) bridgeTiles.Add(IntermediateGrid[x, y]);
                    if (type == TileType.Road) roadTiles.Add(IntermediateGrid[x, y]);
                    if (type == TileType.Road || type == TileType.Bridge) npcWalkable.Add(IntermediateGrid[x, y]);
                }
            }

            var worldGrid = new WorldGrid(pathfindingGrid);
            pathfinder = new PathFinder(worldGrid, new PathFinderOptions { PunishChangeDirection = true, UseDiagonals = false, SearchLimit = int.MaxValue, HeuristicFormula = AStar.Heuristics.HeuristicFormula.Euclidean });

            #endregion

            Tile[,] Grid = IntermediateGrid;
            //for(int y = 0; y < mapHeight; y++)
            //{
            //    for(int x = 0; x < mapWidth; x++)
            //    {
            //        Grid[x, y] = IntermediateGrid[x * 2 + 1, y * 2 + 1];
            //    }
            //}

            mapHeight *= 2;
            mapWidth *= 2;

            #endregion

            Show();

            #region | Rendering |

            Canvas MainCanvas = new OCanvas();
            Canvas BGCanvas = new OCanvas();
            Canvas GameCanvas = new OCanvas();
            Canvas CameraCanvas = new OCanvas();
            Canvas UICanvas = new OCanvas();

            Canvas.SetLeft(CameraCanvas, 0);
            Canvas.SetTop(CameraCanvas, 0);

            RenderOptions.SetEdgeMode(GameCanvas, EdgeMode.Aliased);

            MainCanvas.Children.Add(CameraCanvas);
            MainCanvas.Children.Add(UICanvas);

            CameraCanvas.Children.Add(BGCanvas);
            CameraCanvas.Children.Add(GameCanvas);

            MainCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            MainCanvas.VerticalAlignment = VerticalAlignment.Top;

            Content = MainCanvas;

            int tileSize = TileSize;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Canvas image = Renderer.Render(Grid[x, y].Type, x, y, Grid);

                    image.Height = tileSize;
                    image.Width = tileSize;

                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

                    Canvas.SetLeft(image, x * tileSize);
                    Canvas.SetTop(image, y * tileSize);

                    //image.Opacity = pathfindingGrid[y, x];

                    GameCanvas.Children.Add(image);
                }
            }

            #endregion

            #region | Controls |

            MainCanvas.MouseDown += (a, b) => MouseIsDown = true;
            MainCanvas.MouseUp += (a, b) => MouseIsDown = false;
            MainCanvas.MouseMove += (a, b) =>
            {
                if (MouseIsDown)
                {
                    var newpos = PointToScreen(Mouse.GetPosition(this));
                    var diff = newpos - MousePos;
                    Canvas.SetLeft(CameraCanvas, Canvas.GetLeft(CameraCanvas) + diff.X);
                    Canvas.SetTop(CameraCanvas, Canvas.GetTop(CameraCanvas) + diff.Y);
                }
                MousePos = PointToScreen(Mouse.GetPosition(this));
            };

            ScaleTransform scale = new ScaleTransform(1, 1);
            CameraCanvas.RenderTransform = scale;

            MainCanvas.MouseWheel += (a, b) =>
            {
                float multi = 0.952f;
                if (b.Delta > 0) multi = 1.05f;

                scale.ScaleX *= multi;
                scale.ScaleY *= multi;

                if (b.Delta < 0)
                {
                    scale.ScaleX = Math.Floor(scale.ScaleX * 100) / 100f;
                    scale.ScaleY = Math.Floor(scale.ScaleY * 100) / 100f;
                }
                else
                {
                    scale.ScaleX = Math.Ceiling(scale.ScaleX * 100) / 100f;
                    scale.ScaleY = Math.Ceiling(scale.ScaleY * 100) / 100f;
                }

                Debug.WriteLine(scale.ScaleX);
            };
            #endregion

            #region | Entities |

            DispatcherTimer EntityLoop = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1 / 10) };
            EntityLoop.Tick += (a, b) =>
            {
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                foreach (Entity entity in Entities)
                {
                    long delta = milliseconds - entity.Time;
                    entity.Time = milliseconds;
                    entity.Tick(delta);

                    if (entity.Object is null)
                    {
                        entity.Object = entity.Render();
                        RenderOptions.SetBitmapScalingMode(entity.Object, BitmapScalingMode.NearestNeighbor);
                        var rt = new RotateTransform(entity.Rotation);
                        rt.CenterX = MainWindow.TileSize / 2;
                        rt.CenterY = MainWindow.TileSize / 2;
                        entity.Object.RenderTransform = rt;
                        GameCanvas.Children.Add(entity.Object);
                    }
                    ((RotateTransform)entity.Object.RenderTransform).Angle = entity.Rotation;
                    Canvas.SetLeft(entity.Object, entity.X);
                    Canvas.SetTop(entity.Object, entity.Y);

                }
            };
            EntityLoop.Start();

            #endregion

            for (int n = 0; n < NPCCount; n++)
            {
                Car car = new Car();
                Tile startTile = npcWalkable[random.Next(0, npcWalkable.Count)];
                Tile targetTile = npcWalkable[random.Next(0, npcWalkable.Count)];

                car.Point = new Point(startTile.X, startTile.Y);

                car.Target = new Point(targetTile.X, targetTile.Y);

                Car.CarEvent reset = car =>
                {
                    Tile targetTile = npcWalkable[random.Next(0, npcWalkable.Count)];
                    DispatcherTimer delay = new DispatcherTimer { Interval = TimeSpan.FromSeconds(random.Next(1, 5)) };
                    delay.Tick += (a, b) =>
                    {
                        car.Target = new Point(targetTile.X, targetTile.Y);
                        delay.Stop();
                    };
                    delay.Start();
                };

                car.JourneyFinished += reset;
                car.JourneyImpossible += reset;

                Entities.Add(car);
            }
        }
    }
}