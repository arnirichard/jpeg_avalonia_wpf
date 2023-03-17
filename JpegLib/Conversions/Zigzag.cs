using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class Zigzag
    {
        public static readonly int[] ZIGZAG = {
            0,  1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63,
        };

        public static int[] Zigzagize(int[] data)
        {
            int[] result = new int[data.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = data[ZIGZAG[i]];
            }
            return result;
        }

        public static int[] DeZigzagize(int[] data)
        {
            int[] result = new int[data.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[ZIGZAG[i]] = data[i];
            }
            return result;
        }
    }
}
