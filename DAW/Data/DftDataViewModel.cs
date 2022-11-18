using DAW.Utils;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DAW.DFT
{
    public class DftDataViewModel
    {
        XY[] dft;
        int length;

        public PlotData? Data { get; }
        public float AvgSamplePower { get; }
        public float EvenPower { get; }
        public double DC { get; }
        public double SPL { get; }
        public float EvenPerc { get; }


        public DftDataViewModel(XY[] dft, int sampleRate, int length)
        {
            this.dft = dft;
            this.length = length;

            float[] power = dft.Select(d => d.Power).ToArray();
            float totalPower = power.Sum();

            AvgSamplePower = totalPower / length;
            DC = Math.Sqrt(dft[0].Power);

            if (totalPower > 0)
            {
                float[] perc = power.Select(p => p / totalPower * 100).ToArray();

                Data = new PlotData(perc,
                    new FloatRange(0, perc.Max()),
                    new FloatRange(0, power.Length));

                for(int i = 2; i < power.Length; i+=2)
                {
                    EvenPower += power[i];
                }
                EvenPerc = EvenPower / totalPower;
            }
        }
    }
}
