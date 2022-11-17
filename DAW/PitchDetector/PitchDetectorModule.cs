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
using System.Windows.Controls;

namespace DAW.PitchDetector
{
    internal class PitchDetectorModule : IModule
    {
        public string Name => "Pitch Detector";

        PitchDetectorView? view;
        IPlayer? player;

        public UserControl UserInterface => view ?? (view = new PitchDetectorView());

        public void Deactivate()
        {
            
        }

        public void SetFile(string filename)
        {
            if(File.Exists(filename) && view != null)
            {
                AudioData? audioData = AudioData.ReadSamples(filename);
                if(audioData != null)
                {
                    PitchTracker pitchTracker = new PitchTracker(audioData.Format.SampleRate / 300, audioData.Format.SampleRate / 80, audioData.Format.SampleRate);
                    float[] samples = audioData.ChannelData[0];

                    for (int i = 0; i < samples.Length; i++)
                    {
                        pitchTracker.AddSample(samples[i]);
                    }

                    SignalViewModel vs = new SignalViewModel(new FileInfo(filename), audioData.Format,
                        new PlotData(audioData!.ChannelData[0], new FloatRange(-1, 1),
                        new FloatRange(0, audioData.ChannelData[0].Length/(float)audioData.Format.SampleRate)),
                        CreatePitchPlotData.GetPitchPlotData(pitchTracker),
                        GetPitchData(pitchTracker));
                    view.DataContext = vs;
                }
            }
        }

        PlotData GetPitchData(PitchTracker pitchTracker)
        {
            float[] y = pitchTracker.Data.PeriodFits.Select(p => (float)p.Period).ToArray();
            float[] x = pitchTracker.Data.PeriodFits.Select(p => (float)p.Sample/ pitchTracker.SampleRate).ToArray();

            return new PlotData(y,
                new FloatRange(y.Min() - 5, y.Max() + 5),
                new FloatRange(0, pitchTracker.TotalSamples / (float)pitchTracker.SampleRate),
                x, pitchTracker.Data.PeriodFits.ToArray());
        }

        public void SetFolder(string folder)
        {
            
        }

        public void SetPlayer(IPlayer player)
        {
            this.player = player;
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            
        }
    }
}
