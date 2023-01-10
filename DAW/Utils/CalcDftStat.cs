using DAW.DFT;
using PitchDetector;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace DAW.Utils
{
    internal static class CalcDftStat
    {
        public static string CalcStats(IntRange? range, PlotData signal, PlotData pitchData, int sampleRate)
        {
            int length = 30;
            double[] stats = new double[length*2];
            // var_n+1 = [var_n + (avg_n-x_n+1)^2 * n/(n^2-1)]*(n-1)/n
            PeriodFit? pf;
            XY[] dft;
            DftDataViewModel dftDataViewModel;
            int count = 0;
            double value;
            Dictionary<int, int> localMaxMap = new();
            Dictionary<int, double> localMaxDiff = new();
            int v;
            double d1, d2, dv;

            if (pitchData!.Data != null && range != null)
                for(int i = 0; i < pitchData.Data!.Length; i++)
                {
                    pf = pitchData.Data[i] as PeriodFit;
                    if(pf != null && pf.Sample-pf.Period >= range.Value.Start &&
                        pf.Sample < range.Value.End)
                    {
                        dft = Dft.CalcDft(signal.Y, pf.Sample-pf.Period, pf.Period);
                        dftDataViewModel = new DftDataViewModel(dft, sampleRate, pf.Period);
                        double max = dftDataViewModel.Data!.Y.Max();
                        
                        for (int j = 0; j < length; j++)
                        {
                            value = dftDataViewModel.Data!.Y[j]-max;
                            if (count > 0)
                                stats[j + 30] = (stats[j + 30] +
                                    Math.Pow(stats[j] / count - value, 2) *
                                    1 / (count +1)) *
                                    count / (count+1);
                            if(j > 1 && j < length && 
                                (d1 = dftDataViewModel.Data!.Y[j] - dftDataViewModel.Data!.Y[j-1])>0 &&
                                (d2 = dftDataViewModel.Data!.Y[j] - dftDataViewModel.Data!.Y[j+1])>0)
                            {
                                localMaxMap.TryGetValue(j, out v);
                                localMaxMap[j] = 1 + v;
                                localMaxDiff.TryGetValue(j, out dv);
                                localMaxDiff[j] = dftDataViewModel.Data!.Y[j]+dv;
                            }
                            stats[j] += value;
                        }
                        count++;
                    }
                }

            if(count> 1)
                for (int j = 0; j < length; j++)
                {
                    stats[j] /= count;
                }

            for (int i = length; i < stats.Length; i++)
                stats[i] = Math.Sqrt(stats[i]);

            var str = string.Join('\n', stats.ToList().GetRange(0, length).Select(y => y.ToString()));
            //var list = localMaxMap.ToList().OrderByDescending(kvp => kvp.Value).ToList();
            //for(int i = 0; i < list.Count; i++)
            //{
            //    double prop = list[i].Value / (double)count;
            //    if (prop < 0.1)
            //        break;
            //    str += "\n" + list[i].Key + "=" + prop.ToString("0.##");
            //}
            return str;
        }
    }
}
