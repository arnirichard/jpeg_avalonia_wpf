using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalPlot
{
    public class ValueDistribution
    {
        public FloatRange Range { get; }
        public FloatRange[] Quantiles { get; }

        public ValueDistribution(FloatRange range, FloatRange[] quantiles)
        {
            Range = range;
            Quantiles = quantiles;
        }
    }

    public struct FloatRange
    {
        public float Start { get; }
        public float End { get; }
        public float Length => End - Start;
        public float Center => (End + Start) / 2;

        public FloatRange(float start, float end)
        {
            Start = start; 
            End = end;
        }

        public bool IsWithinRange(float val)
        {
            return val >= Start && val <= End;
        }

        public double DistFromRange(float val)
        {
            if(val < Start)
            {
                return (Start - val) / (End == Start ? 1 : Length);
            }
            else if(val > End)
            {
                return (val - End)/ (End == Start ? 1 : Length);
            }

            return  -(Length/2 -Math.Abs(val-Center))/ (End == Start ? 1 : Length);
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Start.ToString(), End.ToString());
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

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            IntRange? range = obj as IntRange?;
            if(range != null)
            {
                return Start == range!.Value.Start && End == range!.Value.End;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Start, End);
        }
    }
}
