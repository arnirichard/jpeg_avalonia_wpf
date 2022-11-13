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
                PlayFloats.Play(vm.SignalPlotData.Y, vm.Format.SampleRate);
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
                    PlayFloats.Play(vm.SignalPlotData.Y, vm.Format.SampleRate, fromIndex, length);
                }
            }
        }

        
    }
}
