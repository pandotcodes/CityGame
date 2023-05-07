using System;
using System.Numerics;
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
                Vector2 travel = new Vector2((float)nextTarget.X - X, (float)nextTarget.Y - Y);
                if (travel.Length() < MainWindow.TileSize * 1) Move = false;
                if (travel.Length() > MainWindow.TileSize * 3) Move = true;
                Vector2 direction = Vector2.Normalize(travel);
                float degrees = (float)(Math.Atan2(direction.Y, direction.X) * (180 / Math.PI)) + 90;
                Rotation = degrees;
                float Speedmulti = 1;
                if (travel.Length() < MainWindow.TileSize * 3) Speedmulti = (travel.Length() - MainWindow.TileSize) / (MainWindow.TileSize * 2);
                var possibleDistance = Speed * Speedmulti * deltaTime / 1000;
                var finalDistance = Math.Min(possibleDistance, travel.Length());
                Vector2 travelFinal = direction * finalDistance;
                X += travelFinal.X;
                Y += travelFinal.Y;
            }
        }
    }
}