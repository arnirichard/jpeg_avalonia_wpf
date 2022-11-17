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
        public PlotData? PitchPlotData { get; private set; }
        public PlotData? PitchDetailData { get; private set; }
        public WaveFormat Format { get; private set; }
        public bool IsRecording { get; private set; }

        public int? Samples => SignalPlotData?.Y.Length;

        public double? Duration => Samples.HasValue && Format != null 
            ? Samples.Value / (double)Format.SampleRate
            : null;

        public SignalViewModel(FileInfo file, WaveFormat waveFormat, 
            PlotData signalPlotData, 
            PlotData? pitchPlotData = null, PlotData? pitchDataPlotData = null)
        {
            File = file;
            SignalPlotData = signalPlotData;
            PitchPlotData = pitchPlotData;
            Format = waveFormat;
            PitchDetailData = pitchDataPlotData;
        }

        public void SetRecording(bool recording)
        {
            IsRecording = recording;
            OnPropertyChanged("IsRecording");
        }

        public void SignalChanged(PlotData? plotData = null)
        {
            SignalPlotData = plotData != null && plotData != SignalPlotData
                ? plotData
                : SignalPlotData.Clone();
            OnPropertyChanged("SignalPlotData");
        }

        public SignalViewModel SetNewLength(int newLength, bool copy)
        {
            PlotData? newPitchData = PitchPlotData != null
                ? new PlotData(new float[newLength], new FloatRange(80, 300), new FloatRange(0, 5))
                : null;

            SignalViewModel result = new SignalViewModel(File, Format,
                    new PlotData(new float[newLength], new FloatRange(- 1, 1), new FloatRange(0, 5)),
                    newPitchData);

            if (copy)
            {
                int copyLength = Math.Min(newLength, SignalPlotData.Y.Length);
                Array.Copy(SignalPlotData.Y, result.SignalPlotData.Y, copyLength);
                if(PitchPlotData != null)
                    Array.Copy(PitchPlotData.Y, result.PitchPlotData.Y, copyLength);
            }

            return result;
        }

        public SignalViewModel Trim(int offset, int length)
        {
            int newLength = Math.Min(length, SignalPlotData.Y.Length);

            PlotData? newPitchData = PitchPlotData != null
                ? new PlotData(new float[newLength], new FloatRange(80, 300), new FloatRange(0, 5))
                : null;

            SignalViewModel result = new SignalViewModel(File, Format,
                    new PlotData(new float[newLength], new FloatRange(-1, 1), new FloatRange(0, 5)),
                    newPitchData);
            
            Array.Copy(SignalPlotData.Y, offset, result.SignalPlotData.Y, 0, newLength);
            Array.Copy(SignalPlotData.Y, offset, result.SignalPlotData.Y, 0, newLength);

            return result;
        }
    }
}
