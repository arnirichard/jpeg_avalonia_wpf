//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Net.NetworkInformation;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace DAW.Transcription
//{
//    enum BinType
//    {
//        GlobalPeak,
//        CloseGlobalPeak,
//        LocalPeak,
//        CloseLocalPeak,
//        SinglePeak
//    }

//    internal class BinAtt
//    {
//        public int Index;
//        public BinType Type;
//        public float? AbsAverage;
//        public float? AbsStdDev;
//        public float? PeakDiff; // Avg diff from peak (negative)
//        public float? PeakStdDev;

//        public override string ToString()
//        {
//            return string.Format("{0}:{1}", Index, Type);
//        }
//    }

//    class PhonemeSample
//    {
//        public float[] Samples;
//        public BinAtt[][] Atts;

//        public PhonemeSample(float[] samples)
//        {
//            Samples = samples;
//            Atts = new BinAtt[5][];
//            Atts[0] = GetTopPeak();
//            Atts[1] = GetCloseTopPeak();
//            Atts[2] = GetLocalPeaks(2);
//            Atts[3] = GetCloseLocalPeaks();
//            Atts[4] = GetLocalPeaks(1);
//        }

//        public string GetKey(int index)
//        {
//            return string.Join('_', Atts[index].Select(i => i.Index));
//        }

//        public string GetCombinedKey(int index)
//        {
//            List<int> result = new();

//            for(int i = 0;i <= index; i++)
//            {
//                result.AddRange(Atts[i].Select(a => a.Index));
//            }

//            return string.Join('_', result.OrderBy(a => a));
//        }

//        bool HasBin(int i)
//        {
//            foreach(var atts in Atts)
//            {
//                if (atts == null)
//                    continue;
//                if (atts.Any(a => a.Index == i))
//                    return true;
//            }
//            return false;
//        }

//        public BinAtt[] GetCloseLocalPeaks()
//        {
//            List<BinAtt> result = new List<BinAtt>();

//            BinAtt[] bins = Atts[2];

//            List<int> list = new List<int>();

//            foreach (var bin in bins)
//            {
//                list.Clear();

//                for (int i = bin.Index - 2; i < bin.Index; i++)
//                    if (i >= 0)
//                        list.Add(i);

//                for (int i = bin.Index+1; i <= bin.Index+2; i++)
//                    if (i < Samples.Length)
//                        list.Add(i);
                

//                foreach(var j in list)
//                {
//                    if (HasBin(j))
//                        continue;

//                    if (bin.AbsAverage - Samples[j] < 0.3)
//                    {
//                        result.Add(new BinAtt()
//                        {
//                            Index = j,
//                            Type = BinType.CloseLocalPeak,
//                            AbsAverage = Samples[j],
//                            PeakDiff = Samples[j]-bin.AbsAverage
//                        });
//                    }
//                }
//            }

//            return result.OrderBy(r => r.Index).ToArray();
//        }

//        public BinAtt[] GetLocalPeaks(int d)
//        {
//            List<BinAtt> result = new List<BinAtt>();

            
//            float value;
//            List<int> list = new List<int>();

//            for (int i = 0; i < Samples.Length; i++)
//            {
//                if (HasBin(i))
//                    continue;
//                list.Clear();

//                for (int j = i - d; j < i; j++)
//                {
//                    if (j >= 0)
//                        list.Add(j);
//                }

//                for (int j = i + 1; j <= i + d; j++)
//                {
//                    if (j < Samples.Length)
//                        list.Add(j);
//                }

//                value = Samples[i];
//                if (list.Count > 0 && list.All(j => Samples[j] < value))
//                {
//                    result.Add(new BinAtt()
//                    {
//                        Index = i,
//                        Type = d == 1 ? BinType.SinglePeak : BinType.LocalPeak,
//                        AbsAverage = Samples[i],
//                        PeakDiff = 0
//                    });
//                }
//            }

//            return result.ToArray();
//        }

//        public BinAtt[] GetTopPeak()
//        {
//            List<BinAtt> result = new List<BinAtt>();
//            float max = Samples.Max();
//            for(int i = 0; i < Samples.Length; i++)
//            {
//                if (Samples[i] == max)
//                {
//                    result.Add(new BinAtt()
//                    {
//                        Index = i,
//                        Type = BinType.GlobalPeak,
//                        AbsAverage = Samples[i],
//                        PeakDiff = 0
//                    });
//                }
//            }
//            return result.ToArray();
//        }

//        public BinAtt[] GetCloseTopPeak()
//        {
//            if (Atts[0].Length == 0)
//                return new BinAtt[0];
//            float peak = Atts[0][0].AbsAverage ?? 0;
//            List<BinAtt> result = new List<BinAtt>();
//            for (int i = 0; i < Samples.Length; i++)
//            {
//                if (peak-Samples[i] <0.5 && peak != Samples[i])
//                {
//                    result.Add(new BinAtt()
//                    {
//                        Index = i,
//                        Type = BinType.CloseGlobalPeak,
//                        AbsAverage = Samples[i],
//                        PeakDiff = Samples[i] - peak
//                    });
//                }
//            }
//            return result.ToArray();
//        }
//    }
//}
