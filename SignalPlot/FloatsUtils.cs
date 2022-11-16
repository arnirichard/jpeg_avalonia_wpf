using System;
using System.Collections.Generic;
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
    }
}
