using NAudio.MediaFoundation;
using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DAW.PitchDetector
{
    public partial class PitchDetectorView : UserControl
    {
        public PitchDetectorView()
        {
            InitializeComponent();

            List<LinesDefinition> verticalLines = new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 50),
                new LinesDefinition(0, 0.05f, false, Plot.Beige, 50),
                new LinesDefinition(0, 0.01f, false, Plot.Beige, 50),
                new LinesDefinition(0, 0.001f, false, Plot.Beige, 50),
            };

            pitchPlot.VerticalLines.AddRange(verticalLines);
            signalPlot.VerticalLines.AddRange(verticalLines);
            signalPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 50),
            });
            pitchPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 50, false, Plot.Beige, 50),
                new LinesDefinition(0, 10, false, Plot.Beige, 50),
            });

            var pd = DependencyPropertyDescriptor.FromProperty(Plot.CurrentValueProperty, typeof(Plot));
            pd.AddValueChanged(pitchPlot, OnCurrentValueChanged);
        }

        private void OnCurrentValueChanged(object? sender, EventArgs e)
        {
            float? val = pitchPlot.CurrentValue;
            periodTextBlock.Text = val > 0 && DataContext is PitchDetectorViewModel vm && vm.Format != null
                    ? "Period: " + ((int)Math.Round(vm.Format.SampleRate / (float)val))
                    : "";
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PitchDetectorViewModel vm && vm.SignalPlotData?.Y.Length > 0)
            {
                IWaveProvider provider = new RawSourceWaveStream(
                            new MemoryStream(GetSamplesWaveData(vm.SignalPlotData.Y)), 
                            new WaveFormat(vm.Format.SampleRate, 16, 1));

                WaveOut waveOut = new WaveOut();
                waveOut.Init(provider);
                waveOut.Play();
            }
        }

        private void PlaySelected_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PitchDetectorViewModel vm && 
                vm.SignalPlotData?.Y.Length > 0 &&
                signalPlot.SelectedStartIndex != null &&
                signalPlot.SelectedEndIndex != null)
            {
                int fromIndex = Math.Min(signalPlot.SelectedStartIndex.Value, signalPlot.SelectedEndIndex.Value);
                int length = Math.Abs(signalPlot.SelectedStartIndex.Value - signalPlot.SelectedEndIndex.Value);

                if (fromIndex >= 0 && fromIndex+length <= vm.SignalPlotData.Y.Length)
                {
                    IWaveProvider provider = new RawSourceWaveStream(
                                new MemoryStream(GetSamplesWaveData(vm.SignalPlotData.Y, fromIndex, length)),
                                new WaveFormat(vm.Format.SampleRate, 16, 1));

                    WaveOut waveOut = new WaveOut();
                    waveOut.Init(provider);
                    waveOut.Play();
                }
            }
        }

        public static byte[] GetSamplesWaveData(float[] samples, int? offset = null, int? length = null)
        {
            int startIndex = offset ?? 0;
            int len = Math.Min(length ?? samples.Length, samples.Length - startIndex);

            var pcm = new byte[len*2];
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

        
    }
}
