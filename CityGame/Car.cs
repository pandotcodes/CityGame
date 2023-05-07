using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Windows;
using WPFGame;

namespace CityGame
{
    public class Car : Entity
    {
        public delegate void CarEvent(Car car);
        public event CarEvent JourneyFinished;
        public event CarEvent JourneyImpossible;
        public Point? Target { get; set; } = null;
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
        public override OCanvas Render()
        {
            OCanvas canvas = new OCanvas();
            Image car = new SourcedImage("Car.png");

            canvas.Children.Add(car);
            var light = new LightSource { Radius = 64, Intensity = 2, Color = Color.White, Type = LightSourceType.Spotlight, Rotation = -90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            var light2 = new LightSource { Radius = 64, Intensity = 2, Color = Color.White, Type = LightSourceType.Spotlight, Rotation = -90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetLeft(light, 39);
            Canvas.SetTop(light, 19);
            Canvas.SetLeft(light2, 46);
            Canvas.SetTop(light2, 19);
            canvas.Children.Add(light);
            canvas.Children.Add(light2);

            var blight = new LightSource { Radius = 24, Intensity = 1, Color = Color.Red, Type = LightSourceType.PointLight, Rotation = 90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            var blight2 = new LightSource { Radius = 24, Intensity = 1, Color = Color.Red, Type = LightSourceType.PointLight, Rotation = 90, RotationOrigin = new Point(MainWindow.TileSize / 2, MainWindow.TileSize / 2) };
            Canvas.SetLeft(blight, 39);
            Canvas.SetTop(blight, 46);
            Canvas.SetLeft(blight2, 46);
            Canvas.SetTop(blight2, 46);
            canvas.Children.Add(blight);
            canvas.Children.Add(blight2);

            return canvas;
        }
        public Car()
        {
            JourneyFinished += c => { };
            JourneyImpossible += c => { };
        }

        public override void Tick(long deltaTime)
        {
            if (Target is not null)
            {
                //if(Object is not null) Object.ToolTip = Target.ToString();
                if (Path is null)
                {
                    Path = MainWindow.pathfinder.FindPath(Point.Convert(), ((Point)Target).Convert()).Select(x => x.Convert()).ToArray();
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
                    NextTarget++;
                }
                if (NextTarget == Path.Length)
                {
                    Path = null;
                    Target = null;
                    NextTarget = 0;
                    JourneyFinished(this);
                    return;
                }
                if (X.CloselyEquals(nextTarget.X * MainWindow.TileSize) && Y.CloselyEquals(nextTarget.Y * MainWindow.TileSize))
                    return;
                Vector2 travel = new Vector2((float)nextTarget.X * 64 - X, (float)nextTarget.Y * 64 - Y);
                Vector2 direction = Vector2.Normalize(travel);
                float degrees = (float)(Math.Atan2(direction.Y, direction.X) * (180 / Math.PI)) + 90;
                Rotation = degrees;
                var possibleDistance = Speed * deltaTime / 1000;
                var finalDistance = Math.Min(possibleDistance, travel.Length());
                Vector2 travelFinal = direction * finalDistance;
                X += travelFinal.X;
                Y += travelFinal.Y;
            }
        }
    }
}