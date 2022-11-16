using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalPlot
{
    public class PlotLine
    {
        public bool Vertical { get; private set; }
        public int Position { get; private set; }
        public float Value { get; private set; }
        public bool Solid { get; private set; }

        public PlotLine(bool vertical, int position, float value, bool solid)
        {
            Vertical = vertical;
            Position = position;
            Value = value;
            Solid = solid;
        }
    }
}
