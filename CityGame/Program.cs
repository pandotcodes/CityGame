using Microsoft.Xna.Framework.Graphics;
using WPFGame;

namespace CityGame
{
    public class Program { 
        public static void Main() {
            var menu = new MainWindow();
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
        public int Seed;
        public GenerationSettings()
        {

        }
    }
}