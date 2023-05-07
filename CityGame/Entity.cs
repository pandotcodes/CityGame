namespace CityGame
{
    public abstract class Entity : ISelectable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public long Time { get; set; }
        public OCanvas Object { get; set; }

        public OCanvas GetImage()
        {
            return Object;
        }

        public abstract OCanvas Render();

        public bool RunAction(ISelectable target)
        {
            if(this is Helicopter heli)
            {
                heli.Target = target;
                return true;
            }
            return false;
        }

        public abstract void Tick(long deltaTime);

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