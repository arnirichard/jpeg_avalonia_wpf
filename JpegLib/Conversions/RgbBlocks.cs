using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class RgbBlocks
    {
        public static int[] ConvertRgbBlocksToIntArray(int[][] rgbBlocks, int width, int height)
        {
            int[] result = new int[width * height];
            int blocksWidth = (width+7) / 8;
            int x = 0;
            int y = -8;
            int rows, columns;
            int[] block;
            for (int b = 0; b < rgbBlocks.Length; b++)
            {
                block = rgbBlocks[b];
                if (b % blocksWidth == 0)
                {
                    x = 0;
                    y += 8;
                }
                else
                {
                    x += 8;
                }
                rows = Math.Min(height, y + 8) - y;
                columns = Math.Min(width, x + 8) - x;
                for (int r = 0; r < rows; r++)
                {
                    Array.Copy(block, r * 8, result, (y + r) * width, columns);
                }
            }
            return result;
        }

        public static byte[] ConvertRgbBlocksToByteArray(int[][] rgbBlocks, int width, int height)
        {
            int bytesPerPixel = 4;
            byte[] result = new byte[width * height * bytesPerPixel];
            int blocksWidth = (width + 7) / 8;
            int x = 0;
            int y = -8;
            int rows, columns, rgb, destIndex;
            int[] block;

            for (int b = 0; b < rgbBlocks.Length; b++)
            {
                block = rgbBlocks[b];
                if (b % blocksWidth == 0)
                {
                    x = 0;
                    y += 8;
                }
                else
                {
                    x += 8;
                }
                rows = Math.Min(height, y + 8) - y;
                columns = Math.Min(width, x + 8) - x;
                for (int r = 0; r < rows; r++)
                {
                    destIndex = ((y + r) * width + x) * bytesPerPixel;
                    for (int c = 0; c < columns; c++)
                    {
                        rgb = block[r * 8 + c];
                        result[destIndex++] = (byte)(rgb & 0xff);
                        result[destIndex++] = (byte)((rgb & 0x00ff00) >> 8);
                        result[destIndex++] = (byte)((rgb & 0xff0000) >> 16); 
                        result[destIndex++] = 255;
                    }
                }
            }
            return result;
        }
    }
}
