namespace CityGame
{
    public interface ISelectable
    {
        public OCanvas GetImage();
        public bool RunAction(ISelectable target);
        public int X();
        public int Y();
    }
}