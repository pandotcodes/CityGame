using CityGame.Classes.Rendering;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CityGame.Classes.Entities
{
    public class PoliceCar : Car
    {
        public static List<PoliceCar> PCars = new List<PoliceCar>();
        public PoliceCar() : base()
        {
            grid = 2;
            mightSwitchLane = true;
            PCars.Add(this);
            Speed = 192;
            PNGFile = "PoliceCar.png";
        }
        public override OCanvas Render()
        {
            OCanvas canvas = base.Render();

            return canvas;
        }
        public override void Tick(long deltaTime)
        {
            if (PointLight is null) return;
            long ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            Tile myTile = MainWindow.Grid[Point.X, Point.Y]; if (myTile.Type == TileType.Garage)
            {
                PointLight.Color = Color.White;
            }
            else
            {
                if (ms / 250 % 2 == 0)
                {
                    PointLight.Color = Color.Red;
                }
                else
                {
                    PointLight.Color = Color.Blue;
                }
            }

            base.Tick(deltaTime);
        }
    }
}