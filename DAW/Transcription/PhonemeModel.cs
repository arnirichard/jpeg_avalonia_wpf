using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using MathNet.Numerics;

namespace DAW.Transcription
{
    internal class PhonemeModel
    {
        public static List<PhonemeModel> Models = new List<PhonemeModel>();

        public readonly string Name;
        public readonly string SourceName;
        double[] Average;
        //double[] StdDev;
        //int[] OrderedIndexes;
        int? peak;

        public PhonemeModel(string name, string sourceName,  double[] average)
        {
            Name = name;
            Average = average;
            SourceName = sourceName;
            //StdDev = stdDev;

            for (int i = 0; i < average.Length; i++)
            {
                if (average[i] > -0.01)
                {
                    peak = i;
                    break;
                }
            }

            List<int> list = Enumerable.Range(1, average.Length-1).ToList();

            list.Sort((int a, int b) =>
            {
                if (average[a] < average[b])
                    return -1;

                if (average[a] > average[b])
                    return 1;

                return 0;
            });
            //OrderedIndexes = list.ToArray();
        }

        public double CalcProp(double[] harmonics)
        {
            double result = 0;
            //double mult = 1;
            int counter = 0;
            int length = Math.Min(harmonics.Length, Average.Length)-1;

            if(peak != null && harmonics[peak.Value]< 0)
            {
                return double.MinValue;
            }
            bool lastPeak = false;
           
                
            for (int i = 1; i < length; i++)
            {
                result -= Math.Abs(harmonics[i] - Average[i]); // * Math.Pow(5, Average[i]);

                //if ((lastPeak && Average[i - 1]- Average[i] < 0.08) ||
                //    (Average[i] > Average[i - 1] && Average[i] > Average[i + 1])
                //    //||(harmonics[i] > harmonics[i - 1] && harmonics[i] > harmonics[i + 1])
                //    )
                //{

                //    result -= Math.Abs(harmonics[i] - Average[i]); // * Math.Pow(5, Average[i]);
                //    lastPeak = true;
                //    counter++;
                //}
                //else lastPeak = false;
            }

            if (counter > 0)
                result /= counter;

            //double result = 1;
            //int index;
            //MathNet.Numerics.Distributions.Normal dist = new MathNet.Numerics.Distributions.Normal();
            //double stddev;
            //double normalVal;
            //double w = 1 / (double)OrderedIndexes.Length;

            //for (int i = 0; i < OrderedIndexes.Length; i++)
            //{
            //    index = OrderedIndexes[i];
            //    stddev = Math.Max(0.1, StdDev[index]);
            //    normalVal = -Math.Abs((Average[index] - harmonics[index]) / stddev);
            //    result *= (0.75 + dist.CumulativeDistribution(normalVal)/2);
            //}

            return result;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
