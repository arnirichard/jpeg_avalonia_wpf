using DAW.Utils;
using NAudio.Wave;
using PitchDetector;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.PitchDetector
{
    internal class SignalViewModel : ViewModelBase
    {
        public FileInfo File { get; private set; }
        public PlotData SignalPlotData { get; private set; }
        public PlotData PitchPlotData { get; private set; }
        public WaveFormat Format { get; private set; }
        public bool IsRecording { get; private set; }

        public int? Samples => SignalPlotData?.Y.Length;

        public double? Duration => Samples.HasValue && Format != null 
            ? Samples.Value / (double)Format.SampleRate
            : null;

        public SignalViewModel(FileInfo file, PlotData signalPlotData, 
            PlotData pitchPlotData, WaveFormat waveFormat)
        {
            File = file;
            SignalPlotData = signalPlotData;
            PitchPlotData = pitchPlotData;
            Format = waveFormat;
        }

        public void StopRecording()
        {
            IsRecording = false;
            OnPropertyChanged("IsRecording");
        }

        public void SignalChanged(PlotData? newPlotData = null)
        {
            SignalPlotData = newPlotData ?? SignalPlotData.Clone();
            OnPropertyChanged("SignalPlotData");
        }
    }
}
