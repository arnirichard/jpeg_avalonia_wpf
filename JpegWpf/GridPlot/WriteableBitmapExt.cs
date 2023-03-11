using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace JpegWpf
{
    internal static class WriteableBitmapExt
    {
        public static unsafe int[] ReadPixels(this WriteableBitmap writeableBitmap, int x, int y, int height, int width)
        {
            writeableBitmap.Lock();

            x = Math.Max(0, x);
            y = Math.Max(0, y);

            int x2 = Math.Min(x + width, writeableBitmap.PixelWidth);
            int y2 = Math.Min(y + height, writeableBitmap.PixelHeight);

            height = y2 - y;
            width = x2 - x;

            if (width <= 0 || height <= 0)
                return new int[0];

            int[] result = new int[width * height];

            try
            {
                writeableBitmap.Lock();
                IntPtr ptr = writeableBitmap.BackBuffer;

                ptr += y * writeableBitmap.BackBufferStride + x * 4;

                int index = 0;

                for (int _y = 0; _y < height; _y++)
                {
                    for (int _x = 0; _x < width; _x++)
                    {
                        result[index++] = *(int*)ptr;
                        ptr += 4;
                    }
                    ptr += writeableBitmap.BackBufferStride - width * 4;
                }

                return result;
            }
            finally
            {
                writeableBitmap.Unlock();
            }
        }

        internal static unsafe void PaintRect(this WriteableBitmap writeableBitmap, int color, int x, int y, int width, int height)
        {
            x = Math.Max(0, x);
            y = Math.Max(0, y);

            int x2 = Math.Min(x + width, writeableBitmap.PixelWidth);
            int y2 = Math.Min(y + height, writeableBitmap.PixelHeight);

            height = y2 - y;
            width = x2 - x;

            if (width <= 0 || height <= 0)
                return;

            try
            {
                IntPtr ptr = writeableBitmap.BackBuffer;

                ptr += y * writeableBitmap.BackBufferStride + x * 4;

                for (int _x = 0; _x < height; _x++)
                {
                    for (int _y = 0; _y < width; _y++)
                    {
                        *(int*)ptr = color;
                        ptr += 4;
                    }
                    ptr += writeableBitmap.BackBufferStride - width * 4;
                }

                
            }
            finally
            {
                //writeableBitmap.Unlock();
            }
        }
    }
}
