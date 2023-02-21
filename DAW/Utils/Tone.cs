using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    class Tone
    {
        public static float[] GenerateTone(float freq, int sampleRate, int length, float amplitude)
        {
            float[] tone = new float[length];
            int period = (int)(sampleRate / freq);

            for(int i = 0; i < length; i++)
            {
                tone[i]  =(float)( amplitude * Math.Sin(i * 2 * Math.PI / period));
            }

            return tone;
        }
    }
}
