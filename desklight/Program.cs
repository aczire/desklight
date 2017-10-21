using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace desklight
{
    class Program
    {
        static void Main(string[] args)
        {
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string spotlightFolder = "Packages\\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\\LocalState\\Assets";

            string spotlightFolderPath = Path.Combine(appdataPath, spotlightFolder);

            List<String> spotLightImages = new List<string>();

            foreach (string spotfile in Directory.GetFiles(spotlightFolderPath))
            {
                if (new FileInfo(spotfile).Length > 250000)
                {
                    //System.Console.WriteLine(spotfile);
                    if (Image.FromFile(spotfile).Width > Image.FromFile(spotfile).Height)
                    {
                        //Console.WriteLine(spotfile);
                        spotLightImages.Add(spotfile);
                    }
                }
            }

            if (spotLightImages.Count > 0)
            {
                Random rand = new Random();
                int spotIndex = rand.Next(spotLightImages.Count - 1);
                Wallpaper.Set(Image.FromFile(spotLightImages[spotIndex]), Wallpaper.Style.Centered);
            }
        }
    }
}
