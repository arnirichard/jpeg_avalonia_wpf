using DAW.PitchDetector;
using DAW.Utils;
using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW.DftAnalysis
{
    internal class DftAnalysisModule : IModule
    {
        public string Name => "Dft Analysis";

        public UserControl UserInterface  => new DftAnalysisView() { DataContext = DftAnalysisViewModel.DftAnalysis };

        public void Deactivate()
        {
            
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            
        }

        public void SetFile(string filename)
        {
            if (File.Exists(filename))
            {
                AudioData? audioData = AudioData.ReadSamples(filename);
                if (audioData != null)
                {
                    SignalViewModel vs = new SignalViewModel(new FileInfo(filename), audioData.Format,
                        new PlotData(audioData!.ChannelData[0], new FloatRange(-1, 1),
                        new FloatRange(0, audioData.ChannelData[0].Length / (float)audioData.Format.SampleRate)));
                    vs.SetPitchData();
                    DftAnalysisViewModel.DftAnalysis.SetSignal(vs);
                }
            }
        }

        public void SetFolder(string folder)
        {
            
        }

        public void SetPlayer(IPlayer player)
        {
            DftAnalysisViewModel.DftAnalysis.SetPlayer(player);
        }
    }
}
