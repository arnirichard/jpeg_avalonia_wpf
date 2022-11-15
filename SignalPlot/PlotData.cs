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
        public float MinY { get; private set; }
        public float MaxY { get; private set; }
        public float MinX { get; private set; }
        public float MaxX { get; private set; }

        public PlotData(float[] y, float minY, float maxY, float minX, float maxX)
        {
            Y = y;
            MinY = minY;
            MaxY = maxY;
            MaxX = maxX;
            MinX = minX;
        }

        public PlotData Clone()
        {
            return new PlotData(Y, MinY, MaxY, MinX, MaxX);
        }
    }
}
