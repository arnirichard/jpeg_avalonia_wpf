using DAW.Equalization;
using DAW.PitchDetector;
using DAW.Utils;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Equalization
{
    class Gain
    {
        public int Frequency { get; }
        public double Decibel { get; }
        public Gain(int frequency, double decibel)
        {
            Frequency = frequency;
            Decibel = decibel;
        }

        public override string ToString()
        {
            return string.Format("{0}Hz:{1}dB",
                Frequency,
                Decibel);
        }
    }

    class EqualizationViewModule : ViewModelBase
    {
        static int[] Frequencies = new int[] { 10, 20, 30, 40, 60, 80, 100, 125, 150, 200, 300, 400, 500, 630, 800, 1000, 1200, 1500, 2000, 2500, 3000, 4000, 5000, 6000, 8000, 10000, 12500, 15000, 17500, 20000 };
        static int[] Frequencies2 = new int[] { 200, 400, 800, 1200, 2400, 4800, 9600 };

        public SignalViewModel? Signal { get; private set; }
        public PlotData? SignalDFT { get; private set; }
        public PlotData? FilteredSignal { get; private set; }
        public PlotData? FilteredSignalDFT { get; private set; }
        public IPlayer? Player { get; private set; }

        public ObservableCollection<Gain> GainMap { get; private set; } = new();

        public EqualizationViewModule()
        {
            foreach (var f in Frequencies)
                GainMap.Add(new Gain(f, 0));
        }

        internal void SetPlayer(IPlayer player)
        {
            Player = player;
        }

        internal void SetSignal(SignalViewModel signal)
        {
            Signal = signal;
            OnPropertyChanged("Signal");
        }

        internal void SetFilteredSignal(PlotData filtered)
        {
            FilteredSignal = filtered;
            OnPropertyChanged("FilteredSignal");
        }

        internal void SetSignalDft(PlotData plotData)
        {
            SignalDFT = plotData;
            OnPropertyChanged("SignalDFT");
        }

        internal void SetFilteredDft(PlotData plotData)
        {
            FilteredSignalDFT = plotData;
            OnPropertyChanged("FilteredSignalDFT");
        }
    }
}
