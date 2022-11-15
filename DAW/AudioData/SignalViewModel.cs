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
    public class SignalViewModel : ViewModelBase
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

        public SignalViewModel SetNewLength(int newLength, bool copy)
        {
            SignalViewModel result = new SignalViewModel(File,
                    new PlotData(new float[newLength], -1, 1, 0, 5),
                    new PlotData(new float[newLength], -1, 1, 0, 5),
                    Format);

            if (copy)
            {
                int copyLength = Math.Min(newLength, SignalPlotData.Y.Length);
                Array.Copy(SignalPlotData.Y, result.SignalPlotData.Y, copyLength);
                Array.Copy(PitchPlotData.Y, result.PitchPlotData.Y, copyLength);
            }

            return result;
        }

        public SignalViewModel Trim(int offset, int length)
        {
            int copyLength = Math.Min(length, SignalPlotData.Y.Length);

            SignalViewModel result = new SignalViewModel(File,
                    new PlotData(new float[copyLength], -1, 1, 0, 5),
                    new PlotData(new float[copyLength], -1, 1, 0, 5),
                    Format);

            
            Array.Copy(SignalPlotData.Y, offset, result.SignalPlotData.Y, 0, copyLength);
            Array.Copy(SignalPlotData.Y, offset, result.SignalPlotData.Y, 0, copyLength);

            return result;
        }
    }
}
