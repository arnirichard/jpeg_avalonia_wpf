using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    internal static class Extensions
    {
        public static int BitLength(this int n)
        {
            n = n < 0 ? -n : n;
            int length = 0;
            while (n != 0)
            {
                length++;
                n >>= 1;
            }
            return length;
        }

        public static string Print(this ArraySegment<byte> arrSeg)
        {
            StringBuilder stringBuilder= new StringBuilder();
            for (int i = arrSeg.Offset; i < (arrSeg.Offset + arrSeg.Count); i++)
            {
                stringBuilder.Append(string.Format( "[{0}]:{1}"+Environment.NewLine, i, arrSeg.Array[i]));
            }
            return stringBuilder.ToString();
        }
    }
}
