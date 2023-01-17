using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    public static class FloatExtensions
    {
        public static void Normalize(this float[] samples, float toAmp)
        {
            float maxAmp = samples.Max(p => Math.Abs(p));
            if(maxAmp < 1)
            {
                return;
            }

            float scale = toAmp/maxAmp;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = samples[i] * scale;
            }
        }

        public static float[] Extrapolate(this float[] samples, int count)
        {
            float[] result = new float[count*samples.Length];

            for(int i = 0; i < count; i++)
            {
                Array.Copy(samples, 0, result, samples.Length * i, samples.Length);
            }

            return result;
        }

        public static void RampUp(this float[] samples, int start, int length)
        {
            int toSample = Math.Min(start + length, samples.Length);
            float delta = 1f/length;
            float value = 0;
            for(int i = start; i < toSample; i++)
            {
                samples[i] *= value;
                value += delta;
            }
        }

        public static void RampDown(this float[] samples, int start, int length)
        {
            int toSample = Math.Min(start + length, samples.Length);
            float delta = 1f / length;
            float value = 1;
            for (int i = start; i < toSample; i++)
            {
                samples[i] *= value;
                value -= delta;
            }
        }
    }
}
