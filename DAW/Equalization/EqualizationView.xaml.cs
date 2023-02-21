using DAW.DFT;
using DAW.PitchDetector;
using DAW.Utils;
using Microsoft.VisualBasic.Logging;
using NAudio.Dsp;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
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

namespace DAW.Equalization
{
    /// <summary>
    /// Interaction logic for EqualizationView.xaml
    /// </summary>
    public partial class EqualizationView : UserControl
    {
        public EqualizationView()
        {
            InitializeComponent();

            List<LinesDefinition> verticalLines = new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 50),
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 50),
                new LinesDefinition(0, 0.05f, false, Plot.Beige, 50),
                new LinesDefinition(0, 0.01f, false, Plot.Beige, 50),
                new LinesDefinition(0, 0.001f, false, Plot.Beige, 50),
            };

            signalPlot.VerticalLines.AddRange(verticalLines);
            signalPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 20),
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 20),
            });


            filteredSignalPlot.VerticalLines.AddRange(verticalLines);
            filteredSignalPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 20),
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 20),
            });

            var pd = DependencyPropertyDescriptor.FromProperty(Plot.CurrentDataPointProperty, typeof(Plot));
            pd.AddValueChanged(signalPlot, OnCurrentValueChanged);

            responsePlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 10f, false, Plot.Beige, 20),
                new LinesDefinition(0, 5f, false, Plot.Beige, 20),
                new LinesDefinition(0, 1f, false, Plot.Beige, 10),
            });
            responsePlot.VerticalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(100, 100f, false, Plot.Beige, 40),
                new LinesDefinition(500, 500f, false, Plot.Beige, 40),
                new LinesDefinition(1000, 1000f, false, Plot.Beige, 40),
            });

            signalDftPlot.VerticalLines.Add(new LinesDefinition(0, 1, false, Plot.Beige));
            signalDftPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 10, false, Plot.Beige),
                new LinesDefinition(0, 5, false, Plot.Beige, 10),
                new LinesDefinition(0, 1, false, Plot.Beige, 7)
            });
            filteredSignalDftPlot.VerticalLines.Add(new LinesDefinition(0, 1, false, Plot.Beige));
            filteredSignalDftPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 10, false, Plot.Beige),
                new LinesDefinition(0, 5, false, Plot.Beige, 10),
                new LinesDefinition(0, 1, false, Plot.Beige, 7)
            });
        }

        private void OnCurrentValueChanged(object? sender, EventArgs e)
        {
            if(signalPlot.DataContext is PlotData plotData &&
                signalPlot.CurrentDataPoint != null &&
                DataContext is EqualizationViewModule evm &&
                evm.Signal?.PitchPlotData != null)
            {
                float pitch = evm.Signal.PitchPlotData.Y[signalPlot.CurrentDataPoint.Index];

                pitchText.Text = pitch > 0 ? pitch.ToString("N1")+"Hz" : "";

                if (pitch> 0)
                {
                    int period = (int)(evm.Signal.Format.SampleRate / pitch);
                    XY[] dft = Dft.CalcDft(plotData.Y, signalPlot.CurrentDataPoint.Index-period, period);
                    var dftData = new DftDataViewModel(dft, evm.Signal.Format.SampleRate, period);
                    evm.SetSignalDft(dftData.Data);

                    if(evm.FilteredSignal != null)
                    {
                        dft = Dft.CalcDft(evm.FilteredSignal.Y, signalPlot.CurrentDataPoint.Index - period, period);
                        var dftData2 = new DftDataViewModel(dft, evm.Signal.Format.SampleRate, period);
                        evm.SetFilteredDft(dftData2.Data);
                    }
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(sender is Slider fe &&
                fe.DataContext is Gain gain &&
                DataContext is EqualizationViewModule vm)
            {
                var gain2 = vm.GainMap.Where(g => g.Frequency == gain.Frequency).FirstOrDefault();
                if(gain2 != null)
                {
                    Gain newGain = new Gain(gain.Frequency, Math.Round(fe.Value * 10) / 10);
                    vm.GainMap[vm.GainMap.IndexOf(gain2)] = newGain;
                    Filter();
                }
            }
        }

        private void signalPlot_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Filter();
        }

        void Filter()
        {
            if(DataContext is EqualizationViewModule vm &&
                vm.Signal != null)
            {
                
                float[] processed = new float[vm.Signal.SignalPlotData.Y.Length];
                Array.Copy(vm.Signal.SignalPlotData.Y, processed, processed.Length);
                Equalizer eq = GetEqualizer(vm.Signal.Format.SampleRate, vm.GainMap);
                eq.Filter(processed, 0, processed.Length);
                processed.Normalize(0.9f);
                vm.SetFilteredSignal(new PlotData(processed, new FloatRange(-1, 1), new FloatRange(0, processed.Length)));
            }
        }

        private void StopPlaying_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EqualizationViewModule vm)
            {
                vm.Player?.Stop();
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EqualizationViewModule vm)
            {
                var signal = sender == filtPlay ? vm.FilteredSignal?.Y : vm.Signal?.SignalPlotData.Y;

                if(signal != null)
                {
                    vm.Player?.Play(signal, vm.Signal.Format.SampleRate);
                }
            }
        }

        private void GenerateRespone_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EqualizationViewModule vm &&
                vm.Signal != null)
            {
                Equalizer eq = GetEqualizer(vm.Signal.Format.SampleRate, vm.GainMap);
                float freq = 20;
                List<float> xList = new();
                List<float> yList = new();
                float[] tone;
                int period;
                float maxAbs, abs;
                float initialAmp = 0.1f;
                float y;
                float minY = 0, maxY = 0;
                while(freq < 20000)
                {
                    period = (int)(vm.Signal.Format.SampleRate / freq);
                    tone = Tone.GenerateTone(freq, vm.Signal.Format.SampleRate, (int)(vm.Signal.Format.SampleRate / freq * 5), initialAmp);
                    eq.Filter(tone, 0, tone.Length);

                    maxAbs = 0;
                    for(int i = tone.Length-1; i > tone.Length-period-1; i--)
                    {
                        abs = tone[i];
                        if (abs < 0)
                            abs = -1;
                        if (abs > maxAbs)
                            maxAbs = abs;
                    }

                    xList.Add(freq);
                    y = (float)(20 * Math.Log10(maxAbs / initialAmp));
                    yList.Add(y);
                    if (y < minY)
                        minY = y;
                    if(y > maxY)
                        maxY = y;
                    freq *= 1.02f;
                }
                if (minY < -30)
                    minY = -30;

                responsePlot.DataContext = new PlotData(yList.ToArray(), new FloatRange(minY - 1, maxY + 1),
                    new FloatRange(xList[0], xList.Last()), xList.ToArray());
            }
        }

        Equalizer GetEqualizer(int sampleRate, ObservableCollection<Gain> gainMap)
        {
            List<EqualizerBand> bands = new List<EqualizerBand>();
            Gain g;
            Gain? last = null, next = null;
            float upper, lower;
            for (int i = 0; i < gainMap.Count; i++)
            {
                g = gainMap[i];
                next = i + 1 < gainMap.Count ? gainMap[i + 1] : null;
                lower = last == null ? 0 : (g.Frequency + last.Frequency) / 2;
                upper = (float)(next == null ? g.Frequency * 1.2 : (next.Frequency + g.Frequency) / 2);
                bands.Add(new EqualizerBand()
                {
                    Frequency = (upper + lower) / 2,
                    Gain = (float)g.Decibel,
                    Bandwidth = 3f, //(upper - lower),
                });
                last = g;
            }

            Equalizer eq = new Equalizer(sampleRate,
                bands.ToArray());
            return eq;
        }
    }
}
