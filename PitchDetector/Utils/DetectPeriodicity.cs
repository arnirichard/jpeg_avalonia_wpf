using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchDetector
{
    internal class DetectPeriodicity
    {
        public static PeriodFit? TestForPeriodicity(int[] crossingSamples, int index, int minPeriod,
            int maxPeriod, float[] signal, int samplerate, int lastPeriod)
        {
            int currentSample = crossingSamples[index];
            int i = index;
            int sample, period;
            PeriodFit? result = null;
            PeriodFit periodFit;
            double f;

            while (true)
            {
                i--;
                if (i < 0)
                    i += crossingSamples.Length;
                if (i == index)
                    break;
                sample = crossingSamples[i];
                period = currentSample - sample;

                if (period > maxPeriod)
                    break;

                if (period < minPeriod)
                    continue;

                periodFit = GetPeriodicFit(signal, samplerate, period, currentSample);

                periodFit.PrevPeriodRatio = GetRatio(period, lastPeriod);

                f = 1;

                if (periodFit.PrevPeriodRatio > 1.4)
                    f = 6;
                else if (periodFit.PrevPeriodRatio < 1.06)
                    f = .6;
                else if(periodFit.PrevPeriodRatio > 1.2)
                {
                    f = 2;
                }
                else if (periodFit.PrevPeriodRatio > 1.1)
                {
                    f = 1.5;
                }

                if (periodFit.PrevPeriodRatio > 1.9 && Math.Max(Math.Ceiling(f)-f, f-Math.Floor(f)) < 0.1)
                {
                    f = 10;
                }
                else if(f < 1 &&
                    result != null 
                    && result.PrevPeriodRatio < periodFit.PrevPeriodRatio)
                {
                    f = 1;
                }

                if (result == null || IsBetter(periodFit, result, f))
                    result = periodFit;
            }

            if (lastPeriod > 0)
            {
                periodFit = GetPeriodicFit(signal, samplerate, lastPeriod, currentSample);

                f = 1;

                if(result != null)
                {
                    double ratio = GetRatio(result.Period, lastPeriod);
                    f = result.Period < lastPeriod * 0.8 ? 0.3 : 1;

                    if (ratio > 1.9 && Math.Max(Math.Ceiling(f) - f, f - Math.Floor(f)) < 0.1)
                    {
                        f = 1/10;
                    }
                }

                if (result == null || IsBetter(periodFit, result, f: f))
                {
                    result = periodFit;
                    f = 1;
                }

                double last = periodFit.deviation;

                for (int j = 1; j < 3; j++)
                {
                    if (lastPeriod - j < minPeriod)
                        break;

                    PeriodFit periodFitNext = GetPeriodicFit(signal, samplerate, lastPeriod - j, currentSample);

                    if (result == null || IsBetter(periodFitNext, result, f: f))
                        result = periodFitNext;

                    if (periodFitNext.deviation > last)
                        break;

                    last = periodFitNext.deviation;
                }

                for (int j = 1; j < 3; j++)
                {
                    if (lastPeriod + j > maxPeriod)
                        break;

                    PeriodFit periodFitNext = GetPeriodicFit(signal, samplerate, lastPeriod + j, currentSample);

                    if (result == null || IsBetter(periodFitNext, result, f: f))
                        result = periodFitNext;

                    if (periodFitNext.deviation > last)
                        break;

                    last = periodFitNext.deviation;
                }
            }
            else if (result != null && result.Period / 2 >= minPeriod)
            {
                periodFit = GetPeriodicFit(signal, samplerate, result.Period / 2, currentSample);

                if (result == null || IsBetter(periodFit, result))
                    result = periodFit;
            }

            return result;
        }

        static double GetRatio(double nom, double denom)
        {
            if (denom == 0 || nom == 0)
                return 0;
            double ratio = nom / denom;
            return Math.Max(ratio, 1 / ratio);
        }

        static PeriodFit GetPeriodicFit(float[] signal, int samplerate, int period, int currentSample)
        {
            PeriodFit periodFit = new PeriodFit(period, currentSample, samplerate) { };

            int iterations = 30;
            int lag = (int)(period * 1.3) / iterations;
            if (lag < 1)
            {
                lag = 1;

            }
            float s1, s2;
            double diff;

            for (int k = 1; k <= iterations; k++)
            {
                s1 = signal[GetIndex(currentSample - k * lag, signal.Length)];
                s2 = signal[GetIndex(currentSample - k * lag - period, signal.Length)];
                periodFit.SumS1 += s1 < 0 ? -s1 : s1;
                periodFit.SumS2 += s2 < 0 ? -s2 : s2;

                if (s1 < 0 == s2 < 0)
                    periodFit.CountSameSign++;
            }

            double ratio = periodFit.SumS2 > 0 ? periodFit.SumS1 / periodFit.SumS2 : 1;

            for (int k = 1; k <= iterations; k++)
            {
                s1 = signal[GetIndex(currentSample - k * lag, signal.Length)];
                s2 = signal[GetIndex(currentSample - k * lag - period, signal.Length)];
                diff = s1 - s2 * ratio;
                periodFit.SumAbsDeltaDiff += Math.Abs(diff);

                periodFit.Count++;
            }

            return periodFit;
        }

        static int GetIndex(int ind, int length)
        {
            if (ind < 0)
                return (ind + length) % length;
            return ind % length;
        }

        static bool IsBetter(PeriodFit periodFit, PeriodFit than, double f = 1)
        {

            if(periodFit.RelativeIncrease < 0.5 || periodFit.RelativeIncrease > 1.6)
            {
                return false;
            }

            if (than.RelativeIncrease < 0.6 || than.RelativeIncrease > 1.5)
            {
                return true;
            }

            if (than.RelativeIncrease < 0.7 || than.RelativeIncrease > 1.3)
            {
                f /= 3;
            }

            return periodFit.deviation * f <= than.deviation;

        }
    }
}
