using SignalPlot;
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
        public float[] Weights { get; }
        public double DefaultPitch { get; }
        public PlotData[]? Amplitudes { get; }
        public PlotData? Pitch;

        public Harmonic(float[] weights, double pitch, PlotData[]? amplitudes = null, PlotData? pitches = null)
        {
            Weights = weights;
            DefaultPitch = pitch;
            Pitch = pitches;
            Amplitudes = amplitudes;
        }

        public float[] CreateSignal(int sampleRate, int length = -1)
        {
            if(length == -1)
            {
                if (Pitch?.XRange != null)
                    length = (int)(Pitch.XRange.Length*sampleRate);
                else if (Amplitudes?.Length > 0)
                {
                    foreach (var a in Amplitudes)
                    {
                        length = (int)(a.XRange.End * sampleRate);
                    }
                }
                else
                {
                    length = (int)(sampleRate / DefaultPitch);
                }
            }

            float[] result = new float[length];

            double[] ws = new double[Weights.Length];
            for (int i = 0; i < Weights.Length; i++)
                ws[i] = Math.Pow(10, Weights[i]);

            double[] phases = new double[ws.Length];
            int[] periods = new int[length];
            if (Pitch != null)
                for (int i = 0; i < periods.Length; i++)
                {
                    periods[i] = (int)(sampleRate/Pitch.Y[i]);
                }
            else
                Array.Fill(periods, (int)(sampleRate / DefaultPitch));

            int period;
            double phaseDelta;
            float amp = 1;
            int ampIndex = -1, currentAmpIndex = -1;
            PlotData? zeroAmp = Amplitudes?.Length > 0
                ? Amplitudes[0]
                : null;
            float signalLengthSeconds = length / (float)sampleRate;
            float currentSeconds;

            // for each tone
            for (int i = 0; i < result.Length; i++)
            {
                period = periods[i];

                if (period == 0)
                    continue;

                phaseDelta = 2 * Math.PI / period;
                currentSeconds = i / (float)sampleRate;

                if (zeroAmp?.X != null)
                {
                    //currentAmpIndex = -1;

                    while (ampIndex +1 < zeroAmp.X.Length)
                    {
                        if(zeroAmp.X[ampIndex + 1] > currentSeconds)
                        {
                            break;
                        }
                        else
                        {
                            ampIndex++;
                            if (currentSeconds - zeroAmp.X[ampIndex] < 0.01)
                            {
                                currentAmpIndex = ampIndex;
                            }
                        }
                    }
                }


                for (int wi = 0; wi < ws.Length; wi++)
                {
                    amp = currentAmpIndex > -1 && wi < Amplitudes.Length
                        ? Amplitudes[wi].Y[currentAmpIndex]
                        : (Amplitudes != null ? 0 :1);

                    result[i] += (float)(amp * ws[wi] * Math.Sin(phases[wi]));
                    phases[wi] += phaseDelta*(wi+1);
                }
            }

            float maxAmplitude = result.Max();
            if (maxAmplitude > 0)
            {
                float scaling = 0.9f / maxAmplitude;
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] *= scaling;
                }
            }

            return result;
        }
    }
}
