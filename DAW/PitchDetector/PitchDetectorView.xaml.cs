using DAW.Utils;
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
        public IPlayer? Player { get; private set; }

        public PitchDetectorView(IPlayer? player)
        {
            InitializeComponent();

            Player = player;
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

            pitchDataPlot.VerticalLines.AddRange(verticalLines);
            pitchDataPlot.HorizontalLines.Add(new LinesDefinition(0, 10, false, Plot.Beige));

            var pd = DependencyPropertyDescriptor.FromProperty(Plot.CurrentDataPointProperty, typeof(Plot));
            pd.AddValueChanged(pitchPlot, OnCurrentValueChanged);
        }

        internal void SetPlayer(IPlayer player)
        {
            Player = player;
        }

        private void OnCurrentValueChanged(object? sender, EventArgs e)
        {
            float? val = pitchPlot.CurrentDataPoint?.Y;
            periodTextBlock.Text = val > 0 && DataContext is SignalViewModel vm && vm.Format != null
                    ? "Period: " + ((int)Math.Round(vm.Format.SampleRate / (float)val))
                    : "";
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SignalViewModel vm && 
                vm.SignalPlotData?.Y.Length > 0)
            {
                Player?.Play(vm.SignalPlotData.Y, vm.Format.SampleRate);
            }
        }

        private void PlaySelected_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SignalViewModel vm && 
                vm.SignalPlotData?.Y.Length > 0 &&
                signalPlot.SelectedInterval != null)
            {
                int fromIndex = Math.Min(signalPlot.SelectedInterval.Value.Start, signalPlot.SelectedInterval.Value.End);
                int length = Math.Abs(signalPlot.SelectedInterval.Value.Length);

                if (fromIndex >= 0 && fromIndex+length <= vm.SignalPlotData.Y.Length)
                {
                    Player?.Play(vm.SignalPlotData.Y, vm.Format.SampleRate, signalPlot.SelectedInterval);
                }
            }
        }
    }
}
