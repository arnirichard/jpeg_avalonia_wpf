using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchDetector
{
    public class PitchTracker
    {       
        int maxPeriod; // should match the lowest detectable frequency
        int minPeriod;

        public readonly int SampleRate;
        public readonly PitchTrackerDataNew Data;
        public int TotalSamples => Data.TotalSamples;

        public PitchTracker(int minPeriod, int maxPeriod, int sampleRate)
        {
            this.maxPeriod = maxPeriod;
            this.minPeriod = minPeriod;
            SampleRate = sampleRate;

            Data = new PitchTrackerDataNew(sampleRate, minPeriod, maxPeriod);
        }

        public void AddSample(float sample)
        {
            Data.NextSample(sample);
        }
    }
}
