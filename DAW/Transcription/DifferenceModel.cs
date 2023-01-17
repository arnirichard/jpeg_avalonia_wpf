using DAW.Utils;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DAW.Transcription
{
    class DifferenceModel
    {
        // Includes DC term
        ValueDistribution[] Distributions;
        ValueDistribution[][] CrossDistributions;

        public DifferenceModel(ValueDistribution[] distributions,
                ValueDistribution[][] crossDistributions) 
        { 
            Distributions = distributions;
            CrossDistributions = crossDistributions;
        }

        public double FollowsModel(float[] floats)
        {
            ValueDistribution[] cross;
            double result = 0;
            

            for (int i = 1; i < Math.Min(floats.Length, Distributions.Length); i++)
            {
                //if (!Distributions[i].Range.IsWithinRange(floats[i]))
                //{
                result += Distributions[i].Range.DistFromRange(floats[i]); // * (Distributions[i].Range.Length == 0 ? 1000 : 1/ Distributions[i].Range.Length);
                //}

                cross = CrossDistributions[i];

                for (int j = 1; j < Math.Min(floats.Length, Distributions.Length); j++)
                {
                    if (!cross[j].Range.IsWithinRange(floats[j] - floats[i]))
                        result += cross[j].Range.DistFromRange(floats[j] - floats[i]);
                            //* (cross[j].Range.Length == 0 ? 1000 : 1 / cross[j].Range.Length) *
                            //(cross[j].Range.End <= 0 ? 5 : 1);
                     
                }
            }

            return result;
        }

        public static DifferenceModel CreateModel(float[][] floats)
        {
            List<List<float>> data = new List<List<float>>();

            for (int i = 0; i < 30; i++)
                data.Add(new List<float>());

            foreach (var arr in floats)
            {
                for (int j = 0; j < 30; j++)
                    data[j].Add(arr[j]);
            }

            ValueDistribution[] distributions  = data.Select(arr => CalcDftStat.GetDistribution(arr, 8)).ToArray();
            List<ValueDistribution[]> crossDistributions = new List<ValueDistribution[]>();

            for(int i = 0; i < distributions.Length; i++)
            {
                List<ValueDistribution> dist = new List<ValueDistribution>();
                List<float> list = data[i];

                for (int j = 0; j < distributions.Length; j++)
                {
                    List<float> list2 = data[j];
                    List<float> result = new List<float>();
                    for (int k = 0; k < list.Count; k++)
                        result.Add(list2[k] - list[k]);
                    dist.Add(CalcDftStat.GetDistribution(result, 8));
                }

                crossDistributions.Add(dist.ToArray());
            }

            return new DifferenceModel(distributions, crossDistributions.ToArray());
        }
    }
}
