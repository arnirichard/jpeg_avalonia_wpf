using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class DCT
    {
        public static int[] Forward(int[] input)
        {
            double[] output = new double[input.Length];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    output[x * 8 + y] = CalcDCT(input, x, y);
                }
            }
                
            return output.Select(o => (int)o).ToArray();
        }

        static double CalcDCT(int[] input, int x, int y)
        {
            double result = 0;
            double c = 1 / Math.Sqrt(2);

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    result += input[i * 8 + j] *
                        Math.Cos((2 * i + 1) * x * Math.PI / 16) *
                        Math.Cos((2 * j + 1) * y * Math.PI / 16);

                }
            }

            result *= (x == 0 ? c : 1) * (y == 0 ? c : 1) / 4;

            return result;
        }

        public static int[] Backward(int[] input)
        {
            double[] output = new double[input.Length];

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    output[y * 8 + x] = CalcIDCT(input, y, x);
                }
            }

            return output.Select(o => (int)o).ToArray();
        }

        static double CalcIDCT(int[] input, int y, int x)
        {
            double result = 0;
            double c = 1 / Math.Sqrt(2);

            for (int v = 0; v < 8; v++)
            {
                for (int u = 0; u < 8; u++)
                {
                    result += input[v * 8 + u] *
                        (v == 0 ? c : 1) * (u == 0 ? c : 1) *
                            Math.Cos((2 * x + 1) * u * Math.PI / 16) *
                            Math.Cos((2 * y + 1) * v * Math.PI / 16);
                }
            }

            result /= 4;

            return result;
        }

        static double m0 = 2.0 * Math.Cos(1.0 / 16.0 * 2.0 * Math.PI);
        static double m1 = 2.0 * Math.Cos(2.0 / 16.0 * 2.0 * Math.PI);
        static double m3 = 2.0 * Math.Cos(2.0 / 16.0 * 2.0 * Math.PI);
        static double m5 = 2.0 * Math.Cos(3.0 / 16.0 * 2.0 * Math.PI);
        static double m2 = m0 - m5;
        static double m4 = m0 + m5;
        static double s0 = Math.Cos(0.0 / 16.0 * Math.PI) / Math.Sqrt(8);
        static double s1 = Math.Cos(1.0 / 16.0 * Math.PI) / 2.0;
        static double s2 = Math.Cos(2.0 / 16.0 * Math.PI) / 2.0;
        static double s3 = Math.Cos(3.0 / 16.0 * Math.PI) / 2.0;
        static double s4 = Math.Cos(4.0 / 16.0 * Math.PI) / 2.0;
        static double s5 = Math.Cos(5.0 / 16.0 * Math.PI) / 2.0;
        static double s6 = Math.Cos(6.0 / 16.0 * Math.PI) / 2.0;
        static double s7 = Math.Cos(7.0 / 16.0 * Math.PI) / 2.0;

        public static int[] ForwardFast(int[] input)
        {
            double[] doubles = new double[input.Length];
            for(int i = 0; i< input.Length; i++)
            {
                doubles[i] = input[i];
            }
            ForwardDCTBlockComponent(doubles);
            int[] result = new int[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = (int)doubles[i];
            }
            return result;
        }

        // perform 1-D FDCT on all columns and rows of a block component
        //   resulting in 2-D FDCT
        public static void ForwardDCTBlockComponent(double[] component)
        {
            for (uint i = 0; i < 8; ++i)
            {
                double a0 = component[0 * 8 + i];
                double a1 = component[1 * 8 + i];
                double a2 = component[2 * 8 + i];
                double a3 = component[3 * 8 + i];
                double a4 = component[4 * 8 + i];
                 double a5 = component[5 * 8 + i];
                 double a6 = component[6 * 8 + i];
                 double a7 = component[7 * 8 + i];

                 double b0 = a0 + a7;
                 double b1 = a1 + a6;
                 double b2 = a2 + a5;
                 double b3 = a3 + a4;
                 double b4 = a3 - a4;
                 double b5 = a2 - a5;
                 double b6 = a1 - a6;
                 double b7 = a0 - a7;

                 double c0 = b0 + b3;
                 double c1 = b1 + b2;
                 double c2 = b1 - b2;
                 double c3 = b0 - b3;
                 double c4 = b4;
                 double c5 = b5 - b4;
                 double c6 = b6 - c5;
                 double c7 = b7 - b6;

                 double d0 = c0 + c1;
                 double d1 = c0 - c1;
                 double d2 = c2;
                 double d3 = c3 - c2;
                 double d4 = c4;
                 double d5 = c5;
                 double d6 = c6;
                 double d7 = c5 + c7;
                 double d8 = c4 - c6;

                 double e0 = d0;
                 double e1 = d1;
                 double e2 = d2 * m1;
                 double e3 = d3;
                 double e4 = d4 * m2;
                 double e5 = d5 * m3;
                 double e6 = d6 * m4;
                 double e7 = d7;
                 double e8 = d8 * m5;

                 double f0 = e0;
                 double f1 = e1;
                 double f2 = e2 + e3;
                 double f3 = e3 - e2;
                 double f4 = e4 + e8;
                 double f5 = e5 + e7;
                 double f6 = e6 + e8;
                 double f7 = e7 - e5;

                 double g0 = f0;
                 double g1 = f1;
                 double g2 = f2;
                 double g3 = f3;
                 double g4 = f4 + f7;
                 double g5 = f5 + f6;
                 double g6 = f5 - f6;
                 double g7 = f7 - f4;

                component[0 * 8 + i] = g0 * s0;
                component[4 * 8 + i] = g1 * s4;
                component[2 * 8 + i] = g2 * s2;
                component[6 * 8 + i] = g3 * s6;
                component[5 * 8 + i] = g4 * s5;
                component[1 * 8 + i] = g5 * s1;
                component[7 * 8 + i] = g6 * s7;
                component[3 * 8 + i] = g7 * s3;
            }
            for (uint i = 0; i < 8; ++i)
            {
                 double a0 = component[i * 8 + 0];
                 double a1 = component[i * 8 + 1];
                 double a2 = component[i * 8 + 2];
                 double a3 = component[i * 8 + 3];
                 double a4 = component[i * 8 + 4];
                 double a5 = component[i * 8 + 5];
                 double a6 = component[i * 8 + 6];
                 double a7 = component[i * 8 + 7];

                 double b0 = a0 + a7;
                 double b1 = a1 + a6;
                 double b2 = a2 + a5;
                 double b3 = a3 + a4;
                 double b4 = a3 - a4;
                 double b5 = a2 - a5;
                 double b6 = a1 - a6;
                 double b7 = a0 - a7;

                 double c0 = b0 + b3;
                 double c1 = b1 + b2;
                 double c2 = b1 - b2;
                 double c3 = b0 - b3;
                 double c4 = b4;
                 double c5 = b5 - b4;
                 double c6 = b6 - c5;
                 double c7 = b7 - b6;

                 double d0 = c0 + c1;
                 double d1 = c0 - c1;
                 double d2 = c2;
                 double d3 = c3 - c2;
                 double d4 = c4;
                 double d5 = c5;
                 double d6 = c6;
                 double d7 = c5 + c7;
                 double d8 = c4 - c6;

                 double e0 = d0;
                 double e1 = d1;
                 double e2 = d2 * m1;
                 double e3 = d3;
                 double e4 = d4 * m2;
                 double e5 = d5 * m3;
                 double e6 = d6 * m4;
                 double e7 = d7;
                 double e8 = d8 * m5;

                 double f0 = e0;
                 double f1 = e1;
                 double f2 = e2 + e3;
                 double f3 = e3 - e2;
                 double f4 = e4 + e8;
                 double f5 = e5 + e7;
                 double f6 = e6 + e8;
                 double f7 = e7 - e5;

                 double g0 = f0;
                 double g1 = f1;
                 double g2 = f2;
                 double g3 = f3;
                 double g4 = f4 + f7;
                 double g5 = f5 + f6;
                 double g6 = f5 - f6;
                 double g7 = f7 - f4;

                component[i * 8 + 0] = g0 * s0;
                component[i * 8 + 4] = g1 * s4;
                component[i * 8 + 2] = g2 * s2;
                component[i * 8 + 6] = g3 * s6;
                component[i * 8 + 5] = g4 * s5;
                component[i * 8 + 1] = g5 * s1;
                component[i * 8 + 7] = g6 * s7;
                component[i * 8 + 3] = g7 * s3;
            }
        }

        public static int[] InverseFast(int[] input)
        {
            double[] doubles = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                doubles[i] = input[i];
            }
            InverseDCTBlockComponent(doubles);
            int[] result = new int[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = (int)doubles[i];
            }
            return result;
        }

        // perform 1-D IDCT on all columns and rows of a block component
        //   resulting in 2-D IDCT
        public static void InverseDCTBlockComponent(double[]  component) 
        {
            double m0 = 2.0 * Math.Cos(1.0 / 16.0 * 2.0 * Math.PI);
            double m1 = 2.0 * Math.Cos(2.0 / 16.0 * 2.0 * Math.PI);
            double m3 = 2.0 * Math.Cos(2.0 / 16.0 * 2.0 * Math.PI);
            double m5 = 2.0 * Math.Cos(3.0 / 16.0 * 2.0 * Math.PI);
            double m2 = m0 - m5;
            double m4 = m0 + m5;


            double s0 = Math.Cos(0.0 / 16.0 * Math.PI) / Math.Sqrt(8);
            double s1 = Math.Cos(1.0 / 16.0 * Math.PI) / 2.0;
            double s2 = Math.Cos(2.0 / 16.0 * Math.PI) / 2.0;
            double s3 = Math.Cos(3.0 / 16.0 * Math.PI) / 2.0;
            double s4 = Math.Cos(4.0 / 16.0 * Math.PI) / 2.0;
            double s5 = Math.Cos(5.0 / 16.0 * Math.PI) / 2.0;
            double s6 = Math.Cos(6.0 / 16.0 * Math.PI) / 2.0;
            double s7 = Math.Cos(7.0 / 16.0 * Math.PI) / 2.0;

            for (uint i = 0; i< 8; ++i) {
                 double g0 = component[0 * 8 + i] * s0;
                 double g1 = component[4 * 8 + i] * s4;
                 double g2 = component[2 * 8 + i] * s2;
                 double g3 = component[6 * 8 + i] * s6;
                 double g4 = component[5 * 8 + i] * s5;
                 double g5 = component[1 * 8 + i] * s1;
                 double g6 = component[7 * 8 + i] * s7;
                 double g7 = component[3 * 8 + i] * s3;

                 double f0 = g0;
                 double f1 = g1;
                 double f2 = g2;
                 double f3 = g3;
                 double f4 = g4 - g7;
                 double f5 = g5 + g6;
                 double f6 = g5 - g6;
                 double f7 = g4 + g7;

                 double e0 = f0;
                 double e1 = f1;
                 double e2 = f2 - f3;
                 double e3 = f2 + f3;
                 double e4 = f4;
                 double e5 = f5 - f7;
                 double e6 = f6;
                 double e7 = f5 + f7;
                 double e8 = f4 + f6;

                 double d0 = e0;
                 double d1 = e1;
                 double d2 = e2 * m1;
                 double d3 = e3;
                 double d4 = e4 * m2;
                 double d5 = e5 * m3;
                 double d6 = e6 * m4;
                 double d7 = e7;
                 double d8 = e8 * m5;

                 double c0 = d0 + d1;
                 double c1 = d0 - d1;
                 double c2 = d2 - d3;
                 double c3 = d3;
                 double c4 = d4 + d8;
                 double c5 = d5 + d7;
                 double c6 = d6 - d8;
                 double c7 = d7;
                 double c8 = c5 - c6;

                 double b0 = c0 + c3;
                 double b1 = c1 + c2;
                 double b2 = c1 - c2;
                 double b3 = c0 - c3;
                 double b4 = c4 - c8;
                 double b5 = c8;
                 double b6 = c6 - c7;
                 double b7 = c7;

                component[0 * 8 + i] = b0 + b7;
                component[1 * 8 + i] = b1 + b6;
                component[2 * 8 + i] = b2 + b5;
                component[3 * 8 + i] = b3 + b4;
                component[4 * 8 + i] = b3 - b4;
                component[5 * 8 + i] = b2 - b5;
                component[6 * 8 + i] = b1 - b6;
                component[7 * 8 + i] = b0 - b7;
            }
            for (uint i = 0; i< 8; ++i) {
                 double g0 = component[i * 8 + 0] * s0;
             double g1 = component[i * 8 + 4] * s4;
             double g2 = component[i * 8 + 2] * s2;
             double g3 = component[i * 8 + 6] * s6;
             double g4 = component[i * 8 + 5] * s5;
             double g5 = component[i * 8 + 1] * s1;
             double g6 = component[i * 8 + 7] * s7;
             double g7 = component[i * 8 + 3] * s3;

             double f0 = g0;
             double f1 = g1;
             double f2 = g2;
             double f3 = g3;
             double f4 = g4 - g7;
             double f5 = g5 + g6;
             double f6 = g5 - g6;
             double f7 = g4 + g7;

             double e0 = f0;
             double e1 = f1;
             double e2 = f2 - f3;
             double e3 = f2 + f3;
             double e4 = f4;
             double e5 = f5 - f7;
             double e6 = f6;
             double e7 = f5 + f7;
             double e8 = f4 + f6;

             double d0 = e0;
             double d1 = e1;
             double d2 = e2 * m1;
             double d3 = e3;
             double d4 = e4 * m2;
             double d5 = e5 * m3;
             double d6 = e6 * m4;
             double d7 = e7;
             double d8 = e8 * m5;

             double c0 = d0 + d1;
             double c1 = d0 - d1;
             double c2 = d2 - d3;
             double c3 = d3;
             double c4 = d4 + d8;
             double c5 = d5 + d7;
             double c6 = d6 - d8;
             double c7 = d7;
             double c8 = c5 - c6;

             double b0 = c0 + c3;
             double b1 = c1 + c2;
             double b2 = c1 - c2;
             double b3 = c0 - c3;
             double b4 = c4 - c8;
             double b5 = c8;
             double b6 = c6 - c7;
             double b7 = c7;

            component[i * 8 + 0] = b0 + b7;
                component[i * 8 + 1] = b1 + b6;
                component[i * 8 + 2] = b2 + b5;
                component[i * 8 + 3] = b3 + b4;
                component[i * 8 + 4] = b3 - b4;
                component[i * 8 + 5] = b2 - b5;
                component[i * 8 + 6] = b1 - b6;
                component[i * 8 + 7] = b0 - b7;
            }
        }
    }
}
