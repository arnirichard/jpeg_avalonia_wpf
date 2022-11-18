using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    public static class Gaps
    {
        public static List<IntRange> FindGaps(float[] signal, float threshold, int minGapLength = 1000)
        {
            List<IntRange> result = new List<IntRange>();

            IntRange? currentGap = null;

            for (int i =  0; i < signal.Length; i++)
            {
                if (Math.Abs(signal[i]) >= threshold)
                {
                    if(currentGap != null)
                    {
                        if(currentGap.Value.Length >= minGapLength)
                            result.Add(currentGap.Value);
                        currentGap = null;
                    }
                }
                else 
                {
                    if (currentGap != null)
                        currentGap = new IntRange(currentGap.Value.Start, currentGap.Value.Length+1);
                    else
                        currentGap = new IntRange(i, 1);
                }
            }

            if (currentGap?.Length >= minGapLength)
                result.Add(currentGap.Value);

            return result;
        }
    }
}
