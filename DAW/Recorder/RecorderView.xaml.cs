using DAW.PitchDetector;
using DAW.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace DAW.Recorder
{
    /// <summary>
    /// Interaction logic for RecorderView.xaml
    /// </summary>
    public partial class RecorderView : UserControl
    {      
        public RecorderView()
        {
            InitializeComponent();
        }

        private void deviceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if(sender is FrameworkElement fe && fe.DataContext is SignalViewModel vm)
            {
                PlayFloats.Play(vm.SignalPlotData.Y, vm.Format.SampleRate);
            }
        }

        private void PlaySelected_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && 
                fe.DataContext is SignalViewModel vm)
            {
                Plot? plot = fe.FindName("signalPlot") as Plot;

                if(plot != null && plot.SelectedInterval!= null)
                {
                    PlayFloats.Play(vm.SignalPlotData.Y, vm.Format.SampleRate, plot.SelectedInterval);
                }
            }
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is SignalViewModel signalVM &&
                DataContext is RecorderViewModel vm)
            {
                if (vm.IsRecording)
                    vm.StopRecording();
                else
                {
                    vm.StartRecording(signalVM);
                }
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && 
                fe.DataContext is SignalViewModel record &&
                DataContext is RecorderViewModel vm)
            {
                vm.Records.Remove(record);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && DataContext is RecorderViewModel vm &&
                !string.IsNullOrEmpty(vm.Folder) && 
                Directory.Exists(vm.Folder))
            {
                var splits = recordingList.Text.Split('\n', ' ', '\r');
                List<string> toAdd = new();

                foreach (var split in splits)
                {
                    if (string.IsNullOrWhiteSpace(split))
                        continue;

                    if(!vm.Records.Any(r => r.File.Name.ToLower() == split+".wav"))
                    {
                        vm.AddFile(Path.Combine(vm.Folder, split + ".wav"));
                    }
                }
            }
        }

        private void Normalize_Click(object sender, RoutedEventArgs e)
        {
            float factor;
            if(float.TryParse(normalization.Text, out factor) &&
                sender is FrameworkElement fe &&
                fe.DataContext is SignalViewModel record)
            {
                float max = 0;
                for(int i = 0; i < record.SignalPlotData.Y.Length; i++)
                {
                    if (Math.Abs(record.SignalPlotData.Y[i]) > max)
                        max = Math.Abs(record.SignalPlotData.Y[i]);
                }
                if (max > 0)
                {
                    factor = factor / max;
                    for (int i = 0; i < record.SignalPlotData.Y.Length; i++)
                    {
                        record.SignalPlotData.Y[i] *= factor;
                    }
                    record.SignalChanged();
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Trim_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
