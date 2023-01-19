using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Equalization
{
    public class EqualizerBand
    {
        public float Frequency { get; set; }
        public float Gain { get; set; }
        public float Bandwidth { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}dB",
                (int)Frequency,
                Gain);
        }
    }

    public class Equalizer
    {
        private readonly int sampleRate;
        private readonly EqualizerBand[] bands;
        private readonly BiquadFilter[,] filters;
        private readonly int bandCount;
        private bool updated;

        public Equalizer(int sampleRate, EqualizerBand[] bands)
        {
            this.bands = bands;
            this.sampleRate = sampleRate;
            bandCount = bands.Length;
            filters = new BiquadFilter[1, bands.Length];
            CreateFilters();
        }

        private void CreateFilters()
        {
            for (int bandIndex = 0; bandIndex < bandCount; bandIndex++)
            {
                var band = bands[bandIndex];
                if (filters[0, bandIndex] == null)
                    filters[0, bandIndex] = BiquadFilter.PeakingEQ(sampleRate, band.Frequency, band.Bandwidth, band.Gain);
                else
                    filters[0, bandIndex].SetPeakingEq(sampleRate, band.Frequency, band.Bandwidth, band.Gain);
            }
        }

        public void Update()
        {
            updated = true;
            CreateFilters();
        }

        public void Filter(float[] buffer, int offset, int count)
        {
            if (updated)
            {
                CreateFilters();
                updated = false;
            }

            for (int n = 0; n < count; n++)
            {
                for (int band = 0; band < bandCount; band++)
                {
                    buffer[offset + n] = filters[0, band].Transform(buffer[offset + n]);
                }
                //buffer[offset + n] = filters[0, 15].Transform(buffer[offset + n]);
            }   
        }
    }
}
