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
        public PlotData? BinAmp { get; private set; }
        public PlotData? BinPhase { get; private set; }
        public PlotData? SynthData { get; private set; }

        public WaveFormat Format { get; private set; }
        public bool IsRecording { get; private set; }
        public int? Samples => SignalPlotData?.Y.Length;

        public double? Duration => Samples.HasValue && Format != null 
            ? Samples.Value / (double)Format.SampleRate
            : null;
        PlotData[]? dftBinAmps, dftBinPhases;
        public PlotData[]? DftBinAmps => dftBinAmps;
        public IPlayer? Player;
        
        public string FormatShortString
        {
            get 
            {
                return string.Format("{0}Hz,{1}Ch,{2}Bit",
                    Format.SampleRate, Format.Channels, Format.BitsPerSample);
            }
        }

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

        public void SetDftBinData(int bin)
        {
            if(PitchDetailData?.Data != null)
            {
                if (dftBinAmps == null)
                {
                    CreateDFTs(PitchDetailData);
                    OnPropertyChanged("SynthData");
                }
                BinAmp = bin >= 0 && bin < dftBinAmps?.Length
                    ? dftBinAmps[bin]
                    : null;
                BinPhase = bin >= 0 && bin < dftBinPhases?.Length
                    ? dftBinPhases[bin]
                    : null;

                var bp1 = dftBinPhases?[1];

                float p1, pn, dpn, p;
                float r;

                if(bp1 != null && BinPhase != null)
                {
                    var bp2 = BinPhase;

                    BinPhase = new PlotData(new float[bp1.Y.Length],
                           new FloatRange(0, (float)(2 * Math.PI)),
                           SignalPlotData.XRange, bp1.X);

                    for(int i = 0; i < bp1.Y.Length; i++)
                    {
                        p1 = bp1.Y[i];
                        pn = bp2.Y[i];
                        dpn = (float)Math.PI * 2 / bin;
                        p = (float)Mod(p1 - pn / bin, 2 * Math.PI);

                        r = (float)Math.Floor(p / dpn);
                        BinPhase.Y[i] = (float)Mod(p - dpn * r, 2 * Math.PI);
                    }

                    BinPhase.SetYRange( new FloatRange(0, (float)(BinPhase.Y.Max() * 1.1)));
                }

                OnPropertyChanged("BinAmp");
                OnPropertyChanged("BinPhase");
            }
        }

        private void CreateDFTs(PlotData pitchDetailData)
        {
            float[] synth = new float[SignalPlotData.Y.Length];
            PlotData[] binAmps = new PlotData[30];
            PlotData[] binPhases = new PlotData[30];

            for (int i = 0; i < binAmps.Length; i++)
            {
                binAmps[i] = new PlotData(new float[pitchDetailData.Y.Length],
                    new FloatRange(0, 1), SignalPlotData.XRange, pitchDetailData.X);
                binPhases[i] = new PlotData(new float[pitchDetailData.Y.Length],
                    new FloatRange(0, (float)(2*Math.PI)), 
                    SignalPlotData.XRange, pitchDetailData.X);
            }
            float[] max = new float[binAmps.Length];

            int index, ind2;
            float amp;
            double ph;

            if (pitchDetailData.Data != null)
            {
                double[] phase = new double[binAmps.Length];
                int lastSample = -1;
                double corr; // ph, corr, per;
                double delta;
                //PeriodFit? lastPf = null;
                float[] amps = new float[binAmps.Length];
                Smoother[] smoothers = new Smoother[binAmps.Length];
                for(int j = 0;j < smoothers.Length; j++)
                    smoothers[j] = new Smoother(5, 0);
                Smoother sm;


                float ampDelta;

                for (int i = 0; i < pitchDetailData.Data.Length; i++)
                {

                    if (pitchDetailData.Data[i] is PeriodFit pf)
                    {
                        index = Math.Max(0, pf.Sample - pf.Period);
                        XY[] dft = Dft.CalcDft(SignalPlotData.Y, index, pf.Period);
                        if (dft.Length > 0)
                        {
                            binAmps[0].Y[i] = Math.Abs(dft[0].X) / pf.Period;
                            ind2 = Math.Min(binAmps.Length, dft.Length);
                            delta = 2 * Math.PI / pf.Period;
                            //bool resusePhase = lastPf != null &&
                            //    lastPf.Sample + lastPf.Period * 1.5 > pf.Sample;

                            for (int j = 1; j < ind2; j++)
                            {
                                sm = smoothers[j];
                                amps[j] = binAmps[j].Y[i] = sm.Add(2 * dft[j].Abs / pf.Period);

                                corr = (pf.Sample - lastSample) * delta * j;
                                binPhases[j].Y[i] = (float)Mod(dft[j].Phase, 2 * Math.PI);

                                
                                ph = phase[j] = (float)Mod(dft[j].Phase, 2 * Math.PI);

                                if (sm.Estimate > max[j])
                                    max[j] = sm.Estimate;

                                if (j <= 15 && lastSample > -1)
                                {
                                    ph -= corr;
                                    amp = sm.LastEstimate;
                                    ampDelta = (sm.Estimate - sm.LastEstimate) / (pf.Sample - lastSample);

                                    for (int k = lastSample + 1; k <= pf.Sample; k++)
                                    {
                                        //if(j < 18)
                                            synth[k] += (float)(sm.LastEstimate * Math.Cos(ph));
                                        ph += delta * j;
                                        amp += ampDelta;
                                    }
                                }
                            }

                            lastSample = pf.Sample;
                            //lastPf = pf;
                        }
                    }
                }
            }

            var firstPlot = binAmps[0];

            for(int i = 0; i < firstPlot.Y.Length; i++)
            {
                firstPlot.Y[i] = 0;
                for( int j = 1; j < binAmps.Length; j++)
                {
                    firstPlot.Y[i] += (float)Math.Pow(binAmps[j].Y[i], 2);
                }

                firstPlot.Y[i] = (float)Math.Sqrt(firstPlot.Y[i]);
            }
            max[0] = firstPlot.Y.Max();

            for (int i = 0; i < binAmps.Length; i++)
                binAmps[i].SetYRange(new FloatRange(0, max[i]*1.1f));

            dftBinAmps = binAmps;
            dftBinPhases = binPhases;

            SynthData = new PlotData(synth, SignalPlotData.YRange, SignalPlotData.XRange);
        }

        double Mod(double v, double m)
        {
            if (v < 0)
            {
                return m - (-v % m);
            }
            else
                return v % m;
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
            int newLength = Math.Min(length, SignalPlotData.Y.Length- offset);

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
