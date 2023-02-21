using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    internal static class Extensions
    {
        public static bool Equals(this IntRange? range1, IntRange? range2)
        {
            if (range1 == null && range2 == null)
                return true;

            if (range1 == null || range2 == null)
                return true;

            return range1!.Equals(range2!);
        }

        public static string GetShortString(this WaveFormat waveFormat)
        {
            return string.Format("{0}Hz,{1}Ch,{2}bit",
                waveFormat.SampleRate,
                waveFormat.Channels,
                waveFormat.BitsPerSample);
        }
    }
}
