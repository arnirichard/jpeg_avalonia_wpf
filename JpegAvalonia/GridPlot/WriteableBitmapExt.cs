using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JpegAvalonia
{
    internal static class WriteableBitmapExt
    {
        public static unsafe int[] ReadPixels(this WriteableBitmap writeableBitmap, int x, int y, int height, int width)
        {
            x = Math.Max(0, x);
            y = Math.Max(0, y);

            int x2 = Math.Min(x + width, writeableBitmap.PixelSize.Width);
            int y2 = Math.Min(y + height, writeableBitmap.PixelSize.Height);

            height = y2 - y;
            width = x2 - x;

            if (width <= 0 || height <= 0)
                return new int[0];

            int[] result = new int[width * height];

            using (var buf = writeableBitmap.Lock())
            {
                var ptr = (uint*)buf.Address;

                ptr += y * buf.Size.Width + x;
                int index = 0;

                for (int _y = 0; _y < height; _y++)
                {
                    for (int _x = 0; _x < width; _x++)
                    {
                        result[index++] = (int)*ptr;
                        ptr += 1;
                    }
                    ptr += buf.Size.Width - width;
                }
            }

            return result;
        }


        internal static unsafe void PaintRect(this WriteableBitmap writeableBitmap, uint color, int x, int y, int width, int height)
        {
            x = Math.Max(0, x);
            y = Math.Max(0, y);

            int x2 = Math.Min(x + height, writeableBitmap.PixelSize.Width);
            int y2 = Math.Min(y + width, writeableBitmap.PixelSize.Height);

            height = y2 - y;
            width = x2 - x;

            if (width <= 0 || height <= 0)
                return;

            using (var buf = writeableBitmap.Lock())
            {
                var ptr = (uint*)buf.Address;

                ptr += y * buf.Size.Width + x;

                for (int _y = 0; _y < height; _y++)
                {
                    for (int _x = 0; _x < width; _x++)   
                    {
                        *ptr = color;
                        ptr += 1;
                    }
                    ptr += buf.Size.Width - width;
                }
            }
        }
    }
}
