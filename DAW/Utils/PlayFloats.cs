using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    internal class PlayFloats
    {
        public static void Play(float[] samples, int sampleRate, int? offset = null, int? length = null)
        {
            IWaveProvider provider = new RawSourceWaveStream(
            new MemoryStream(Get32BitSamplesWaveData(samples, offset, length)),
            new WaveFormat(sampleRate, 32, 1));

            WaveOut waveOut = new WaveOut();
            waveOut.Init(provider);
            waveOut.Play();
        }

        public static byte[] Get16BitSamplesWaveData(float[] samples, int? offset = null, int? length = null)
        {
            int startIndex = offset ?? 0;
            int len = Math.Min(length ?? samples.Length, samples.Length - startIndex);

            var pcm = new byte[len * 2];
            int sampleIndex = startIndex, pcmIndex = 0;
            short outsample;
            int toSampelIndex = startIndex + len;

            while (sampleIndex < toSampelIndex)
            {
                outsample = (short)(samples[sampleIndex++] * short.MaxValue);

                pcm[pcmIndex++] = (byte)(outsample & 0xff);
                pcm[pcmIndex++] = (byte)((outsample >> 8) & 0xff);
            }

            return pcm;
        }

        public static byte[] Get32BitSamplesWaveData(float[] samples, int? offset = null, int? length = null)
        {
            int startIndex = offset ?? 0;
            int len = Math.Min(length ?? samples.Length, samples.Length - startIndex);

            var pcm = new byte[len * 4];
            int sampleIndex = startIndex, pcmIndex = 0;
            int outsample;
            int toSampelIndex = startIndex + len;

            while (sampleIndex < toSampelIndex)
            {
                outsample = (int)(samples[sampleIndex++] * int.MaxValue);

                pcm[pcmIndex++] = (byte)(outsample & 0xff);
                pcm[pcmIndex++] = (byte)((outsample >> 8) & 0xff);
                pcm[pcmIndex++] = (byte)((outsample >> 16) & 0xff);
                pcm[pcmIndex++] = (byte)((outsample >> 24) & 0xff);
            }

            return pcm;
        }
    }
}
