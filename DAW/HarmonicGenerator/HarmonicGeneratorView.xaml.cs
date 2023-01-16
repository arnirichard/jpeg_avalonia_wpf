using DAW.Transcription;
using DAW.Utils;
using Microsoft.VisualBasic.Logging;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.WebSockets;
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

namespace DAW.HarmonicGenerator
{

    class HarmonicWeight : ViewModelBase
    {
        public int Number { get; }
        public double Log { get; set; }

        public HarmonicWeight(int number, double log)
        {
            Number = number;
            Log = log;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}",
                Number, Log);
        }

        public void SetWeight(int w)
        {
            Log = w;
            OnPropertyChanged("Weight");
        }
    }

    /// <summary>
    /// Interaction logic for HarmonicGeneratorView.xaml
    /// </summary>
    public partial class HarmonicGeneratorView : UserControl
    {
        ObservableCollection<HarmonicWeight> harmonicWeights= new ObservableCollection<HarmonicWeight>();

        public HarmonicGeneratorView()
        {
            InitializeComponent();
            for (int i = 0; i < 40; i++)
                harmonicWeights.Add(new HarmonicWeight(i + 1, i == 0 ? -0.5 : -5));
            harmoicWeightsItems.ItemsSource = harmonicWeights;
            phonemes.ItemsSource = PhonemeModel.Models;
            ampPlot.VerticalLines.Add(new LinesDefinition(0, 1f, false, Plot.Beige, 10));
            ampPlot.HorizontalLines.Add(new LinesDefinition(0, 1f, false, Plot.Beige, 10));
        }

        private void Weight_TextChanged(object sender, TextChangedEventArgs e)
        {
            int newWeight;
            if (sender is TextBox tb &&
                tb.DataContext is HarmonicWeight hw &&
                int.TryParse(tb.Text, out newWeight))
            {
                hw.SetWeight(newWeight);
                var weights = new double[harmonicWeights.Count];
                foreach (var w in harmonicWeights)
                {
                    weights[w.Number - 1] = w.Log;
                }
                if (DataContext is HarmoicGeneratorViewModel vm)
                {
                    vm.SetHarmonic(new Harmonic(vm.Harmonic.Weights, vm.Harmonic.DefaultPitch, vm.Harmonic.Amplitudes, vm.Harmonic.Pitch));
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[-^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if(DataContext is HarmoicGeneratorViewModel vm)
            {
                var signal = vm.Period.Y;

                //if (signal.Length < vm.SampleRate)
                //{
                //    signal = signal.Extrapolate(vm.SampleRate*2/signal.Length);
                //}

                vm.Player?.Play(signal, vm.SampleRate);
            }
        }

        private void pasteAmplitudes(object sender, RoutedEventArgs e)
        {
            if (DataContext is HarmoicGeneratorViewModel vm)
            {
                string text = Clipboard.GetText();

                var splits = text.Split('\n');
                float log;
                float[] logs = new float[40];
                for (int i = 0; i < logs.Length; i++)
                    logs[i] = -5;
                for (int i = 1; i < Math.Min(logs.Length, splits.Length); i++)
                {
                    if (!float.TryParse(splits[i], out log))
                        return;
                    logs[i-1] = log;
                }
                harmonicWeights.Clear();
                for (int i = 0; i < logs.Length; i++)
                {
                    harmonicWeights.Add(new HarmonicWeight(i + 1, logs[i]));
                }
                vm.SetHarmonic(new Harmonic(logs, vm.Harmonic.DefaultPitch, vm.Harmonic.Amplitudes, vm.Harmonic.Pitch));
            }
        }

        private void createEnvelope(object sender, RoutedEventArgs e)
        {
            PlotData[]? amplitudes = MyClipboard.Object as PlotData[];
            PlotData? pitch = MyClipboard.Object as PlotData;
            if (DataContext is HarmoicGeneratorViewModel vm)
            {
                vm.SetHarmonic(new Harmonic(vm.Harmonic.Weights, vm.Harmonic.DefaultPitch, 
                    amplitudes ?? vm.Harmonic.Amplitudes, pitch ?? vm.Harmonic.Pitch));
            }
        }

        private void phonemes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(phonemes.SelectedItem is PhonemeModel ph &&
                DataContext is HarmoicGeneratorViewModel vm)
            {
                vm.SetHarmonic(new Harmonic(ph.Average, vm.Harmonic.DefaultPitch,
                    vm.Harmonic.Amplitudes, vm.Harmonic.Pitch));
            }
        }
    }
}
