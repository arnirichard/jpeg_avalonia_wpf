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
                double amp = Math.Sqrt(TotalPower);
                float[] perc = power.Select(p => (float) Math.Log10(Math.Sqrt(p) / amp)).ToArray();

                float maxPerc = perc.Max();
                for (int i = 0; i < perc.Length; i++)
                    perc[i] -= maxPerc;
                //maxPerc = (float)(Math.Ceiling(maxPerc / 10) * 10);

                Data = new PlotData(perc,
                    new FloatRange(Math.Max(-3, perc.Min()), perc.Max()),
                    new FloatRange(0, power.Length));

                for(int i = 2; i < power.Length; i+=2)
                {
                    EvenPower += power[i];
                }
                EvenPerc = EvenPower / TotalPower;
            }
        }

        public double[] GetNormalisedAmps(int length)
        {
            double[] result = new double[length];
            double v;
            double maxValue = -5;
            for(int i = 0; i < length; i++)
            {
                result[i] = v = Data!.Y[i];
                if(v > maxValue)
                {
                    maxValue = v;
                }
            }
            for (int i = 0; i < length; i++)
            {
                result[i] -= maxValue;
            }

            return result;
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
