using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    internal class Smoother
    {
        public float[] Values;
        public float Estimate, LastEstimate;
        int index = 0;
        float sum;


        public Smoother(int length, float init)
        {
            LastEstimate = Estimate = init;
            Values = new float[length];
            for(int i = 0; i < length; i++)
                Values[i] = init;
            sum = Values.Length * init;
        }

        public float Add(float value)
        {
            index = ++index % Values.Length;
            sum -= Values[index];
            Values[index] = value;
            sum += value;

            float avg = sum/Values.Length;
            LastEstimate = Estimate;

            return Estimate = avg;
        }
    }
}
