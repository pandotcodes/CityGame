namespace CityGame
{
    public abstract class Entity
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
        public long Time { get; set; }
        public OCanvas Object { get; set; }
        public abstract OCanvas Render();
        public abstract void Tick(long deltaTime);
    }
}