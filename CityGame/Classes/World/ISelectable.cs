using CityGame.Classes.Rendering;

namespace CityGame.Classes.World
{
    public interface ISelectable
    {
        public OCanvas GetImage();
        public bool RunAction(ISelectable target);
        public int X();
        public int Y();
    }
}