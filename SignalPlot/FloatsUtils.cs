using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalPlot
{
    public static class FloatsUtils
    {
        public static float GetAbsPeak(this float[] floats, int? offset = null, int? count = null)
        {
            float peak = 0;
            int start = offset ?? 0;
            int length = count ?? floats.Length - start;
            int end  = start+length;
            if (end > floats.Length)
            {
                end = floats.Length;
            }
            float v;
            for(int i = start; i < end; i++)
            {
                v = floats[i];
                if (v < 0)
                    v = -v;
                if(v > peak) peak = v;
            }
            return peak;
        }

        // Values has increasing order
        public static int FindClosestIndex(float[] values, float search)
        {
            if (search <= values[0])
                return 0;
            if (search >= values[values.Length - 1])
                return values.Length - 1;

            int indFrom = 0, indTo = values.Length - 1;
            //float valFrom = values[indFrom], valTo = values[values.Length - 1];
            float val;
            int index;

            while (indTo - indFrom > 2)
            {
                index = (indFrom + indTo) / 2;
                val = values[index];
                if(search < val)
                {
                    indTo = index;
                }
                else
                {
                    indFrom = index;
                }
            }

            return Math.Abs(values[indFrom]-search) < Math.Abs(values[indTo] - search)
                ? indFrom
                : indTo;
        }
    }
}
