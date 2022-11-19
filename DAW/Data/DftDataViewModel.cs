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
        float[] power;

        public PlotData? Data { get; }
        public float AvgSamplePower { get; }
        public float EvenPower { get; }
        public double DC { get; }
        public double SPL { get; }
        public float EvenPerc { get; }
        public float F0 { get; }

        public float TotalPower { get; }

        public DftDataViewModel(XY[] dft, int sampleRate, int length)
        {
            this.dft = dft;
            this.length = length;
            F0 = sampleRate/ length;

            power = new float[dft.Length];
            power[0] = dft[0].Power;
            for(int i = 1; i < dft.Length; i++)
            {
                power[i] = dft[i].Power * 2;
            }

            TotalPower = power.Sum();

            AvgSamplePower = TotalPower / length / length;

            DC = Math.Sqrt(dft[0].Power);

            if (TotalPower > 0)
            {
                float[] perc = power.Select(p => p / TotalPower * 100).ToArray();

                Data = new PlotData(perc,
                    new FloatRange(0, perc.Max()),
                    new FloatRange(0, power.Length));

                for(int i = 2; i < power.Length; i+=2)
                {
                    EvenPower += power[i];
                }
                EvenPerc = EvenPower / TotalPower;
            }
        }

        public float GetPower(IntRange range)
        {
            float result = 0;

            for(int i = range.Start; i < range.End; i++)
            {
                result += power[i];
            }

            return result;
        }
    }
}
