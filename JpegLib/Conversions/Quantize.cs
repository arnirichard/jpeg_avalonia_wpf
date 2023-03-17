using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class Quant
    {
        public static readonly int[] NoQuant = {
            1, 1, 1, 1, 1, 1, 1, 1 ,
            1, 1, 1, 1, 1, 1, 1, 1 ,
            1, 1, 1, 1, 1, 1, 1, 1 ,
            1, 1, 1, 1, 1, 1, 1, 1 ,
            1, 1, 1, 1, 1, 1, 1, 1 ,
            1, 1, 1, 1, 1, 1, 1, 1 ,
            1, 1, 1, 1, 1, 1, 1, 1 ,
            1, 1, 1, 1, 1, 1, 1, 1 ,
        };

        public static readonly int[] QuantLuminance = {
            16, 11, 10, 16, 24, 40, 51, 61 ,
            12, 12, 14, 19, 26, 58, 60, 55,
            14, 13, 16, 24, 40, 57, 69, 56,
            14, 17, 22, 29, 51, 87, 80, 62,
            18, 22, 37, 56, 68, 109, 103, 77,
            24, 35, 55, 64, 81, 104, 113, 92,
            49, 64, 78, 87, 103, 121, 120, 101,
            72, 92, 95, 98, 112, 100, 103, 99
        };

        // chroma quantization table
        public static readonly int[] QuantChrominance = {
            17, 18, 24, 27, 47, 99, 99, 99,
            18, 21, 26, 66, 99, 99, 99, 99,
            24, 26, 56, 99, 99, 99, 99, 99,
            47, 66, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99
        };

        public static int[] Quantize(int[] ints, int[] quantTable)
        {
            int[] result = new int[ints.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ints[i] / quantTable[i];
            }

            return result;
        }

        public static int[] Dequantize(int[] ints, int[] quantTable)
        {
            int[] result = new int[ints.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ints[i] * quantTable[i];
            }

            return result;
        }

        public static int Dequantize(int number, int index, int[] quantTable)
        {
            return number * quantTable[index];
        }
    }
}
