using OrpticonGameHelper;
using OrpticonGameHelper.Classes.Elements;
using OrpticonGameHelper.Classes.Elements.RUI;

namespace CityGame.Classes.Menu
{
    public class MenuWindow : Window
    {
        public MenuWindow()
        {
            UICanvas = new Canvas();
            UICanvas.Children.Add(new ReflectedUIWindow(new GenerationSettings(), "CNRGN  Builder", 24) { Height = 1080, Width = 300 });

            Show();
        }
    }
}