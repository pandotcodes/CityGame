using WPFGame;

namespace CityGame
{
    public class OCanvas : Canvas
    {
        public static implicit operator OCanvas(Image image)
        {
            OCanvas canvas = new OCanvas();
            canvas.Children.Add(image);
            return canvas;
        }
        public OCanvas() : base()
        {
        }
    }
}