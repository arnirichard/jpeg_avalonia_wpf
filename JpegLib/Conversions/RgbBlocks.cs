//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JpegLib
//{
//    public static class RgbBlocks
//    {
//        public static int[] ConvertRgbBlocksToArray(int[][] rgbBlocks, JfifHeader jfif)
//        {
//            int[] result = new int[jfif.Width * jfif.Height];
//            int x = 0;
//            int y = -8;
//            int rows, columns;
//            int[] block;
//            for (int b = 0; b < rgbBlocks.Length; b++)
//            {
//                block = rgbBlocks[b];
//                if (b % jfif.BlocksWidth == 0)
//                {
//                    x = 0;
//                    y += 8;
//                }
//                else
//                {
//                    x += 8;
//                }
//                rows = Math.Min(jfif.Height, y + 8) - y;
//                columns = Math.Min(jfif.Width, x + 8) - x;
//                for (int r = 0; r < rows; r++)
//                {
//                    Array.Copy(block, r * 8, result, (y + r) * jfif.Width, columns);
//                }
//            }
//            return result;
//        }
//    }
//}
