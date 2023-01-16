using DAW.DFT;
using DAW.DftAnalysis;
using DAW.Transcription;
using PitchDetector;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DAW.Utils
{
    internal static class CalcDftStat
    {
        internal static PlotData? DftDistribution;


        public static List<string> CalcStats(IntRange? range, PlotData signal, PlotData pitchData, int sampleRate)
        {
            //int length = 30;
            //double[] stats = new double[length*2];
            // var_n+1 = [var_n + (avg_n-x_n+1)^2 * n/(n^2-1)]*(n-1)/n
            //PeriodFit? pf;
            //XY[] dft;
            //DftDataViewModel dftDataViewModel;
            //int count = 0;
            //int period;
            List<string> lines = new List<string>();


            float[][] floats = GetDfts(range, signal, pitchData, sampleRate);
            //float[] floats;




            //List<List<float>> floatList = new();
            //for(int i = 0; i < 30; i++)
            //    floatList.Add(new List<float>());

            //if (pitchData!.Data != null && range != null)
            //    for(int i = 0; i < pitchData.Data!.Length; i++)
            //    {
            //        pf = pitchData.Data[i] as PeriodFit;
            //        if(pf != null && pf.Sample-pf.Period >= range.Value.Start &&
            //            pf.Sample < range.Value.End)
            //        {
            //            period = FindExactPeriod(signal.Y, pf.Sample, pf.Period);
            //            dft = Dft.CalcDft(signal.Y, pf.Sample- period, period);
            //            dftDataViewModel = new DftDataViewModel(dft, sampleRate, period);
            //            floats = dftDataViewModel.Data!.Y.ToList().GetRange(0, 30).ToArray();
            //            lines.Add(string.Join(',', floats));

            //            for (int j = 0; j < 30; j++)
            //                floatList[j].Add(floats[j]);
            //            count++;
            //        }
            //    }

            //if(count> 1)
            //    for (int j = 0; j < length; j++)
            //    {
            //        stats[j] /= count;
            //    }

            //for (int i = length; i < stats.Length; i++)
            //    stats[i] = Math.Sqrt(stats[i]);

            List<List<float>> data = new List<List<float>>();

            for (int i = 0; i < 30; i++)
                data.Add(new List<float>());

            foreach (var arr in floats)
            {
                lines.Add(string.Join(',', arr));

                for (int j =10; j < 30; j++)
                    data[j].Add(arr[j]);
            }

            return lines;
        }

        public static float[][] GetDfts(IntRange? range, PlotData signal, PlotData pitchData, int sampleRate)
        {
            List<float[]> floatList = new();

            PeriodFit? pf;
            XY[] dft;
            DftDataViewModel dftDataViewModel;
            int period;
            float[] floats;

            if (pitchData!.Data != null && range != null)
                for (int i = 0; i < pitchData.Data!.Length; i++)
                {
                    pf = pitchData.Data[i] as PeriodFit;
                    if (pf != null && pf.Sample - pf.Period >= range.Value.Start &&
                        pf.Sample < range.Value.End)
                    {
                        period = FindExactPeriod(signal.Y, pf.Sample, pf.Period);
                        dft = Dft.CalcDft(signal.Y, pf.Sample - period, period);
                        dftDataViewModel = new DftDataViewModel(dft, sampleRate, period);
                        floats = dftDataViewModel.Data!.Y.ToList().GetRange(0, 30).ToArray();
                        floatList.Add(floats);
                    }
                }

            return floatList.ToArray();
        }

        public static PlotData CreateDftDistribution(List<List<float>> data)
        {
            float[] y= data.Select(f => f.Average()).ToArray();
            FloatRange yRange = new FloatRange(-3, 0);
            ValueDistribution[] distributions = data.Select(d =>
                GetDistribution(d, 8)).ToArray();
            return new PlotData(y, yRange, new FloatRange(0, data.Count), null, null, distributions);
        }

        public static ValueDistribution GetDistribution(List<float> data, int quantiles)
        {
            var list = data.ToList();
            list.Sort();
            List<FloatRange> quantileList = new List<FloatRange>();
            int indexFrom = 0;
            int count = data.Count / quantiles;
            for(int q = 0; q < quantiles; q++)
            {
                if(q == quantiles -1)
                {
                    quantileList.Add(new FloatRange(list[indexFrom], list[list.Count-1]));
                }
                else
                {
                    quantileList.Add(new FloatRange(list[indexFrom], list[indexFrom + count]));
                }
                indexFrom += count;
            }
            return new ValueDistribution(
                new FloatRange(list[0], list[list.Count - 1]),
                quantileList.ToArray());
        }

        public static int FindExactPeriod(float[] signal, int sample, int period)
        {
            float x1 = signal[sample];
            int result = period;
            float x = signal[sample - result];
            int k;
            for(int j = 1; j < 5; j++)
            {
                k = sample - period - j;
                if(k >= 0 && Math.Abs(signal[k]-x1) < Math.Abs(x1 - x))
                {
                    result = sample-k;
                    x = signal[k];
                }
                k = sample - period + j;
                if (Math.Abs(signal[k] - x1) < Math.Abs(x1 - x))
                {
                    result = sample - k;
                    x = signal[k];
                }
            }
            return result;
        }
    }
}
