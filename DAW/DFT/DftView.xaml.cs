using DAW.PitchDetector;
using DAW.Utils;
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

            var pd = DependencyPropertyDescriptor.FromProperty(Plot.CurrentDataPointProperty, typeof(Plot));
            pd.AddValueChanged(signalPlot, OnCurrentValueChanged);

            var pd2 = DependencyPropertyDescriptor.FromProperty(Plot.SelectedIntervalProperty, typeof(Plot));
            pd2.AddValueChanged(signalPlot, OnSelectedXRangeChanged);
        }

        private void OnCurrentValueChanged(object? sender, EventArgs e)
        {
            float? val = pitchPlot.CurrentDataPoint?.Y;
            periodTextBlock.Text = val > 0 && DataContext is SignalViewModel vm && vm.Format != null
                    ? "Period: " + ((int)Math.Round(vm.Format.SampleRate / (float)val))
                    : "";
        }

        private void OnSelectedXRangeChanged(object? sender, EventArgs e)
        {
            if(signalPlot.DataContext is PlotData plotData &&
                DataContext is DftViewModel dvm &&
                dvm.Signal != null &&
                signalPlot.SelectedInterval?.Length > 4)
            {
                XY[] dft = Dft.CalcDft(plotData.Y, signalPlot.SelectedInterval.Value.Start, signalPlot.SelectedInterval.Value.Length);
                dvm.SetDftData(new DftDataViewModel(dft, dvm.Signal.Format.SampleRate, signalPlot.SelectedInterval.Value.Length));
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

        }

        private void PlaySelected_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
