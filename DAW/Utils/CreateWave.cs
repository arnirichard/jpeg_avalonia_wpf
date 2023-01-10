using DAW.PitchDetector;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    public static class CreateWave
    {
        public static void WriteSignalViewModel(SignalViewModel record)
        {
            WriteSingleChannelWave(record.File.FullName, record.Format, record.SignalPlotData.Y);
        }

        public static void WriteSingleChannelWave(string filepath, WaveFormat waveFormat, float[] samples)
        {
            if(waveFormat.Channels > 1)
            {
                waveFormat = WaveFormat.CreateCustomFormat(
                    waveFormat.Encoding,
                    waveFormat.SampleRate,
                    1,
                    waveFormat.AverageBytesPerSecond / waveFormat.Channels,
                    waveFormat.BlockAlign / waveFormat.Channels,
                    waveFormat.BitsPerSample);
            }

            using (var writer = new WaveFileWriter(filepath, waveFormat))
            {
                writer.WriteSamples(samples, 0, samples.Length);
            }
        }
    }
}
