using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;

namespace RefreshWallpaper
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);
        private static UInt32 SPI_SETDESKWALLPAPER = 20;
        private static UInt32 SPIF_UPDATEINIFILE = 0x1;
        private const string EXTENSION = ".jpg";
        private const string BING = "bing";

        private static Random random = new Random();

        static void Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "help")
            {
                Console.WriteLine("Use a directory path or an image location as parameter. Ex: RefreshWallpaper d:\temp");
                return;
            }
            UpdateRegistrySettings();
            var path = args[0];
            if (string.Compare(path, BING) == 0)
            {
                Task<string> randomImage = GetRandomBingImage();
                randomImage.Wait();
                SetImage(randomImage.Result);
                return;
            }
            if (Directory.Exists(path))
            {
                string fileName = GetRandomImage(path);
                SetImage(fileName);
                return;
            }
            if (File.Exists(path) && Path.GetExtension(path) == EXTENSION)
            {
                SetImage(path);
                return;
            }
            throw new InvalidOperationException("The image doesn't exist or is invalid.");
        }

        private static void UpdateRegistrySettings()
        {
            var controlPane = Registry.CurrentUser.CreateSubKey("Control Panel");
            if (controlPane == null)
                throw new InvalidOperationException("Cannot create Control Panel registry key");
            var desktop = controlPane.CreateSubKey("Desktop");
            if (desktop == null)
                throw new InvalidOperationException("Cannot create desktop registry key");
            desktop.SetValue("WallpaperStyle", 0, RegistryValueKind.String);
        }

        private static string GetRandomImage(string path)
        {
            string[] images = Directory.GetFiles(path, "*" + EXTENSION);
            int index = random.Next(0, images.Length);
            return images[index];
        }

        private static void SetImage(string filename)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, SPIF_UPDATEINIFILE);
        }

        private static async Task<string> GetRandomBingImage()
        {
            string themeUrl = ConfigurationManager.AppSettings["themeUrl"];
            var items = (from x in XDocument.Load(themeUrl).Descendants("item")
                         select x.Element("enclosure").Attribute("url").Value);
            int index = random.Next(0, items.Count());
            string url = items.Skip(index).First();
            return await RetrieveImageAsync(url);
        }

        private static async Task<string> RetrieveImageAsync(string url)
        {
            var uri = new Uri(url);
            string imageName = Path.Combine(ConfigurationManager.AppSettings["wallpaperFolder"], uri.Segments.Last());
            if (!File.Exists(imageName))
                await DownloadImageAsync(url, imageName);
            return imageName;
        }

        private static async Task DownloadImageAsync(string url, string imageName)
        {
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(url, imageName);
            }
        }
    }
}
