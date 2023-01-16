using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using MathNet.Numerics;

namespace DAW.Transcription
{
    internal class PhonemeModel
    {
        public static List<PhonemeModel> Models = new List<PhonemeModel>();

        public readonly string Name;
        public readonly string SourceName;
        // Contains dc term
        public float[] Average;
        public float[,] Corr;
        public float[,] Cov;
        public int[] Peaks;
        public int[] Sorted;
        public float[] StdDev;
        float[] smooth;
        float maxSmooth;
        float[] weight;
        public readonly DifferenceModel DiffModel;

        public PhonemeModel(string name, string sourceName,  float[] average, float[,] cov, DifferenceModel differenceModel)
        {
            Name = name;
            Average = average;
            Cov = cov;
            SourceName = sourceName;
            DiffModel = differenceModel;
            List<int> peaks = new List<int>();

            for (int i = 1; i < average.Length - 1; i++)
            {
                if (average[i] > average[i - 1] && average[i] > average[i + 1])
                {
                    peaks.Add(i);
                }
            }
            Peaks = peaks.ToArray();

            List<int> list = Enumerable.Range(1, average.Length - 1).ToList();

            list.Sort((int a, int b) =>
            {
                if (average[a] < average[b])
                    return 1;

                if (average[a] > average[b])
                    return -1;

                return 0;
            });
            Sorted = list.ToArray();
            Cov = cov;

            StdDev = new float[Average.Length];
            for(int i = 0; i < StdDev.Length; i++)
            {
                StdDev[i] = (float) Math.Sqrt(cov[i, i]);
            }
            smooth = new float[Average.Length];
            int from, to;
            for (int i = 0; i < smooth.Length; i++)
            {
                from = Math.Max(0, i - 2);
                to = Math.Min(smooth.Length, i + 2);
                for(int j = from; j < to; j++)
                {
                    smooth[i] += Average[j];
                }
                smooth[i] = smooth[i] / (to - from);
            }
            maxSmooth = smooth.Max();
            weight = new float[smooth.Length];
            for (int i = 0; i < weight.Length; i++)
            {
                weight[i] = Average[i] - smooth[i];
            }
            float weightMax = weight.Max();
            for (int i = 0; i < weight.Length; i++)
            {
                weight[i] -= weightMax;
                weight[i] = (float)Math.Pow(5, weight[i]);
            }

            //List<PhonemeTree>? list2;
            //string key;
            //int c;
            //foreach (var pt in tree.GetAllSubBranches())
            //{
            //    key = pt.CombinedKey;
            //    if(!combinedTrees.TryGetValue(key, out list2))
            //    {
            //        list2 = combinedTrees[key] = new List<PhonemeTree>();
            //    }
            //    list2.Add(pt);

            //    //foreach(var b in pt.Bins)
            //    //{
            //    //    if(b.Type == BinType.LocalPeak)
            //    //    {
                        
            //    //        localPeaks[b.Index] = localPeaks.TryGetValue(b.Index, out c)
            //    //            ? c + 1
            //    //            : 1;
            //    //    }
            //    //}
            //}

            //foreach(var ps in samples)
            //{
            //    foreach(var b in ps.Atts[2])
            //    {
            //        localPeaks[b.Index] = localPeaks.TryGetValue(b.Index, out c)
            //            ? c + 1
            //            : 1;
            //    }
            //}

            Corr = new float[Average.Length, Average.Length];

            for (int i = 0; i < Average.Length; i++)
                for (int j = 0; j < Average.Length; j++)
                    Corr[i, j] = (float)(Cov[i, j] / Math.Sqrt(Cov[i, i]) / Math.Sqrt(Cov[j, j]));
        }

        public double CalcDistance(float[] harmonics)
        {
            double res = DiffModel.FollowsModel(harmonics);
            if (res > 0)
            {
                if(SourceName == "E1")
                {

                }

                return res * 10;
            }

            //if(!IsInTree(harmonics))
            //{
            //    return double.PositiveInfinity;
            //}

            double result = 0;
            MathNet.Numerics.Distributions.Normal dist = new MathNet.Numerics.Distributions.Normal();
            double wSum = 0;

            for (int i = 0; i < Math.Min(harmonics.Length, Average.Length); i++)
            {
                if (StdDev[i] == 0)
                    result += harmonics[i] == Average[i] ? 0 : 0.5;
                else
                {
                    wSum += weight[i];
                    result += weight[i] * Math.Abs(0.5 - dist.CumulativeDistribution((harmonics[i] - Average[i]) / StdDev[i]));
                }
            }

            if (wSum == 0)
                return double.PositiveInfinity;

            return result / wSum;


            //double mult = 1;
            //int counter = 0;
            //int length = Math.Min(harmonics.Length, Average.Length)-1;

            ////if(peak != null && harmonics[peak.Value]< 0)
            ////{
            ////    return double.MinValue;
            ////}          
            //double weight;    

            //for (int i = 1; i < length; i++)
            //{
            //    weight = 1;

            //    if(i > 1 && i +1 < length)
            //        weight = Math.Min(0, Math.Min(Average[i] - Average[i+1], Average[i] - Average[i-1]));

            //    result += Math.Abs(harmonics[i] - Average[i]) * Math.Pow(10, weight);
            //    counter++;
            //    //if ((lastPeak && Average[i - 1]- Average[i] < 0.08) ||
            //    //    (Average[i] > Average[i - 1] && Average[i] > Average[i + 1])
            //    //    //||(harmonics[i] > harmonics[i - 1] && harmonics[i] > harmonics[i + 1])
            //    //    )
            //    //{

            //    //    result -= Math.Abs(harmonics[i] - Average[i]); // * Math.Pow(5, Average[i]);
            //    //    lastPeak = true;
            //    //    counter++;
            //    //}
            //    //else lastPeak = false;
            //}

            //if (counter > 0)
            //    result /= counter;

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

            //return result;
        }

