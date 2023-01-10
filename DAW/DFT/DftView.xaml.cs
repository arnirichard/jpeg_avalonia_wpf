using DAW.PitchDetector;
using DAW.Recorder;
using DAW.Tasks;
using DAW.Transcription;
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
        IntRange? selectedInterval;

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
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 20),
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 20),
            });

            synthPlot.VerticalLines.AddRange(verticalLines);
            synthPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 20),
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 20),
            });
            pitchPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 50, false, Plot.Beige, 40),
                new LinesDefinition(0, 10, false, Plot.Beige, 40),
            });

            dftPlot.VerticalLines.Add(new LinesDefinition(0, 1, false, Plot.Beige));
            dftPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 10, false, Plot.Beige),
                new LinesDefinition(0, 5, false, Plot.Beige, 10),
                new LinesDefinition(0, 1, false, Plot.Beige, 7)
            });
                

            dftBinPlot.VerticalLines.AddRange(verticalLines);
            dftBinPlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 0.5f, false, Plot.Beige, 20),
                new LinesDefinition(0, 0.1f, false, Plot.Beige, 10),
                new LinesDefinition(0, 0.01f, false, Plot.Beige, 10),
                new LinesDefinition(0, 0.001f, false, Plot.Beige, 10),
            });

            dftBinPhase.VerticalLines.AddRange(verticalLines);
            dftBinPhase.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, (float)Math.PI/4f, false, Plot.Beige, 15),
                new LinesDefinition(0, (float)Math.PI/2f, false, Plot.Beige, 20),
            });

            var pd = DependencyPropertyDescriptor.FromProperty(Plot.CurrentDataPointProperty, typeof(Plot));
            pd.AddValueChanged(pitchPlot, OnCurrentValueChanged);

            var pd2 = DependencyPropertyDescriptor.FromProperty(Plot.SelectedIntervalProperty, typeof(Plot));
            pd2.AddValueChanged(signalPlot, OnSelectedXRangeChanged);

            var pd3 = DependencyPropertyDescriptor.FromProperty(Plot.IntervalProperty, typeof(Plot));
            pd3.AddValueChanged(dftPlot, OnDftXRangeChanged);

            for (int i = 0; i < 20; i++)
                binCombo.Items.Add(new ComboBoxItem()
                {
                    Content = "Harmonic "+i
                });
            binCombo.SelectedIndex = 1;
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

        private void calcDftState_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftViewModel dvm &&
                dvm.Signal?.PitchDetailData != null &&
                signalPlot.SelectedInterval != null)
            {
                string str = CalcDftStat.CalcStats(signalPlot.SelectedInterval, 
                    dvm.Signal.SignalPlotData, dvm.Signal.PitchDetailData,
                    dvm.Signal.Format.SampleRate);
                
                Clipboard.SetText(str);
            }
        }


        private void OnCurrentValueChanged(object? sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                return;
            }

            float? val = pitchPlot.CurrentDataPoint?.Y;
            int? period = val > 0 && DataContext is DftViewModel vm && vm.Signal?.Format != null
                    ? ((int)Math.Round(vm.Signal.Format.SampleRate / (float)val))
                    : null;
            periodTextBlock.Text = period != null
                    ? "Period: " + period
                    : "";

            if (signalPlot.SelectedInterval == null &&
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
                string phonemeStr = "";
                double[] norm = dftData.GetNormalisedAmps(30);
                double prop, currentProp = double.NegativeInfinity;

                foreach (var m in PhonemeModel.Models)
                {
                    prop = m.CalcProp(norm);
                    if (prop > currentProp)
                    {
                        currentProp = prop;
                        phonemeStr = m.SourceName + " " + prop;
                    }
                }
                phoneme.Text = phonemeStr;
            }
        }

        private void OnSelectedXRangeChanged(object? sender, EventArgs e)
        {
            //if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            //{
            //    return;
            //}

            if (signalPlot.DataContext is PlotData plotData &&
                DataContext is DftViewModel dvm &&
                dvm.Signal != null &&
                signalPlot.SelectedInterval?.Length > 4)
            {
                var interval = selectedInterval = signalPlot.SelectedInterval;

                setBinHandler.AddJob(delegate
                {
                    if (!interval.Equals(selectedInterval))
                        return;
                    XY[] dft = Dft.CalcDft(plotData.Y, interval.Value.Start, interval.Value.Length);
                    if (!interval.Equals(selectedInterval))
                        return;
                    var dftData = new DftDataViewModel(dft, dvm.Signal.Format.SampleRate, interval.Value.Length);
                    dvm.SetDftData(dftData);

                    Application.Current.Dispatcher.Invoke(new Action(() => 
                    {
                        if (!interval.Equals(selectedInterval))
                            return;
                        spl.Text = Decibel.AvgPowerToSQL(dftData.AvgSamplePower).ToString("N1") + " SPL";
                    }));
                    
                });
            }
            else if (DataContext is DftViewModel dvm2)
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

        private void Play_Synth(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftViewModel vm &&
                vm.Signal?.SynthData?.Y.Length > 0)
            {
                vm.Player?.Play(vm.Signal.SynthData.Y, vm.Signal.Format.SampleRate);
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
                    vm.Player?.Play(vm.Signal.SignalPlotData.Y, vm.Signal.Format.SampleRate, signalPlot.SelectedInterval);
                }
            }
        }

        private void PlaySelectedRepeat_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftViewModel vm &&
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
                            copyLength = Math.Min(length, samples.Length-index);
                            Array.Copy(vm.Signal.SignalPlotData.Y, fromIndex, samples, index, copyLength);
                            index += copyLength;
                        }
                        vm.Player?.Play(samples, vm.Signal.Format.SampleRate);
                    }
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

        private void copyAmplitudes(object sender, RoutedEventArgs e)
        {
            if (DataContext is DftViewModel dvm &&
                dvm.DftData?.Data != null)
            {
                var y = dvm.DftData.Data?.Y;

                if(y != null)
                {
                    var str = string.Join("\n", y.ToList().GetRange(0, Math.Min(30, y.Length)).Select(y => y.ToString()));
                    Clipboard.SetText(str);
                }
            }
        }

        private void PlayDftRepeat_Click(object sender, RoutedEventArgs e)
        {
            float? val = pitchPlot.CurrentDataPoint?.Y;
            int? period = val > 0 && DataContext is DftViewModel vm && vm.Signal?.Format != null
                    ? ((int)Math.Round(vm.Signal.Format.SampleRate / (float)val))
                    : null;

            if (signalPlot.SelectedInterval == null &&
                    signalPlot.CurrentDataPoint != null &&
                    DataContext is DftViewModel dvm &&
                    dvm.Signal != null &&
                    signalPlot?.DataContext is PlotData plotData &&
                    period != null)
            {
                int index = Math.Max(0, signalPlot.CurrentDataPoint.Index - period.Value);
                float[] periodSignal = new float[period.Value];
                Array.Copy(plotData.Y, index, periodSignal, 0, period.Value);

                string str = string.Join('\n', periodSignal);
                Clipboard.SetText(str);

                periodSignal = periodSignal.Extrapolate(dvm.Signal.Format.SampleRate / period.Value*2);

                dvm.Player?.Play(periodSignal, dvm.Signal.Format.SampleRate);
            }
        }

        private void copyEnvelope(object sender, RoutedEventArgs e)
        {
            if(dftBinPlot.DataContext is PlotData pd)
            {
                MyClipboard.Object = pd;
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
