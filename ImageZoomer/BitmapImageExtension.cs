using System.IO;
using System.Windows.Media.Imaging;

namespace ImageZoomer;

internal static class BitmapImageExtension
{
    internal static BitmapImage? FromFile(string? imagePath)
    {
        if (!File.Exists(imagePath))
            return null;

        static BitmapImage ToBitmapImage(Stream stream)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CreateOptions = BitmapCreateOptions.None;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = stream;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        //return new BitmapImage(new Uri(imagePath));  これでも読めるがファイルがロックされます

        using var stream = File.OpenRead(imagePath);
        return ToBitmapImage(stream);
    }
}