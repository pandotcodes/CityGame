using CityGame.Classes.Rendering;
using OrpticonGameHelper.Classes.Elements;

namespace CityGame.Classes.Entities
{
    public class GasPipe : Entity
    {
        public int Exploded { get; set; }
        public GasPipe()
        {
            if (MainWindow.random.Next(0, 2) == 0) Rotation += 180;
            SingleSelect = true;
        }

        public override OCanvas Render()
        {
            return new SourcedImage("ManholeCover.png") { ZIndex = 98, Effects = { selectedEffect } };
        }

        public override void Tick(long deltaTime)
        {

        }
    }
}
