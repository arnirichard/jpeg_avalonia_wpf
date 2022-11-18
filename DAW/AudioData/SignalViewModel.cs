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
        PitchTracker? pitchTracker;

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

        public SignalViewModel(FileInfo file, WaveFormat waveFormat, PlotData signalPlotData)
        {
            File = file;
            Format = waveFormat;
            SignalPlotData = signalPlotData;
        }

        public void SetPitchData()
        {
            pitchTracker = new PitchTracker(Format.SampleRate / 300, Format.SampleRate / 80, Format.SampleRate);

            float[] samples = SignalPlotData.Y;

            for (int i = 0; i < samples.Length; i++)
                pitchTracker.AddSample(samples[i]);

            float[] y = pitchTracker.Data.PeriodFits.Select(p => (float)p.Period).ToArray();
            float[] x = pitchTracker.Data.PeriodFits.Select(p => (float)p.Sample / pitchTracker.SampleRate).ToArray();

            PitchPlotData = CreatePitchPlotData.GetPitchPlotData(pitchTracker);
            PitchDetailData = new PlotData(y,
                new FloatRange(y.Min() - 5, y.Max() + 5),
                new FloatRange(0, pitchTracker.TotalSamples / (float)pitchTracker.SampleRate),
                x, pitchTracker.Data.PeriodFits.ToArray());
        }

        public void SetWaveFormat(WaveFormat waveFormat)
        {
            Format = waveFormat;
            OnPropertyChanged("Format");
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
            SignalViewModel result = new SignalViewModel(File, Format,
                    new PlotData(new float[newLength], new FloatRange(- 1, 1),
                    new FloatRange(0, newLength / (float)Format.SampleRate)));

            if (copy)
            {
                int copyLength = Math.Min(newLength, SignalPlotData.Y.Length);
                Array.Copy(SignalPlotData.Y, result.SignalPlotData.Y, copyLength);
                if (pitchTracker != null)
                    result.SetPitchData();
            }

            return result;
        }

        public SignalViewModel Trim(int offset, int length)
        {
            int newLength = Math.Min(length, SignalPlotData.Y.Length);

            SignalViewModel result = new SignalViewModel(File, Format,
                    new PlotData(new float[newLength], 
                        new FloatRange(-1, 1), 
                        new FloatRange(0, length/(float)Format.SampleRate)));
            
            Array.Copy(SignalPlotData.Y, offset, result.SignalPlotData.Y, 0, newLength);

            if(pitchTracker != null)
                result.SetPitchData();

            return result;
        }

        public SignalViewModel RemoveGaps(List<IntRange> gaps, int rampLength)
        {
            float[] samples = new float[SignalPlotData.Y.Length];
            int iIndex = 0;
            int oIndex = 0;
            IntRange? gap = gaps.FirstOrDefault();
            int nextGapsIndex = 1;
            float ratio;

            while (iIndex < SignalPlotData.Y.Length)
            {
                if(gap == null)
                {
                    Array.Copy(SignalPlotData.Y, iIndex, samples, oIndex, SignalPlotData.Y.Length - iIndex);
                    oIndex += SignalPlotData.Y.Length - iIndex;
                    iIndex = SignalPlotData.Y.Length;
                }
                else
                {
                    
                    Array.Copy(SignalPlotData.Y, iIndex, samples, oIndex, gap.Value.Start - iIndex);
                    oIndex += gap.Value.Start - iIndex;
                    iIndex += gap.Value.Start - iIndex;

                    
                    int rlen = Math.Min(rampLength, gap.Value.Length / 2);
                    float delta = 1 / (float)rlen;

                    // decay ramp
                    if (gap.Value.Start > 0)
                    {
                        ratio = 1 - delta;
                        while (ratio > 0)
                        {
                            samples[oIndex++] = SignalPlotData.Y[iIndex++] * ratio;
                            ratio -= delta;
                        }
                    }
                    else
                        iIndex += rlen;

                    int realGap = gap.Value.Length - 2 * rlen;
                    if (gap.Value.Start > 0 && gap.Value.End < samples.Length)
                    {
                        // gaps
                        oIndex += realGap;
                    }
                    iIndex += realGap;

                    // ramp
                    if (gap.Value.End < samples.Length)
                    {
                        ratio = delta;
                        while (ratio < 1)
                        {
                            samples[oIndex++] = SignalPlotData.Y[iIndex++] * ratio;
                            ratio += delta;
                        }
                    }
                    else
                    {
                        iIndex += rlen;
                    }

                    gap = nextGapsIndex < gaps.Count ? gaps[nextGapsIndex] : null;
                    nextGapsIndex++;
                }
            }

            float[] result = new float[oIndex];
            Array.Copy(samples, result, result.Length);

            return new SignalViewModel(File, Format,
                new PlotData(result, 
                    SignalPlotData.YRange, 
                    new FloatRange(0, result.Length / (float)Format.SampleRate)));
        }
    }
}