        //private bool IsInTree(float[] harmonics)
        //{
        //    PhonemeSample pm = new PhonemeSample(harmonics);

        //    if (!combinedTrees.ContainsKey(pm.GetCombinedKey(0)))
        //        return false;

        //    if (!combinedTrees.ContainsKey(pm.GetCombinedKey(1)))
        //        return false;

        //    if (pm.Atts[(int)BinType.LocalPeak].Any(p => !localPeaks.ContainsKey(p.Index)))
        //        return false;


        //    //if (!combinedTrees.ContainsKey(pm.GetCombinedKey(2)))
        //    //    return false;

        //    //if (!combinedTrees.ContainsKey(pm.GetCombinedKey(3)))
        //    //    return false;

        //    //if (!combinedTrees.ContainsKey(pm.GetCombinedKey(4)))
        //    //    return false;

        //    return true;
        //}

        public override string ToString()
        {
            return Name + "-" + SourceName;
        }

        // data[0] is the bands omitting dc term
        public static PhonemeModel? FromData(string name, float[][] data)
        {
            if(data.Length == 0) return null;

            float[] avg = new float[data[0].Length];
            float[,] cov = new float[avg.Length, avg.Length];

            float[] row = data[0];

            for (int i = 0; i < row.Length; i++)
                avg[i] = row[i];

            float fraction;
            List<double> flots = data.Select(d => (double)d[0]).ToList();
            //var stddev = MathNet.Numerics.Statistics.Statistics.MeanStandardDeviation(flots);


            for (int i = 1; i < data.Length; i++)
            {
                row = data[i];
                fraction = 1 / (float)(i+1);

                for (int j = 0; j < row.Length; j++)
                {
                    
                    // https://stats.stackexchange.com/questions/310680/sequential-recursive-online-calculation-of-sample-covariance-matrix
                    // D_n = X_n - avg
                    // COV_n = (n-2)/(n-1)*COV_n-1 + avg*avgT/n
                    for (int k = j; k < row.Length; k++)
                    {
                        cov[j, k] = (1-fraction)*cov[j,k] + fraction* (row[j] - avg[j]) * (row[k] - avg[k]);
                    }
                }

                for (int k = 0; k < row.Length; k++)
                {
                    avg[k] = (1 - fraction) * avg[k] + fraction * row[k];
                }
            }

            for (int j = 0; j < row.Length; j++)
            {
                for (int k = j; k < row.Length; k++)
                {
                    cov[k, j] = cov[j, k];
                }
            }

            PhonemeModel resutl = new PhonemeModel(name.Substring(0,1), name, avg, cov, DifferenceModel.CreateModel(data));

            return resutl;
        }

        //public double CalcDistance(PhonemeModel pm)
        //{
        //    if(Name == "U" )
        //    {
        //        return pm.IsU() ? 0 : double.PositiveInfinity;
        //    }

        //    return CalcDistance(pm.Average);
        //}

        private bool IsU()
        {
            if (Sorted.Length < 4)
                return false;
            if (Sorted[0] > 3 || Sorted[1] > 3 || Sorted[2] > 3)
                return false;

            if ((Average[1] + Average[2] + Average[3]) / 3 < -0.29)
                return false;

            if (Peaks[0] == 2 && (Average[1] + Average[2] + Average[3]) / 3 < -0.25)
            {
                return false;
            }

            if (!Peaks.Contains(2) && !Peaks.Contains(3))
                return false;

            if (Peaks.Contains(5) && Average[5] > -0.5)
                return false;

            if (!Peaks.Contains(8) && !Peaks.Contains(9) && !Peaks.Contains(10))
                return false;

            if (!Peaks.Contains(15) && !Peaks.Contains(16) && !Peaks.Contains(17))
                return false;

            if (!Peaks.Contains(22) && !Peaks.Contains(23) && !Peaks.Contains(21) && !Peaks.Contains(24) && !Peaks.Contains(25) && !Peaks.Contains(26))
                return false;

            if (Average[4] > -0.57)
                return false;

            //if (Peaks.Contains(5) || Peaks.Contains(11) || Peaks.Contains(18))
            //    return false;

            return true;
        }
    }
}
