using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DAW.FilterDesign
{
    class IIRFilter
    {
        List<Complex> poles;
        List<Complex> zeros;
        // H(z) = coeff * (1-z_1)*...(1-z_k) /  [ (1-p_1)*...*(1-p_m) ]
        // y_n = b_0 * x_n + b_1 * x_n-1 + ... + b_k * x_n-k + a_0 * y_n-1 + a_1 * y_n-2 + ... + a_m * y_n-m
        float[] A;
        float[] B;

        public IIRFilter(List<Complex> poles, List<Complex> zeros)
        {
            this.poles = poles;
            this.zeros = zeros;
            Polynomial nominator = new Polynomial();
            Polynomial denominator = new Polynomial();
            foreach (var pole in poles)
                denominator = denominator.Mult(Polynomial.FromRoot(pole));
            foreach (var zero in zeros)
                nominator = nominator.Mult(Polynomial.FromRoot(zero));

            int bCount = nominator.GetOrder();
            List<Complex> bList = new List<Complex>();
            List<Complex> aList = new List<Complex>();

            while (bList.Count <= bCount)
                bList.Add(0f);
            foreach (var t in nominator.Terms)
                bList[t.Power] = new Complex();

            int aCount = denominator.GetOrder();
            
            while (aList.Count <= aCount)
                aList.Add(0f);
            foreach (var t in denominator.Terms)
                aList[t.Power] = t.Coeff;

            A = aList.Select(c => (float)c.Real).ToArray();
            B = bList.Select(c => (float)c.Real).ToArray();
        }

        public void Filter(float[] floats)
        {
            float[] stateB = new float[B.Length];
            int indexB = 0;
            float[] stateA = new float[A.Length];
            int indexA = 0;
            float result;

            for(int i = 0; i < floats.Length; i++)
            {
                indexB = (indexB+1) % stateB.Length;
                indexA = (indexA+1) % stateA.Length;

                stateB[indexB] = floats[i];
                result = 0;
                for(int j = 0; j < stateB.Length; j++)
                {
                    result += B[j] * stateB[(indexB - j + stateB.Length) % stateB.Length];
                }
                for (int j = 1; j < stateA.Length; j++)
                {
                    result += A[j] * stateA[(indexA - j + stateA.Length) % stateA.Length];
                }
                
                floats[i] = stateA[indexA] = result;
                
            }
        }
    }
}
