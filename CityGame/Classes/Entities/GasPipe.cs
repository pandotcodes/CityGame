using CityGame.Classes.Rendering;
using CityGame.Classes.Rendering.Particles;
using Microsoft.Xna.Framework;
using OrpticonGameHelper.Classes.Elements;

namespace CityGame.Classes.Entities
{
    public class GasPipe : Entity
    {
        public long Exploded { get; set; }
        public Image image { get; set; }
        public OCanvas canvas { get; set; }
        public Explosion Smoke { get; set; }
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
                if(Smoke is null)
                {
                    CreateSmoke();
                }
            }
        }
        void CreateSmoke()
        {
            Explosion smoke = new Explosion();
            smoke.DirectionTendency = MainWindow.Wind;
            smoke.DirectionVariance = 22.5f;
            smoke.MinColor = Color.LightGray;
            smoke.MaxColor = Color.DarkGray;
            smoke.MaxParticleDistance = MainWindow.TileSize * 3;
            smoke.EmissionTime = 3;
            smoke.ParticleCountMin = 1;
            smoke.ParticleCountMax = 2;
            smoke.Size = 16;
            smoke.RotationOrigin = new Microsoft.Xna.Framework.Point(MainWindow.TileSize / 2);
            smoke.EaseMovement = false;
            Smoke = smoke;
            Object.Children.Add(Smoke);
            Canvas.SetLeft(Smoke, GetParticleOrigin().X);
            Canvas.SetTop(Smoke, GetParticleOrigin().Y);
            smoke.Emit().ContinueWith(t => CreateSmoke());
        }
    }
}
