using MathNet.Numerics.LinearAlgebra;
using NAudio.CoreAudioApi;
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
        float gain = 194.40139137271677f;

        public override string ToString()
        {
            List<string> list = new List<string>();
            for(int i = 0; i < B.Length; i++)
            {
                list.Add(string.Format("b_{0} * x_n{1}",
                    i,
                    i == 0 ? "" : "_" + i));
            }
            
            for (int i = 1; i < A.Length; i++)
            {
                list.Add(string.Format("{0} * y_n{1}",
                    A[i],
                    i == 0 ? "" : "_" + i));
            }

            return string.Join('+', list);
        }

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
                bList[t.Power] = t.Coeff;

            int aCount = denominator.GetOrder();
            
            while (aList.Count <= aCount)
                aList.Add(0f);
            foreach (var t in denominator.Terms)
                aList[t.Power] = t.Coeff;

            A = aList.Select(c => (float)c.Real).ToArray();
            B = bList.Select(c => (float)c.Real).ToArray();
        }

        public void FilterOld(float[] floats)
        {

// "1.0150954633340554x_n+-1.9528145988251429x_n-1+1.0150954633340554x_n-2 - -1.9528145988251429y_n-1 - 0.9696653590187516y_n-2"


            A = new float[] { 1, 1.9528145988251429f , -0.96966535901875162f};
            B = new float[] { 1.0150954633340554f, -1.9528145988251429f, 0.95456989568469641f };

            float[] xv2 = new float[B.Length];
            float[] yv2 = new float[A.Length];
            float result2;

            for (int j = 0; j < floats.Length; j++)
            {
                result2 = 0;
                xv2[j % xv2.Length] = floats[j]; // / gain;

                for (int i = 0; i < B.Length; i++)
                    result2 += xv2[(j - i + B.Length) % B.Length] * B[i];

                for (int i = 1; i < A.Length; i++)
                    result2 += yv2[(j - i + A.Length) % A.Length] * A[i];
                
                yv2[j % yv2.Length] = result2;
                floats[j] = result2;
            }
        }

        public void Filter(float[] floats)
        {
            float[] xv2 = new float[B.Length];
            float[] yv2 = new float[A.Length];
            float result2;

            for (int j = 0; j < floats.Length; j++)
            {
                result2 = 0;
                xv2[j % xv2.Length] = floats[j]; // / gain;

                for (int i = 0; i < B.Length; i++)
                    result2 += xv2[(j+ i+1) % B.Length] * B[i];
                
                for (int i = 0; i < A.Length - 1; i++)
                    result2 -= yv2[(j + i + 1) % A.Length] * A[i];
                yv2[j%yv2.Length] = result2;
                floats[j] = result2;
            }
        }
    }
}
