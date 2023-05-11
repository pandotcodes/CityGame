using OrpticonGameHelper.Classes.Elements.RUI.Attributes;

namespace CityGame.Classes.Menu
{
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