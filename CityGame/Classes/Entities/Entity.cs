using CityGame.Classes.Rendering;
using CityGame.Classes.World;
using OrpticonGameHelper.Classes.Effects;

namespace CityGame.Classes.Entities
{
    public abstract class Entity : ISelectable
    {
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