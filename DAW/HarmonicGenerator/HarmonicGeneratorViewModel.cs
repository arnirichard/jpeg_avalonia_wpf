using DAW.Utils;
using MathNet.Numerics;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DAW.HarmonicGenerator
{
    class HarmoicGeneratorViewModel : ViewModelBase
    {
        public readonly int SampleRate = 20050;
        public Harmonic Harmonic { get; set; }
        public PlotData Period { get; set; }
        public PlotData Amplitudes { get; set; }
        public IPlayer? Player;

        public HarmoicGeneratorViewModel()
        {
            float[] weights = new float[20];
            for(int i = 0; i < weights.Length; i++)
            {
                weights[i] = i == 0 ? 0 : -5;
            }
            Harmonic = new Harmonic(weights, 138);
            Period = CreatPlotData();
            Amplitudes = new PlotData(weights,
                new FloatRange(-4, 0),
                new FloatRange(1, weights.Length));
        }

        public void SetHarmonic(Harmonic harmonic)
        {
            Harmonic = harmonic;
            Period = CreatPlotData();
            Amplitudes = new PlotData(Harmonic.Weights,
                new FloatRange(-4, 0),
                new FloatRange(1, Harmonic.Weights.Length));
            OnPropertyChanged("Period");
            OnPropertyChanged("Amplitudes");
        }

        PlotData CreatPlotData()
        {
            //PlotData[] ?
            float[] signal = Harmonic.CreateSignal(SampleRate);

            //int lengthInSamples = amplitudes == null
            //    ? period.Length
            //    : (int)(amplitudes.XRange.End * SampleRate);
            //lengthInSamples += (period.Length - lengthInSamples % period.Length);
            //int periods = lengthInSamples / period.Length;
            //float[] multiplier = new float[periods];
            //if (amplitudes == null)
            //{ 
            //  Array.Fill(multiplier, 1);
            //}
            //else
            //{
            //    float x, y;
            //    int p;
            //    for(int i = 0; i < amplitudes.X.Length; i++)
            //    {
            //        p = (int)(periods * amplitudes.X[i]/amplitudes.XRange.End);
            //        if(p >= 0 && p < period.Length)
            //        {
            //            multiplier[p] = amplitudes.Y[i];
            //        }
            //    }
            //}

            //float[] signal = new float[lengthInSamples];
            //float amp1 = multiplier[0], amp2= multiplier[0], amp;
            //int index = 0;
            //float delta = 1 / (float)period.Length;
            //float d;
            //for(int i = 0; i< periods;i++)
            //{
            //    if(i+1 < periods)
            //        amp2 = multiplier[i+1];
            //    amp = amp1;
            //    d = (amp2 - amp1) * delta;
            //    for (int j = 0; j < period.Length; j++)
            //    {
            //        signal[index++] = period[j] * amp;
            //        amp += d;
            //    }
            //    amp1 = amp2;
            //}

            return new PlotData(signal, new FloatRange(-1, 1), new FloatRange(0, signal.Length));
        }
    }
}
