using System;
using System.IO;
using System.Runtime.InteropServices;
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
            desktop.SetValue("TileWallpaper", 1, RegistryValueKind.String);
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
    }
}
