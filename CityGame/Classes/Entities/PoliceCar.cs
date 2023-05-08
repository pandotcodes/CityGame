using CityGame.Classes.Rendering;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using WPFGame;

namespace CityGame.Classes.Entities
{
    public class PoliceCar : Car
    {
        public static List<PoliceCar> PCars = new List<PoliceCar>();
        protected LightSource sirenLight;
        public PoliceCar() : base()
        {
            desperate = true;
            PCars.Add(this);
            Speed = 192;
            PNGFile = "PoliceCar.png";
        }
        public override OCanvas Render()
        {
            OCanvas canvas = base.Render();

            sirenLight = new LightSource { Radius = 24, Angle = 24, Intensity = 4f, Color = Color.Red, Type = LightSourceType.PointLight, RotationOrigin = new Point(MainWindow.TileSize / 2) };
            canvas.Children.Add(sirenLight);
            lights.Add(sirenLight);
            Canvas.SetLeft(sirenLight, 43);
            Canvas.SetTop(sirenLight, 32);

            return canvas;
        }
        public override void Tick(long deltaTime)
        {
            if (sirenLight is null) return;
            long ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (ms / 250 % 2 == 0)
            {
                sirenLight.Color = Color.Red;
            }
            else
            {
                sirenLight.Color = Color.Blue;
            }

            base.Tick(deltaTime);
        }
    }
}