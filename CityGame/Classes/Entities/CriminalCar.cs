using CityGame.Classes.Rendering;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using OrpticonGameHelper;
using OrpticonGameHelper.Classes.Elements;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;

namespace CityGame.Classes.Entities
{
    public class CriminalCar : Car
    {
        public long CaughtTimer { get; set; }
        public bool GotAway { get; set; } = false;
        public bool Caught { get; set; } = false;
        public static List<Tile> targetTiles = new List<Tile>();
        public static List<CriminalCar> CCars = new List<CriminalCar>();
        public CriminalCar() : base()
        {
            CCars.Add(this);
            if (targetTiles.Count == 0)
            {
                for (int x = 0; x < MainWindow.Grid.GetLength(0); x++)
                {
                    targetTiles.Add(MainWindow.Grid[x, 0]);
                    targetTiles.Add(MainWindow.Grid[x, MainWindow.Grid.GetLength(1) - 1]);
                }
                for (int y = 0; y < MainWindow.Grid.GetLength(1); y++)
                {
                    targetTiles.Add(MainWindow.Grid[0, y]);
                    targetTiles.Add(MainWindow.Grid[MainWindow.Grid.GetLength(0) - 1, y]);
                }
            }
            grid = 2;
            //mightSwitchLane = true;
            Speed = 160;
            PNGFile = "CriminalCar.png";
            TimeUntilReroute = 1000;
        }
        public override OCanvas Render()
        {
            OCanvas canvas = base.Render();

            return canvas;
        }
        public override void Tick(long deltaTime)
        {
            if (!Caught && !GotAway)
            {
                if (CaughtTimer >= 0)
                {
                    if (Path is null || Path.Length == 0)
                    {
                        var myTile = MainWindow.Grid[(int)X / MainWindow.TileSize, (int)Y / MainWindow.TileSize];
                        Target = MainWindow.FindNearestTile(myTile, MainWindow.Grids[grid], targetTiles.ToArray());
                        if (Target is Tile tile && tile.X == myTile.X && tile.Y == myTile.Y)
                        {
                            GotAway = true;
                            Window.FireSound("missing");
                            Object.Visible = false;
                        }
                    }

                    base.Tick(deltaTime);

                    if (Path is null || Path.Length == 0)
                    {
                        CaughtTimer -= deltaTime;
                    }
                    else
                    {
                        CaughtTimer = 10000;
                    }
                }
                else
                {
                    ((SourcedImage)Object.Children[0]).Source = Environment.CurrentDirectory + "\\Resources\\CaughtCar.png";
                    Caught = true;
                    Window.FireSound("in_custody");
                }
            }
            if(Caught)
            {
                DoLaneBlockades();
            }
        }
    }
}