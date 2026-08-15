using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WoTMapWPF
{
    public class BitmapColorChanger
    {
        public static void ChangeColorKeepAlpha(WriteableBitmap bitmap, Color color)
        {
            PixelColor[,] pixelColors = GetPixels(bitmap);
            for (int i = 0; i < bitmap.PixelWidth; i++)
                for (int j = 0; j < bitmap.PixelHeight; j++)
                {
                    pixelColors[i, j].Blue = color.B;
                    pixelColors[i, j].Green = color.G;
                    pixelColors[i, j].Red = color.R;
                }
            PutPixels(bitmap, pixelColors, 0, 0);
        }

        private static PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            BitmapCopyPixels(source, result, width * 4, 0);
            return result;
        }
        private static void PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, int x, int y)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
        }

        private static void BitmapCopyPixels(BitmapSource source, PixelColor[,] pixelsOutput, int stride, int offset)
        {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    pixelsOutput[y + y0, x + x0] = new PixelColor
                    {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }
    }
}
