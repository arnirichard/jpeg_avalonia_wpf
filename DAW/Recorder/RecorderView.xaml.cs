using DAW.PitchDetector;
using DAW.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            if(sender is FrameworkElement fe && fe.DataContext is PitchDetectorViewModel vm)
            {
                PlayFloats.Play(vm.SignalPlotData.Y, vm.Format.SampleRate);
            }
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && 
                fe.DataContext is PitchDetectorViewModel record &&
                DataContext is RecorderViewModel vm)
            {
                vm.Records.Remove(record);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && DataContext is RecorderViewModel vm &&
                !string.IsNullOrEmpty(vm.Folder) && Directory.Exists(vm.Folder))
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
    }
}
