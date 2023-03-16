using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class YCbCrRgbColor
    {
        public static int[] YuvToRgb(int[] yuvValues, int subtract = 0)
        {
            int[] result = new int[64];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = YCbCrToRgb(yuvValues[i], subtract);
            }

            return result;
        }

        public static int[][] RgbToYCrCb(int[] rgbValues, int add = 0)
        {
            int[][] result = new int[3][];
            int[] yuv;

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new int[64];
            }

            for(int i =0; i< rgbValues.Length; i++)
            {
                yuv = RgbToYuv(rgbValues[i], add);
                result[0][i] = yuv[0];
                result[1][i] = yuv[1];
                result[2][i] = yuv[2];
            }

            return result;
        }

        public static int YCbCrToRgb(int yuv, int subtract = 0)
        {
            return YCbCrToRgb(((yuv & 0x00ff0000) >> 16) - subtract,
                ((yuv & 0x0000ff00) >> 8)-subtract,
                (yuv & 0x000000ff)-subtract);
        }

        // y,u,v are in the range -128 to 127
        public static int YCbCrToRgb(int y, int u, int v)
        {
            int r = (int)(y + 1.402 * v + 128);
            int g = (int)(y - 0.344f * u - 0.714f * v + 128);
            int b = (int)(y + 1.772f * u + 128);
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


        public static int[] RgbToYuv(int rgb, int add = 0)
        {
            int r = (rgb & 0x00ff0000) >> 16;
            int g = (rgb & 0x0000ff00) >> 8;
            int b = rgb & 0x000000ff;
            int y = (int)(0.2990 * r + 0.5870 * g + 0.1140 * b)-128+ add;
            int u = (int)(-0.1687 * r - 0.3313 * g + 0.5000 * b + add);
            int v = (int)(0.5000 * r - 0.4187 * g - 0.0813 * b + add);

            return new int[] { y, u, v };
                //(y << 16) | (u << 8) | v;
        }
    }
}
