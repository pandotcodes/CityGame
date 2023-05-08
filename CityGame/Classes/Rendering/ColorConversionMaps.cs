using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityGame.Classes.Rendering
{
    public class ColorConversionMaps
    {

        public static Dictionary<string, string> HouseToLakeMap = new Dictionary<string, string>
            {
                { "#616161", "#006bb3" },
                { "#3b3b3b", "#005485" }
            };
        public static Dictionary<string, string> LakeToRiver = new Dictionary<string, string>
            {
                { "#006bb3", "#005ba3" },
                { "#005485", "#004475" }
            };
        public static Dictionary<string, string> HouseToBuildingDarkMap = new Dictionary<string, string>
            {
                { "#616161", "#3b3b3b" },
                { "#3b3b3b", "#1b1b1b" }
            };
        public static Dictionary<string, string> HouseToBuildingBlueMap = new Dictionary<string, string>
            {
                { "#616161", "#616181" },
                { "#3b3b3b", "#3b3b5b" }
            };
        public static Dictionary<string, string> HouseToBuildingRedMap = new Dictionary<string, string>
            {
                { "#616161", "#816161" },
                { "#3b3b3b", "#5b3b3b" }
            };
        public static Dictionary<string, string> HouseToBuildingGreenMap = new Dictionary<string, string>
            {
                { "#616161", "#618161" },
                { "#3b3b3b", "#3b5b3b" }
            };
        public static Dictionary<string, string> HouseToParkMap = new Dictionary<string, string>
            {
                { "#616161", "#00a34b" },
                { "#3b3b3b", "#007534" }
            };
        public static Dictionary<string, string> RoadToPathMap = new Dictionary<string, string>()
            {
                { "#00000000", "#00b36b00" },
                { "#616161", "#00b36b00" },
                { "#303030", "#8a5e00" },
                { "#ffffff", "#8a5e00" }
            };
        public static Dictionary<string, string> RoadToBridgeMap = new Dictionary<string, string>()
            {
                { "#00000000", "#00b36b00" },
                { "#616161", "#00b36b00" }
            };
        public static Dictionary<string, string> RoadToHighwayMap = new Dictionary<string, string>()
            {
                { "#00000000", "#00b36b00" },
                { "#616161", "#303030" }
            };
        public static Dictionary<string, string> CarToNPCCar = new Dictionary<string, string>()
        {
            { "#ff0000", "#888888" },
            { "#fc4141", "#fcfcfc" },
            { "#b80000", "#b8b8b8" }
        };
        public static Dictionary<string, string> CarToPoliceCar = new Dictionary<string, string>()
        {
            { "#ff0000", "#0000ff" },
            { "#fc4141", "#4141fc" },
            { "#b80000", "#0000b8" }
        };
    }
}
