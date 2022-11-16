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
        public float MaxPeak { get; private set; }

        public PlotData(float[] y, FloatRange yRange, FloatRange xRange)
        {
            Y = y;
            YRange = yRange;
            XRange = xRange;
            MaxPeak = y.GetAbsPeak();
        }

        public PlotData Clone()
        {
            return new PlotData(Y, YRange, XRange);
        }
    }
}
