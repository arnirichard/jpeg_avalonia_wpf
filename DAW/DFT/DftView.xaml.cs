using DAW.PitchDetector;
using DAW.Tasks;
using DAW.Utils;
using PitchDetector;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
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

namespace DAW.DFT
{
    /// <summary>
    /// Interaction logic for DftView.xaml
    /// </summary>
    public partial class DftView : UserControl
    {
        JobHandler setBinHandler = new JobHandler(1);

        public DftView()
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

            pitchPlot.VerticalLines.AddRange(verticalLines);
            signalPlot.VerticalLines.AddRange(verticalLines);
            signalPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 40),
            });
            pitchPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 50, false, Plot.Beige, 40),
                new LinesDefinition(0, 10, false, Plot.Beige, 40),
            });

            dftPlot.VerticalLines.Add(new LinesDefinition(0, 1, false, Plot.Beige));
            dftPlot.HorizontalLines.Add(new LinesDefinition(0, 10, false, Plot.Beige));

            dftBinPlot.VerticalLines.AddRange(verticalLines);
            dftBinPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 40),
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 40),
                new LinesDefinition(0, 0.01f, false, Plot.Beige, 30),
                new LinesDefinition(0, 0.001f, false, Plot.Beige, 30),
            });

            var pd = DependencyPropertyDescriptor.FromProperty(Plot.CurrentDataPointProperty, typeof(Plot));
            pd.AddValueChanged(pitchPlot, OnCurrentValueChanged);

            var pd2 = DependencyPropertyDescriptor.FromProperty(Plot.SelectedIntervalProperty, typeof(Plot));
            pd2.AddValueChanged(signalPlot, OnSelectedXRangeChanged);

            var pd3 = DependencyPropertyDescriptor.FromProperty(Plot.IntervalProperty, typeof(Plot));
            pd3.AddValueChanged(dftPlot, OnDftXRangeChanged);

            //float[] signal = new float[8];

            //for (int i = 0; i < signal.Length; i++)
            //    signal[i] = (float)Math.Sin(2 * Math.PI * i / signal.Length);

            //XY[] dft = Dft.CalcDft(signal, 0, signal.Length);

            //double pow = 0;
            //for (int i = 0; i < dft.Length; i++)
            //{
            //    pow += dft[i].Power;
            //}
            for (int i = 0; i < 20; i++)
                binCombo.Items.Add(new ComboBoxItem()
                {
                    Content = "Bin "+i
                });
        }

        private void self_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            
        }

        private void binCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is DftViewModel dvm &&
                dvm.Signal != null)
            {
                SetDftBins(dvm.Signal);
            }
        }

        void SetDftBins(SignalViewModel signalViewModel)
        {
            int bin = binCombo.SelectedIndex;

            setBinHandler.AddJob(delegate
            {
                signalViewModel.SetDftBinData(bin);
            });
        }


        private void OnDftXRangeChanged(object? sender, EventArgs e)
        {
            if(DataContext is DftViewModel vm)
            {
                powRat.Text = vm.DftData?.TotalPower > 0
                    ? (vm.DftData.GetPower(dftPlot.Interval)/ vm.DftData.TotalPower*100).ToString("N1")+"%"
                    : null;
            }
        }

        private void OnCurrentValueChanged(object? sender, EventArgs e)
        {
            float? val = pitchPlot.CurrentDataPoint?.Y;
            int? period = val > 0 && DataContext is DftViewModel vm && vm.Signal?.Format != null
                    ? ((int)Math.Round(vm.Signal.Format.SampleRate / (float)val))
                    : null;
            periodTextBlock.Text = period != null
                    ? "Period: " + period
                    : "";

            if(signalPlot.SelectedInterval == null &&
                signalPlot.CurrentDataPoint != null &&
                DataContext is DftViewModel dvm &&
                dvm.Signal != null &&
                signalPlot?.DataContext is PlotData plotData &&
                period != null)
            {
                int index = Math.Max(0, signalPlot.CurrentDataPoint.Index - period.Value);
                XY[] dft = Dft.CalcDft(plotData.Y, index, period.Value);
                var dftData = new DftDataViewModel(dft, dvm.Signal.Format.SampleRate, period.Value);
                dvm.SetDftData(dftData);
                spl.Text = Decibel.AvgPowerToSQL(dftData.AvgSamplePower).ToString("N1") + " SPL";
            }
        }

        private void OnSelectedXRangeChanged(object? sender, EventArgs e)
        {
            if(signalPlot.DataContext is PlotData plotData &&
                DataContext is DftViewModel dvm &&
                dvm.Signal != null &&
                signalPlot.SelectedInterval?.Length > 4)
            {
                XY[] dft = Dft.CalcDft(plotData.Y, signalPlot.SelectedInterval.Value.Start, signalPlot.SelectedInterval.Value.Length);

                //int start = signalPlot.SelectedInterval.Value.Start;
                //int length = signalPlot.SelectedInterval.Value.End;
                //double pow = 0;
                //double avg = 0;
                //for(int i = start; i < length; i++)
                //{
                //    pow += Math.Pow(plotData.Y[i], 2);
                //    avg += plotData.Y[i];
                //}
                
                //int l = signalPlot.SelectedInterval.Value.Length;
                //pow /= l;
                //double p = dft.Sum(d => d.Power) / l / l;

                var dftData = new DftDataViewModel(dft, dvm.Signal.Format.SampleRate, signalPlot.SelectedInterval.Value.Length);
                dvm.SetDftData(dftData);
                spl.Text = Decibel.AvgPowerToSQL(dftData.AvgSamplePower).ToString("N1")+" SPL";
            }
            else if(DataContext is DftViewModel dvm2)
            {
                dvm2.SetDftData(null);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftViewModel vm &&
                vm.Signal?.SignalPlotData?.Y.Length > 0)
            {
                vm.Player?.Play(vm.Signal.SignalPlotData.Y, vm.Signal.Format.SampleRate);
            }
        }

        private void PlaySelected_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftViewModel vm &&
                vm.Signal?.SignalPlotData?.Y.Length > 0 &&
                signalPlot.SelectedInterval != null)
            {
                int fromIndex = Math.Min(signalPlot.SelectedInterval.Value.Start, signalPlot.SelectedInterval.Value.End);
                int length = Math.Abs(signalPlot.SelectedInterval.Value.Length);

                if (fromIndex >= 0 && fromIndex + length <= vm.Signal.SignalPlotData.Y.Length)
                {
                    vm.Player?.Play(vm.Signal.SignalPlotData.Y, vm.Signal.Format.SampleRate);
                }
            }
        }

        private void signalPlot_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is DftViewModel dvm &&
                dvm.Signal != null)
            {
                SetDftBins(dvm.Signal);
            }
        }
    }
}
