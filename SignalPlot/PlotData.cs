using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalPlot
{
    public class DataPoint
    {
        public float X { get; }
        public float Y { get; }
        public object? Data { get; }
        public int Index { get; }
        public DataPoint(float x, float y, int index, object? data)
        {
            X = x;
            Y = y;
            Index = index;
            Data = data;
        }
    }

    public class PlotData
    {
        public float[] Y { get; }
        public FloatRange YRange { get; private set; }
        public FloatRange XRange { get; private set; }
        public float AbsPeak { get; }
        public float[]? X { get; }
        public object[]? Data { get; }

        public PlotData(float[] y, FloatRange yRange, FloatRange xRange, 
            float[]? x = null, object[]? data = null)
        {
            Y = y;
            YRange = yRange;
            XRange = xRange;
            AbsPeak = y.GetAbsPeak();
            X = x;
            Data = data;
        }

        public PlotData Clone()
        {
            return new PlotData(Y, YRange, XRange, X);
        }

        public void SetYRange(FloatRange yRange)
        {
            YRange = yRange;
        }

        public void SetXRange(FloatRange xRange)
        {
            XRange = xRange;
        }

        internal DataPoint? GetDataPoint(float x, float tolerance)
        {
            if (x >= XRange.Start &&
                x <= XRange.End &&
                X?.Length > 0)
            {
                int index = FloatsUtils.FindClosestIndex(X, x);
                if(index > -1 && Math.Abs(X[index] - x) <= tolerance)
                {
                    return new DataPoint(X[index], Y[index], index, Data?[index]);
                }
            }

            return null;
        }
    }
}
