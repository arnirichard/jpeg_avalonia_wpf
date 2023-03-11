using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class YuvRgb
    {
        public static int[] YuvToRgb(int[] yuvValues)
        {
            int[] result = new int[64];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = YuvToRgb(yuvValues[i]);
        }

            return result;
        }

        public static int[] RgbToYuv(int[] rgbValues)
        {
            int[] result = new int[rgbValues.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = RgbToYuv(rgbValues[i]);
            }

            return result;
        }

        public static int YuvToRgb(int yuv)
        {
            return YuvToRgb((yuv & 0x00ff0000) >> 16,
                (yuv & 0x0000ff00) >> 8,
                yuv & 0x000000ff);
        }

        public static int YuvToRgb(int y, int u, int v)
        {
            if (y < 0)
                y = 0;
            if (y > 255)
                y = 255;
            if (u < 0)
                u = 0;
            if (u > 255)
                u = 255;
            if (v < 0)
                v = 0;
            if (v > 255)
                v = 255;

            int r = (int)(y + 1.402 * (v - 128));
            int g = (int)(y - 0.344f * (u - 128) - 0.714f * (v - 128));
            int b = (int)(y + 1.772f * (u - 128));
            if (r < 0)
                r = 0;
            if (r > 255)
                r = 255;
            if (g < 0)
                g = 0;
            if (g > 255)
                g = 255;
            if (b < 0)
                b = 0;
            if (b > 255)
                b = 255;

            return (0xff << 24) | (r << 16) | (g << 8) | b;
        }


        public static int RgbToYuv(int rgb)
        {
            int r = (rgb & 0x00ff0000) >> 16;
            int g = (rgb & 0x0000ff00) >> 8;
            int b = rgb & 0x000000ff;
            int y = (int)(0.2990 * r + 0.5870 * g + 0.1140 * b);
            int u = (int)(-0.1687 * r - 0.3313 * g + 0.5000 * b) + 128;
            int v = (int)(0.5000 * r - 0.4187 * g - 0.0813 * b) + 128;
            if(y< 0) 
                y = 0;
            if (y > 255)
                y = 255;
            if (u < 0)
                u = 0;
            if (u > 255)
                u = 255;
            if (v < 0)
                v = 0;
            if (v > 255)
                v = 255;

            return (y << 16) | (u << 8) | v;

        }
    }
}
