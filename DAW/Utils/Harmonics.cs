using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    class Harmonic
    {
        public double[] Weights { get; }
        public double Pitch { get; }

        public Harmonic(double[] weights, double pitch)
        {
            Weights = weights;
            Pitch = pitch;
        }

        public float[] CreatePeriod(int sampleRate, bool shift = true)
        {
            if(Pitch <= 0)
                return new float[0];

            float[] result = new float[(int)Math.Round(sampleRate / Pitch)];

            double w;
            //double totalWeight = Weights.Where(w => w > 0).Sum();
            List<double> weighs = new();
            double amp;
            for (int i = 0; i < Weights.Length; i++)
            {
                w = Weights[i];
                
                weighs.Add(amp = Math.Pow(10, w));

                for (int j = 0; j  < result.Length; j++)
                {
                    result[j] += (float)(amp * 0.9 * Math.Sin((i+1)*2 *j* Math.PI/result.Length));
                }
                
            }

            float maxAmplitude = result.Max();
            if(maxAmplitude > 0)
                for (int i = 0; i < Weights.Length; i++)
                {
                    result[i] *= 0.9f / maxAmplitude;
                }

            return result;
        }
    }
}
