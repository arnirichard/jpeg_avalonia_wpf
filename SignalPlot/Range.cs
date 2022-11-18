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

    public struct IntRange
    {
        public int Start { get; }
        public int End { get; }
        public int Length => End - Start;
        public IntRange()
        {
            Start = 0;
            End = 0;
        }

        public IntRange(int start, int length)
        {
            Start = start;
            End = start+length;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Start, End);
        }
    }
}
