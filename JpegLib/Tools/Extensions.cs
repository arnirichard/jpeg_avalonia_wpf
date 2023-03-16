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
    }
}
