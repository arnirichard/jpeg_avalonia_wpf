using PitchDetector;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    internal static class CreatePitchPlotData
    {
        public static PlotData GetPitchPlotData(float[] samples, int sampleRate)
        {
            PitchTracker pitchTracker = new PitchTracker(sampleRate / 300, sampleRate / 80, sampleRate);

            for (int i = 0; i < samples.Length; i++)
            {
                pitchTracker.AddSample(samples[i]);
            }

            PitchTrackerDataNew data = pitchTracker.Data;

            PeriodFit[] validPerioFits = data.PeriodFits.Where(p => p.IsValid).ToArray();

            if (validPerioFits.Length > 2)
            {
                PeriodFit p0 = validPerioFits[0], p1 = validPerioFits[1], p2;
                p0.Pitch = (p0.Pitch + p1.Pitch) / 2;
                for (int i = 2; i < validPerioFits.Length; i++)
                {
                    p2 = validPerioFits[i];

                    if (p1.Period > p0.Period &&
                        p1.Period > p2.Period &&
                        p2.Sample - p0.Sample < 200)
                    {
                        p1.Pitch = (p0.Pitch + p2.Pitch) / 2;
                    }
                    else if (p1.Period < p0.Period &&
                        p1.Period < p2.Period &&
                        p2.Sample - p0.Sample < 200)
                    {
                        p1.Pitch = (p0.Pitch + p2.Pitch) / 2;
                    }
                    else
                    {
                        p1.Pitch = (p0.Pitch + p1.Pitch + p2.Pitch) / 3;
                    }

                    p0 = p1;
                    p1 = p2;
                }
            }

            float[] pitch = new float[samples.Length];
            int lastIndex = int.MinValue;
            float lastFreq = 0;
            //float freq;
            float delta, ratio;
            float pi;
            float minPitch = float.MaxValue;
            float maxPitch = float.MinValue;

            Array.Clear(pitch);

            int lastPeriod = (int)(sampleRate / minPitch);
            float freq;
            int freqsIndex = 0;
            float[] freqs = new float[50];
            float weightedFreq = 100;


            foreach (var p in validPerioFits)
            {
                if (p.IsValid)
                {
                    if (p.Sample < lastIndex + lastPeriod)
                    {
                        delta = 1 / ((float)p.Sample - lastIndex);
                        ratio = 0;
                        for (int i = lastIndex; i <= p.Sample; i++)
                        {
                            freq = pi = (1 - ratio) * lastFreq + ratio * p.Pitch;
                            weightedFreq -= freqs[freqsIndex % freqs.Length] / freqs.Length;
                            freqs[freqsIndex++ % freqs.Length] = freq;
                            weightedFreq += freq / freqs.Length;
                            pitch[i] = weightedFreq;
                            minPitch = Math.Min(minPitch, pi);
                            maxPitch = Math.Max(maxPitch, pi);
                            ratio += delta;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < freqs.Length; i++)
                            freqs[i] = p.Pitch;
                        weightedFreq = p.Pitch;
                        if (lastFreq == 0)
                            minPitch = maxPitch = p.Pitch;
                        else
                        {
                            minPitch = Math.Min(minPitch, p.Pitch);
                            maxPitch = Math.Max(maxPitch, p.Pitch);
                        }
                        pitch[p.Sample] = p.Pitch;
                    }

                    lastIndex = p.Sample;
                    lastFreq = pitch[p.Sample];
                    lastPeriod = p.Period;
                }
            }

            return new PlotData(pitch,
                new FloatRange(80, 300),
                new FloatRange(0, pitch.Length / (float)sampleRate));
        }
    }
}
