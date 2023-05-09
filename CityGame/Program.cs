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
        private GraphicsDevice graphics;
        public MenuWindow()
        {
            

            Show();
        }
    }
    public class GenerationSettings
    {
        public GenerationSettings()
        {

        }
    }
}