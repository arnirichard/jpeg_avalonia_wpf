using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    public static class Dft
    {
        public static XY[] CalcDft(float[] samples, int sampleFrom, int length)
        {
            if(length <= 0)
                return new XY[0];

            double[] cos = new double[length];
            double[] sin = new double[length];

            for (int i = 0; i < length; i++)
            {
                cos[i] = Math.Cos(2 * Math.PI * i / length);
                sin[i] = Math.Sin(2 * Math.PI * i / length);
            }

            XY[] result = new XY[length / 2];

            if (result.Length == 0)
                return result;

            double x = 0, y;

            int sampleTo = sampleFrom + length;
            int sample;
            for (sample = sampleFrom; sample < sampleTo; sample++)
            {
                x += samples[sample];
            }
            result[0] = new XY() { X = (float)x, Y = 0 };

            if (result.Length == 1)
                return result;

            x = 0;
            y = 0;
            int n = 0;

            for (sample = sampleFrom; sample < sampleTo; sample++)
            {
                x += samples[sample] * cos[n];
                y += samples[sample] * sin[n];
                n++;
            }
            result[1] = new XY() { X = (float)x, Y = (float)y };

            int m = 2;

            for (; m < result.Length; m++)
            {
                n = 0;
                x = y = 0;
                for (sample = sampleFrom; sample < sampleTo; sample++)
                {
                    x += samples[sample] * cos[n];
                    y += samples[sample] * sin[n];
                    n = (n + m) % length;
                }
                result[m] = new XY() { X = (float)x, Y = (float)y };
            }

            return result;
        }
    }
}
