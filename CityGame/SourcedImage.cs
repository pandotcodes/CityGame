using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CityGame
{
    public class SourcedImage : Image
    {
        public Dictionary<string, string> Alternatives = new Dictionary<string, string>()
        {
            {"Path3.png", "Path3c.png" }
        };

        public static Dictionary<string, BitmapSource> loadedSources = new Dictionary<string, BitmapSource>();

        public SourcedImage(string source, string tooltip = "")
        {
            Source = SourceToImage(source);
            ToolTip = source + "\n" + tooltip;
            Height = 64;
            Width = 64;

            ImageFailed += SourcedImage_ImageFailed;
        }

        private void SourcedImage_ImageFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            Debug.WriteLine("failed");
        }

        public BitmapSource SourceToImage(string src, UriKind kind = UriKind.Absolute)
        {
            string degs = src.Split(':').Last();
            string uri = src;
            int deg = 0;
            if (degs == "0" || degs == "90" || degs == "180" || degs == "270" || degs == "360" || degs == "450" || degs == "540" || degs == "630" || degs == "720")
            {
                deg = Convert.ToInt32(degs);
                uri = string.Join(":", src.Split(':').Take(src.Split(':').Count() - 1).ToArray());
            }
            string last = "";
            if(Alternatives.ContainsKey(uri))
            {
                uri = Alternatives[uri];
            }
            uri = Environment.CurrentDirectory + "\\Resources\\" + uri;
            if (loadedSources.ContainsKey(src))
            {
                return loadedSources[src];
            }
            else
            {
                if (!File.Exists(uri))
                {
                    uri = Environment.CurrentDirectory + "\\Resources\\ErrorRed.png";
                }
                loadedSources.Add(src, new TransformedBitmap(new BitmapImage(new Uri(uri, kind)), new RotateTransform(deg)));
                return loadedSources[src];
            }
        }
    }
}