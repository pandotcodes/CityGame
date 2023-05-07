using Microsoft.Xna.Framework;
using System;
using WPFGame;

namespace CityGame
{
    public class Helicopter : Entity
    {
        public float Speed { get; set; } = 256;
        public bool Landed = false;
        Image Heli1;
        Image Heli2;
        Image Blades1;
        Image Blades2;
        int RotorState = 0;
        public ISelectable Target;
        bool Move;
        public LightSource Spotlight;
        public override OCanvas Render()
        {
            OCanvas canvas = new OCanvas();
            Heli1 = new SourcedImage("Helicopter.png");
            Heli2 = new SourcedImage("HelicopterFlight.png");
            Blades1 = new SourcedImage("HelicopterBlades.png");
            Blades2 = new SourcedImage("HelicopterBlades2.png");

            Heli1.Visible = false;
            Blades2.Visible = false;

            canvas.Children.Add(Heli1);
            canvas.Children.Add(Heli2);
            canvas.Children.Add(Blades1);
            canvas.Children.Add(Blades2);

            Spotlight = new LightSource { Type = LightSourceType.Spotlight, Color = Color.White, Intensity = 3, Rotation = -90, Radius = MainWindow.TileSize * 3, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetTop(Spotlight, 7);
            Canvas.SetLeft(Spotlight, 32);

            canvas.Children.Add(Spotlight);

            return canvas;
        }

        public override void Tick(long deltaTime)
        {
            if (Heli1 is null) return;
            long ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (ms / 250 % 2 == 0)
            {
                Blades1.Visible = false;
                Blades2.Visible = true;
            } else
            {
                Blades1.Visible = true;
                Blades2.Visible = false;
            }
            if (Target is not null)
            {
                IntPoint nextTarget = new IntPoint(Target.X(), Target.Y());
                if(Target is Car car)
                {
                    var correctionvector = new IntPoint((int)Math.Cos(Microsoft.Xna.Framework.MathHelper.ToRadians(car.Rotation)), (int)Math.Sin(Microsoft.Xna.Framework.MathHelper.ToRadians(car.Rotation)));
                    correctionvector *= MainWindow.TileSize / 4;
                    nextTarget += correctionvector;
                }
                Vector2 travel = new Vector2((float)nextTarget.X - X, (float)nextTarget.Y - Y);
                float minDistance = MainWindow.TileSize * 1;
                if (Target is Tile) minDistance = 0;
                float minSpeedyDistance = MainWindow.TileSize * 3;
                Spotlight.Radius = (int)Math.Min(minSpeedyDistance, travel.Length());
                if (travel.Length() < minDistance) Move = false;
                if (travel.Length() > minSpeedyDistance) Move = true;
                Vector2 direction = Vector2.Normalize(travel);
                float degrees = (float)(Math.Atan2(direction.Y, direction.X) * (180 / Math.PI)) + 90;
                Rotation = degrees;
                float Speedmulti = 1;
                if (travel.Length() < minSpeedyDistance) Speedmulti = (travel.Length() - minDistance) / (minSpeedyDistance - minDistance);
                var possibleDistance = Speed * Speedmulti * deltaTime / 1000;
                var finalDistance = Math.Min(possibleDistance, travel.Length());
                Vector2 travelFinal = direction * finalDistance;
                X += travelFinal.X;
                Y += travelFinal.Y;
            }
        }
    }
}