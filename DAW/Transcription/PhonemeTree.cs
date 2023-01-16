//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace DAW.Transcription
//{
//    class PhonemeTree
//    {
//        public string Key;
//        public int Samples;
//        public PhonemeTree? Parent;
//        public BinType Type;
//        public BinAtt[] Bins;
//        public Dictionary<string, PhonemeTree> Branches = new();

//        PhonemeTree(int samples, PhonemeTree? parent, BinType type, BinAtt[] bins)
//        {
//            Samples = samples;
//            Parent = parent;
//            Type = type;
//            Bins = bins;
//            Key = string.Join('_', bins.Select(s => s.Index));
//        }

//        public List<int> GetCombinedIndexes()
//        {
//            List<int> result = new List<int>();

//            result.AddRange(Bins.Select(b => b.Index));

//            PhonemeTree? p = Parent;

//            while(p != null)
//            {
//                result.AddRange(p.Bins.Select(b => b.Index));
//                p = p.Parent;
//            }

//            result = result.OrderBy(r => r).ToList();

//            return result;
//        }

//        public List<PhonemeTree> GetAllSubBranches()
//        {
//            List<PhonemeTree> toProcess = new List<PhonemeTree>() { this };
//            List<PhonemeTree> next = new();
//            List<PhonemeTree> result = new List<PhonemeTree>();

//            while(toProcess.Count > 0)
//            {
//                foreach(var pt in toProcess)
//                {
//                    result.AddRange(pt.Branches.Values);
//                    next.AddRange(pt.Branches.Values);
//                }

//                toProcess = next;
//                next = new();
//            }

//            return result;
//        }

//        public string CombinedKey => string.Join('_', GetCombinedIndexes());

//        public override string ToString()
//        {
//            return string.Format("{0}:Count {1}",
//                Key, Samples);
//        }

//        public static PhonemeTree CreateTree(List<PhonemeSample> phonemes)
//        {
            
//            string key;
//            List<PhonemeSample>? list;
//            Dictionary<PhonemeTree, List<PhonemeSample>> toProcess = new();
//            Dictionary<PhonemeTree, List<PhonemeSample>> nextRound = new();
//            PhonemeTree result;
//            toProcess.Add(result = new PhonemeTree(phonemes.Count, null, BinType.GlobalPeak, new BinAtt[0]), phonemes);

//            int index = 0;

//            while (index < 5 && toProcess.Count > 0)
//            {

//                foreach (var kvp2 in toProcess)
//                {
//                    Dictionary<string, List<PhonemeSample>> dict = new Dictionary<string, List<PhonemeSample>>();

//                    foreach (var ps in kvp2.Value)
//                    {
//                        key = ps.GetKey(index);
//                        if (!dict.TryGetValue(key, out list))
//                            dict.Add(key, list = new List<PhonemeSample>());
//                        list.Add(ps);
//                    }
//                    foreach (var kvp in dict)
//                    {
//                        var atts = kvp.Value.First().Atts[index];
//                        BinAtt[] binatts = new BinAtt[atts.Length];
//                        for (int i = 0; i < atts.Length; i++)
//                        {
//                            var stast = MathNet.Numerics.Statistics.Statistics.MeanStandardDeviation(kvp.Value.Select(v => v.Atts[index][i].AbsAverage ?? 0));
//                            binatts[i] = new BinAtt()
//                            {
//                                Index = atts[i].Index,
//                                Type = (BinType)index,
//                                AbsAverage = (float)stast.Mean,
//                                AbsStdDev = (float)stast.StandardDeviation
//                            };
//                        }

//                        PhonemeTree pt = kvp2.Key.Branches[kvp.Key] = new PhonemeTree(kvp.Value.Count, kvp2.Key, BinType.GlobalPeak, binatts);

//                        if (kvp.Value.Count > 1)
//                        {
//                            nextRound[pt] = kvp.Value;
//                        }
//                    }
//                }

//                toProcess = nextRound;
//                nextRound = new();

//                index++;
//            }

//            return result;
//        }
//    }
//}
