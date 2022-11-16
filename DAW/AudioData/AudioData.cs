using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW
{
    public class AudioData
    {
        public readonly float[][] ChannelData;
        public readonly WaveFormat Format;

        public AudioData(float[][] channelData, WaveFormat format)
        {
            ChannelData = channelData;
            Format = format;
        }

        public static AudioData? ReadSamples(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            if (fileName.EndsWith(".pcm"))
            {
                return ReadPcm(fileName);
            }

            AudioFileReader audioFileReader = new AudioFileReader(fileName);
            long numberOfSamples = audioFileReader.Length / audioFileReader.WaveFormat.BitsPerSample * 8;


            float[] data = new float[numberOfSamples];
            if(data.Length > 0)
                audioFileReader.Read(data, 0, data.Length);
            float[][] audioData = new float[audioFileReader.WaveFormat.Channels][];
            float[] mono;

            if (audioFileReader.WaveFormat.Channels == 1)
                audioData[0] = data;
            else
            {
                for (int i = 0; i < audioFileReader.WaveFormat.Channels; i++)
                {
                    mono = audioData[i] = new float[(int)(numberOfSamples / audioFileReader.WaveFormat.Channels)];
                    for (int j = 0; j < mono.Length; j++)
                        mono[j] = data[j * audioFileReader.WaveFormat.Channels];
                }
            }

            return new AudioData(audioData, audioFileReader.WaveFormat);
        }

        static AudioData ReadPcm(string fileName)
        {
            byte[] data = File.ReadAllBytes(fileName);

            //Array.Reverse(data);

            float[] samples = new float[data.Length / 2];
            int index = 0;
            int offset = 0; // data.Length - 2;

            float divisor = 32767;

            while (offset < data.Length - 1)
            {
                samples[index++] = BitConverter.ToInt16(data, offset) / divisor;
                offset += 2;
            }


            WaveFormat waveFormat = new WaveFormat(20050, 1);
            float[][] audioData = new float[1][];
            audioData[0] = samples;

            return new AudioData(audioData, waveFormat);
        }
    }
}
