using CityGame.Classes.Rendering;
using CityGame.Classes.Rendering.Particles;
using CityGame.Classes.World;
using OrpticonGameHelper.Classes.Effects;
using OrpticonGameHelper.Classes.Elements;

namespace CityGame.Classes.Entities
{
    public abstract class Entity : ISelectable
    {
        public bool IsSingleSelect() => SingleSelect;
        public float X { get; set; }
        public float Y { get; set; }
        protected float visualX;
        protected float visualY;
        protected float visualRotation;
        public float VisualX { get => UseVisualPosition ? visualX : X; set => X = visualX = value; }
        public float VisualY { get => UseVisualPosition ? visualY : Y; set => Y = visualY = value; }
        public float VisualRotation { get => UseVisualPosition ? visualRotation : Rotation; set => Rotation = visualRotation = value; }
        public bool UseVisualPosition { get; set; }
        public float Rotation { get; set; }
        public long Time { get; set; }
        public OCanvas Object { get; set; }
        public  bool SingleSelect { get; set; }
        protected OutlineEffect selectedEffect = new OutlineEffect();
        public OCanvas GetImage()
        {
            return Object;
        }

        public abstract OCanvas Render();

        public bool RunAction(ISelectable target)
        {
            if (this is Helicopter heli)
            {
                heli.Target = target;
                return true;
            }
            if(this is PoliceCar car)
            {
                car.Path = null;
                car.Target = target;
                return true;
            }
            if(this is GasPipe pipe)
            {
                if (pipe.Exploded > 0) return false;
                Explosion x = new Explosion();
                pipe.canvas.Children.Add(x);
                Canvas.SetLeft(x, pipe.GetParticleOrigin().X);
                Canvas.SetTop(x, pipe.GetParticleOrigin().Y);
                x.RotationOrigin = new Microsoft.Xna.Framework.Point(MainWindow.TileSize / 2);
                x.Size = 16;
                x.MaxParticleDistance = 32;
                x.Emit();
                pipe.Exploded = 1;
                return true;
            }
            return false;
        }

        public abstract void Tick(long deltaTime);
        public void BaseTick(long deltaTime)
        {
            if (selectedEffect is not null)
                selectedEffect.Visible = this == MainWindow.Selected;
        }

        int ISelectable.X()
        {
            return (int)X;
        }

        int ISelectable.Y()
        {
            return (int)Y;
        }
    }
}