using AStar.Collections.MultiDimensional;
using CityGame.Classes.Rendering;
using CityGame.Classes.Rendering.Particles;
using CityGame.Classes.World;
using Microsoft.Xna.Framework;
using OrpticonGameHelper.Classes.Elements;

namespace CityGame.Classes.Entities
{
    public class GasPipe : Entity
    {
        public long Exploded { get; set; }
        public long LastSmoke { get; set; }
        public Image image { get; set; }
        public OCanvas canvas { get; set; }
        public GasPipe()
        {
            if (MainWindow.random.Next(0, 2) == 0) Rotation += 180;
            SingleSelect = true;
        }
        public Vector2 GetParticleOrigin()
        {
            return new Vector2(MainWindow.TileSize / 2);
        }
        public override OCanvas Render()
        {
            image = new SourcedImage("ManholeCover.png") { ZIndex = 98, Effects = { selectedEffect } };
            canvas = image;
            return canvas;
        }

        public override void Tick(long deltaTime)
        {
            if (Exploded > 0)
            {
                Exploded += deltaTime;
                LastSmoke += deltaTime;
                if (LastSmoke > 0 && Exploded < 6500)
                {
                    LastSmoke -= 500;
                    CreateSmoke();
                }
                Tile tile = MainWindow.Grid[(int)(X / MainWindow.TileSize), (int)(Y / MainWindow.TileSize)];
                Car.OccupiedTilesFill.WeirdAddToList(tile, null);
                Car.OccupiedTilesFill2.WeirdAddToList(tile, null);
            }
        }
        Explosion CreateSmoke()
        {
            Explosion smoke = new Explosion();
            smoke.DirectionTendency = MainWindow.Wind;
            smoke.DirectionVariance = 22.5f;
            smoke.MinColor = new Color(225, 225, 225);
            smoke.MaxColor = new Color(30, 30, 30);
            smoke.MaxParticleDistance = MainWindow.TileSize * 3;
            smoke.EmissionTime = 6;
            smoke.ParticleCountMin = 1;
            smoke.ParticleCountMax = 2;
            smoke.Size = 16;
            smoke.RotationOrigin = new Microsoft.Xna.Framework.Point(MainWindow.TileSize / 2);
            smoke.EaseMovement = false;
            Object.Children.Add(smoke);
            Canvas.SetLeft(smoke, GetParticleOrigin().X - 8);
            Canvas.SetTop(smoke, GetParticleOrigin().Y - 8);
            smoke.Emit().ContinueWith(t => CreateSmoke());
            return smoke;
        }
    }
}
