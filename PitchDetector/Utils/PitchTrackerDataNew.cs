using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchDetector
{
    public class PitchTrackerDataNew
    {
        const int CrossingHistoryLength = 30;

        float[] signal;
        internal int TotalSamples = 0;
        public List<PeriodFit> PeriodFits = new List<PeriodFit>();
        PeriodFit? lastValidPeriodicFit = null;
        int SampleRate;
        int maxPeriod; // should match the lowest detectable frequency
        int minPeriod;
        float lastSample = 0;

        int[] crossingSampleNoHistoriesUp; // history of crossing sample numbers
        int crossingSampleNoHistoryLastIndexUp; // last index in history
        int[] crossingSampleNoHistoriesDown; // history of crossing sample numbers
        int crossingSampleNoHistoryLastIndexDown; // last index in history

        // Current
        int sampleIndex = 0;

        public PitchTrackerDataNew(int sampleRate, int minPeriod, int maxPeriod)
        {
            SampleRate = sampleRate;
            this.minPeriod = minPeriod;
            this.maxPeriod = maxPeriod;
            signal = new float[maxPeriod * 2];

            crossingSampleNoHistoriesUp = new int[CrossingHistoryLength];
            crossingSampleNoHistoriesDown = new int[CrossingHistoryLength];

            crossingSampleNoHistoriesUp = new int[CrossingHistoryLength];
            crossingSampleNoHistoriesDown = new int[CrossingHistoryLength];
        }

        public void NextSample(float sample)
        {
            // Update signal
            signal[sampleIndex % signal.Length] = sample;
            TotalSamples++;

            if (sample >= 0 != lastSample >= 0)
            {
                int[] sampleHistory = sample >= 0
                    ? crossingSampleNoHistoriesUp
                    : crossingSampleNoHistoriesDown;
                if(sampleHistory == crossingSampleNoHistoriesUp)
                    sampleHistory[++crossingSampleNoHistoryLastIndexUp % CrossingHistoryLength] = sampleIndex;
                else
                    sampleHistory[++crossingSampleNoHistoryLastIndexDown % CrossingHistoryLength] = sampleIndex;

                if (TotalSamples > signal.Length)
                {
                    PeriodFit? periodFit = DetectPeriodicity.TestForPeriodicity(sampleHistory,
                        (sampleHistory == crossingSampleNoHistoriesUp ? crossingSampleNoHistoryLastIndexUp : crossingSampleNoHistoryLastIndexDown) % CrossingHistoryLength,
                        minPeriod, maxPeriod, signal, SampleRate, GetLastPeriod());
                    if (periodFit != null)
                    {
                        periodFit.IsValid = IsValidPeridicFit(periodFit, lastValidPeriodicFit);

                        if (periodFit.IsValid)
                        {
                            PeriodFits.Add(periodFit);
                            lastValidPeriodicFit = periodFit;
                        }
                    }
                }
            }

            sampleIndex++;
            lastSample = sample;
        }

        int GetLastPeriod()
        {
            if(lastValidPeriodicFit != null)
            {
                if(sampleIndex - lastValidPeriodicFit.Sample > lastValidPeriodicFit.Period * 1.2)
                {
                    return 0;
                }

                return lastValidPeriodicFit.Period;
            }

            return 0;
        }

        private bool IsValidPeridicFit(PeriodFit periodFit, PeriodFit? last)
        {
            if (periodFit.SomeMeasure > 1 && periodFit.SameSignRatio < 0.8)
                return false;

            if (last == null)
                return true;

            if (periodFit.Sample > last.Sample + last.Period)
            {
                return true;
            }


            if (Math.Abs(periodFit.Period - last.Period) < 4)
                return periodFit.Deviation < last.Deviation * 10;

            return periodFit.Deviation < last.Deviation * 1.5;
        }
    }
}
