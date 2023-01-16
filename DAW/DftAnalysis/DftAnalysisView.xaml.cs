using DAW.DFT;
using DAW.Utils;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DAW.DftAnalysis
{
    public partial class DftAnalysisView : UserControl
    {
        float[][]? floats;

        public DftAnalysisView()
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

            dftPlot1.VerticalLines.Add(new LinesDefinition(0, 1, false, Plot.Beige));
            dftPlot1.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 10, false, Plot.Beige),
                new LinesDefinition(0, 5, false, Plot.Beige, 10),
                new LinesDefinition(0, 1, false, Plot.Beige, 7)
            });

            for (int i = 0; i < 30; i++)
                binCombo.Items.Add(new ComboBoxItem()
                {
                    Content = i
                });
            binCombo.SelectedIndex = 0;
        }

        private void calcDftStats_Click(object sender, RoutedEventArgs e)
        {
            if(signalPlot.SelectedInterval != null &&
                DataContext is DftAnalysisViewModel dvm &&
                dvm.Signal?.PitchDetailData != null)
            {
                floats = CalcDftStat.GetDfts(signalPlot.SelectedInterval,
                    dvm.Signal.SignalPlotData, dvm.Signal.PitchDetailData,
                    dvm.Signal.Format.SampleRate);

                dvm.SetDFT1(GetDftData(floats, binCombo.SelectedIndex));
            }
        }

        PlotData GetDftData(float[][] floats, int bin)
        {
            List<List<float>> data = new List<List<float>>();

            for (int i = 0; i < 30; i++)
                data.Add(new List<float>());

            if(bin > 0)
            {
                foreach (var arr in floats)
                {
                    for (int j = 0; j < 30; j++)
                        data[j].Add(arr[j]-arr[bin]);
                }
            }
            else
                foreach (var arr in floats)
                {
                    for (int j = 0; j < 30; j++)
                        data[j].Add(arr[j]);
                }
            
            return CalcDftStat.CreateDftDistribution(data);
        }

        private void binCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is DftAnalysisViewModel dvm &&
                dvm.Signal?.PitchDetailData != null &&
                floats != null)
            {
                PlotData plotData = GetDftData(floats, binCombo.SelectedIndex);
                plotData.SetYRange(new FloatRange(plotData.Distributions.Min(d => d.Range.Start), plotData.Distributions.Max(d => d.Range.End)));
                dvm.SetDFT1(plotData);
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftAnalysisViewModel vm &&
                vm.Signal?.SignalPlotData?.Y.Length > 0)
            {
                vm.Player?.Play(vm.Signal.SignalPlotData.Y, vm.Signal.Format.SampleRate);
            }
        }

        private void PlaySelected_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftAnalysisViewModel vm &&
                vm.Signal?.SignalPlotData?.Y.Length > 0 &&
                signalPlot.SelectedInterval != null)
            {
                int fromIndex = Math.Min(signalPlot.SelectedInterval.Value.Start, signalPlot.SelectedInterval.Value.End);
                int length = Math.Abs(signalPlot.SelectedInterval.Value.Length);

                if (fromIndex >= 0 && fromIndex + length <= vm.Signal.SignalPlotData.Y.Length)
                {
                    vm.Player?.Play(vm.Signal.SignalPlotData.Y, vm.Signal.Format.SampleRate, signalPlot.SelectedInterval);
                }
            }
        }

        private void PlaySelectedRepeat_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftAnalysisViewModel vm &&
                vm.Signal?.SignalPlotData?.Y.Length > 0 &&
                signalPlot.SelectedInterval != null)
            {
                int fromIndex = Math.Min(signalPlot.SelectedInterval.Value.Start, signalPlot.SelectedInterval.Value.End);
                int length = Math.Abs(signalPlot.SelectedInterval.Value.Length);

                if (fromIndex >= 0 && fromIndex + length <= vm.Signal.SignalPlotData.Y.Length)
                {
                    int desiredLength = vm.Signal.Format.SampleRate * 5;
                    if (length >= desiredLength)
                        vm.Player?.Play(vm.Signal.SignalPlotData.Y, vm.Signal.Format.SampleRate, signalPlot.SelectedInterval);
                    else
                    {
                        float[] samples = new float[desiredLength];

                        int index = 0;
                        int copyLength;

                        while (index < samples.Length)
                        {
                            copyLength = Math.Min(length, samples.Length - index);
                            Array.Copy(vm.Signal.SignalPlotData.Y, fromIndex, samples, index, copyLength);
                            index += copyLength;
                        }
                        vm.Player?.Play(samples, vm.Signal.Format.SampleRate);
                    }
                }
            }
        }

        private void StopPlaying_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is DftViewModel vm)
            {
                vm.Player?.Stop();
            }
        }
    }
}
