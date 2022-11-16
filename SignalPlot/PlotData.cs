using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalPlot
{
    public class PlotData
    {
        public float[] Y { get; private set; }
        public FloatRange YRange { get; private set; }
        public FloatRange XRange { get; private set; }
        public float AbsPeak { get; private set; }
        public float[]? X { get; private set; }
        public object[]? Data { get; set; }

        public PlotData(float[] y, FloatRange yRange, FloatRange xRange, float[]? x = null, object[]? data = null)
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
    }
}
