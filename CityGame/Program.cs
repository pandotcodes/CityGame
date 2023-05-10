using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WPFGame;

namespace CityGame
{
    public class Program { 
        public static void Main() {
            var menu = new MenuWindow();
        } 
    }
    public class MenuWindow : Window
    {
        public MenuWindow()
        {
            UICanvas = new Canvas();
            UICanvas.Children.Add(new ReflectedUIWindow(new GenerationSettings(), "CNRGN  Builder", 24) { Height = 1080, Width = 300 });

            Show();
        }
    }
    public class GenerationSettings
    {
        [RUITextField(TextColor = 0xFFFF0000, Lines = 2)]
        public string WelcomeText { get; set; } = "Welcome!\nTest";
        [RUITextField(TextColor = 0xFF00FF00, Lines = 2)]
        public int Seed { get; set; } = 0;
        public GenerationSettings()
        {

        }
    }
}