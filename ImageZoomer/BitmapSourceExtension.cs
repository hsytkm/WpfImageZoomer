using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageZoomer;

public static class BitmapSourceExtension
{
    public static IDisposable CopyToClipboard(this BitmapSource source, int scale = 16)
    {
        var wpfBitmap = ScaleBitmapSource(source, scale);
        var bitmap = GetDrawingBitmap(wpfBitmap);
        //bitmap.Save(@"D:\a.bmp");
        Clipboard.SetDataObject(bitmap, true);
        return bitmap;
    }

    private static WriteableBitmap ScaleBitmapSource(BitmapSource source, int scale, bool isFreeze = true)
    {
        if (scale < 1)
            throw new ArgumentException(nameof(scale));

        var bitmap = ConvertTo(source, PixelFormats.Bgr24);
        int bytesPerPixel = (bitmap.Format.BitsPerPixel + (8 - 1)) / 8;
        if (bytesPerPixel != 3)
            throw new ArgumentException($"BytesPerPixel must be 3ch. (Actual={bytesPerPixel})");

        (int srcWidth, int srcHeight) = (bitmap.PixelWidth, bitmap.PixelHeight);
        int srcStride = srcWidth * bytesPerPixel;

        (int destWidth, int destHeight) = (srcWidth * scale, srcHeight * scale);
        int destStride = srcStride * scale;
        int destSize = destHeight * destStride;

        byte[] srcRowBuffer = new byte[srcStride];
        byte[] destBuffer = new byte[destSize];
        unsafe
        {
            fixed (byte* srcHead = srcRowBuffer)
            fixed (byte* destHead = destBuffer)
            {
                byte* srcTail = srcHead + srcStride;
                byte* dest = destHead;
                Int32Rect rect1Line = new(0, 0, srcWidth, 1);

                // 1行ずつメモリに読み出して処理します(メモリ確保量の削減)
                for (int y = 0; y < srcHeight; y++)
                {
                    rect1Line.Y = y;
                    bitmap.CopyPixels(rect1Line, srcRowBuffer, srcStride, 0);

                    byte* destRowHead = dest;

                    // 1行を水平拡大してコピー
                    {
                        byte* destPtr = destRowHead;
                        for (byte* srcPtr = srcHead; srcPtr < srcTail; srcPtr += bytesPerPixel)
                        {
                            for (int shiftX = 0; shiftX < scale; shiftX++)
                            {
                                *(Pixel3ch*)destPtr = *(Pixel3ch*)srcPtr;
                                destPtr += sizeof(Pixel3ch);
                            }
                        }
                    }

                    // コピー済み1行を垂直方向のコピー
                    {
                        byte* src = destRowHead;
                        for (byte* ptr = src + destStride; ptr < src + (scale * destStride); ptr += destStride)
                            UnsafeExtensions.MemCopy(ptr, src, destStride);
                    }

                    dest += destStride * scale;
                }
            }
        }

        WriteableBitmap writeableBitmap = new(destWidth, destHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format, null);
        writeableBitmap.WritePixels(new Int32Rect(0, 0, destWidth, destHeight), destBuffer, destStride, 0);
        if (isFreeze)
            writeableBitmap.Freeze();

        return writeableBitmap;
    }

    [StructLayout(LayoutKind.Sequential, Size = 3)]
    readonly ref struct Pixel3ch
    {
        public readonly byte Ch0;
        public readonly byte Ch1;
        public readonly byte Ch2;
    }

    private static BitmapSource ConvertTo(this BitmapSource bitmap, in System.Windows.Media.PixelFormat pixelFormat)
    {
        if (bitmap.Format == pixelFormat)
            return bitmap;

        FormatConvertedBitmap format = new();
        format.BeginInit();
        format.Source = bitmap;
        format.DestinationFormat = pixelFormat;
        format.EndInit();
        format.Freeze();
        return format;
    }

    private static Bitmap GetDrawingBitmap(BitmapSource source)
    {
        var pixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;

        Bitmap bitmap = new(source.PixelWidth, source.PixelHeight, pixelFormat);
        bitmap.SetResolution((float)source.DpiX, (float)source.DpiY);

        BitmapData? data = null;
        try
        {
            data = bitmap.LockBits(
                new Rectangle(System.Drawing.Point.Empty, bitmap.Size),
                ImageLockMode.WriteOnly, pixelFormat);

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
