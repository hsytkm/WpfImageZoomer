using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageZoomer;

public static class BitmapSourceExtension
{
    public static IDisposable CopyToClipboard(this BitmapSource source, double scale = 1)
    {
        // 拡大
        var wpfBitmap = scale > 1
            ? ScaleBitmapSource(source)
            : source;

        var bitmap = GetDrawingBitmap(wpfBitmap);
        Clipboard.SetDataObject(bitmap, true);
        return bitmap;
    }

    private static BitmapSource ScaleBitmapSource(BitmapSource source, double scale = 4)
    {
        var transform = new ScaleTransform(scale, scale);
        var scaledBitmap = new TransformedBitmap(source, transform);
        scaledBitmap.Freeze();
        return scaledBitmap;
    }

    private static Bitmap GetDrawingBitmap(BitmapSource source)
    {
        Bitmap bitmap = new(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        bitmap.SetResolution((float)source.DpiX, (float)source.DpiY);

        BitmapData? data = null;
        try
        {
            data = bitmap.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
                ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
        return bitmap;
    }

    // 画像が左上にずれて右下にノイズが入ったので使用していません
    //private static Bitmap ResizeBitmap(Bitmap srcBitmap, int ratio = 4)
    //{
    //    int newWidth = srcBitmap.Width * ratio;
    //    int newHeight = srcBitmap.Height * ratio;
    //    Bitmap resizedBitmap = new(newWidth, newHeight);
    //    resizedBitmap.SetResolution(96, 96);

    //    using Graphics graphics = Graphics.FromImage(resizedBitmap);
    //    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
    //    graphics.DrawImage(srcBitmap, 0, 0, newWidth, newHeight);

    //    return resizedBitmap;
    //}
}
