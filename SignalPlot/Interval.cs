using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalPlot
{
    public struct FloatRange
    {
        public float Start { get; }
        public float End { get; }
        public float Length => End - Start;

        public FloatRange(float start, float end)
        {
            Start = start; 
            End = end;
        }
    }

    public struct SampleInterval
    {
        public int Start { get; }
        public int Length { get; }

        public int End => Start + Length;

        public SampleInterval()
        {
            Start = 0;
            Length = 0;
        }

        public SampleInterval(int start, int length)
        {
            Start = start; 
            Length = length;
        }
    }
}
