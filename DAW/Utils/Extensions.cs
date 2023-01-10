using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    internal static class Extensions
    {
        public static bool Equals(this IntRange? range1, IntRange? range2)
        {
            if (range1 == null && range2 == null)
                return true;

            if (range1 == null || range2 == null)
                return true;

            return range1!.Equals(range2!);
        }
    }
}
