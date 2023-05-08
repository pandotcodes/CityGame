using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CityGame.Classes.Rendering
{
    public class ImageConverter
    {
        public static void ChangeColor(string srcFile, string destFile, Dictionary<string, string> conversions)
        {
            destFile = Environment.CurrentDirectory + "\\Resources\\" + destFile + ".png";
            //if (File.Exists(destFile)) return;
            srcFile = Environment.CurrentDirectory + "\\Resources\\" + srcFile + ".png";
            if (!File.Exists(srcFile)) return;

            Dictionary<System.Drawing.Color, System.Drawing.Color> Conversions = conversions.Select(x => new KeyValuePair<System.Drawing.Color, System.Drawing.Color>(System.Drawing.ColorTranslator.FromHtml(x.Key), System.Drawing.ColorTranslator.FromHtml(x.Value))).ToDictionary(x => x.Key, x => x.Value);
            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(srcFile);
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    var color = bmp.GetPixel(x, y);
                    if (Conversions.ContainsKey(color))
                    {
                        bmp.SetPixel(x, y, Conversions[color]);
                    }
                }
            }
            bmp.Save(destFile);
        }
    }
}