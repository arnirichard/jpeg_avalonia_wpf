using DAW.Equalization;
using DAW.Utils;
using NAudio.Utils;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DAW.Loudness
{
    /// <summary>
    /// Interaction logic for LoudnessView.xaml
    /// </summary>
    public partial class LoudnessView : UserControl
    {
        public LoudnessView()
        {
            InitializeComponent();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button slider &&
                slider.DataContext is Gain g &&
                DataContext is LoudnessViewModule vm)
            {
                var tone = Tone.GenerateTone(g.Frequency, vm.SampleRate, vm.SampleRate * 1,
                   (float)Decibels.DecibelsToLinear(vm.DefaultAmplitude * g.Decibel));
                tone.RampUp(0, 1000);
                tone.RampUp(tone.Length-1000, 1000);
                vm.Player?.Play(tone, vm.SampleRate);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(DataContext is LoudnessViewModule vm)
            {
                vm.SaveGainMap();
            }
        }
    }
}
