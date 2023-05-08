using CityGame.Classes.Rendering;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using WPFGame;

namespace CityGame.Classes.Entities
{
    public class Helicopter : Entity
    {
        public float Speed { get; set; } = 256;
        public float RotSpeed { get; set; } = 1;
        public bool Landed = false;
        Image Heli1;
        Image Heli2;
        Image Blades1;
        Image Blades2;
        int RotorState = 0;
        public ISelectable Target;
        bool Move;
        public LightSource Spotlight;
        SoundEffectInstance Sound;
        AudioEmitter emitter;
        public override OCanvas Render()
        {
            Sound = Window.GetSound("helicopter").CreateInstance();
            Sound.IsLooped = true;
            

            emitter = new AudioEmitter { Position = new Vector3(X, Y, 25) };
            Sound.Apply3D(MainWindow.SoundEffectListener, emitter);

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

            Spotlight = new LightSource { Type = LightSourceType.Spotlight, Color = Color.White, Intensity = 3, Rotation = -90, Radius = MainWindow.TileSize * 3, Angle = MainWindow.TileSize * 3, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetTop(Spotlight, 7);
            Canvas.SetLeft(Spotlight, 32);

            LightSource PointLight = new LightSource { Type = LightSourceType.PointLight, Color = Color.White, Radius = MainWindow.TileSize, Angle = MainWindow.TileSize, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetLeft(PointLight, 32);
            Canvas.SetTop(PointLight, 28);

            canvas.Children.Add(Spotlight);
            canvas.Children.Add(PointLight);

            return canvas;
        }

        public override void Tick(long deltaTime)
        {
            if (Heli1 is null) return;
            emitter.Position = new Vector3(X, Y, 25);
            Tile myTile = MainWindow.Grid[(int)(X / MainWindow.TileSize), (int)(Y / MainWindow.TileSize)];
            if (myTile.Type != TileType.Helipad)
            {
                //if (Sound.State == SoundState.Paused || Sound.State == SoundState.Stopped) Sound.Play();
                Spotlight.Visible = true;
                Heli2.Visible = true;
                Heli1.Visible = false;
                long ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                if (ms / 250 % 2 == 0)
                {
                    Blades1.Visible = false;
                    Blades2.Visible = true;
                }
                else
                {
                    Blades1.Visible = true;
                    Blades2.Visible = false;
                }
            } else
            {
                if (Sound.State == SoundState.Playing) Sound.Pause();
                Spotlight.Visible = false;
                Blades1.Visible = true;
                Blades2.Visible = false;
                Heli1.Visible = true;
                Heli2.Visible = false;
            }
            if (Target is not null)
            {
                Vector2 nextTarget = new Vector2(Target.X(), Target.Y());
                if (Target is Car car)
                {
                    var correctionvector = new Vector2((float)Math.Cos(MathHelper.ToRadians(car.Rotation)), (float)Math.Sin(MathHelper.ToRadians(car.Rotation)));
                    correctionvector *= MainWindow.TileSize / 5;
                    nextTarget += correctionvector;
                }
                Vector2 travel = new Vector2(nextTarget.X - X, nextTarget.Y - Y);
                float minDistance = MainWindow.TileSize * 1;
                if (Target is Tile) minDistance = 0;
                float minSpeedyDistance = MainWindow.TileSize * 3;
                Spotlight.Radius = (int)Math.Max(minDistance, Math.Min(minSpeedyDistance, travel.Length()));
                if (Spotlight.Radius < minSpeedyDistance) Spotlight.Angle = Spotlight.Radius;
                if (travel.Length() < minDistance) Move = false;
                if (travel.Length() > minSpeedyDistance) Move = true;
                Vector2 direction = Vector2.Normalize(travel);
                float degrees = (float)(Math.Atan2(direction.Y, direction.X) * (180 / Math.PI)) + 90;
                if (Rotation != degrees)
                {
                    bool dir = degrees - (Rotation % 360) > 0;
                    float dis = Math.Abs(degrees - Rotation);
                    float tra = Math.Min(dis, RotSpeed);
                    Rotation += dir ? tra : -tra;
                }
                Spotlight.Rotation = (int)(Math.Max(-90, Math.Min(90, degrees - Rotation))) - 90;
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