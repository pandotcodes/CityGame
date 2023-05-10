using CityGame.Classes.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFGame;

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
            return new SourcedImage("ManholeCover.png");
        }

        public override void Tick(long deltaTime)
        {

        }
    }
}
