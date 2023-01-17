using DAW.DftAnalysis;
using DAW.PitchDetector;
using DAW.Utils;
using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW.Equalization
{
    class EqualizationModule : IModule
    {
        public string Name => "Equalization";
        EqualizationViewModule viewModule = new EqualizationViewModule();

        public UserControl UserInterface => new EqualizationView() { DataContext = viewModule };

        public void Deactivate()
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
                    viewModule.SetSignal(vs);
                }
            }
        }

        public void SetFolder(string folder)
        {
            
        }

        public void SetPlayer(IPlayer player)
        {
            viewModule.SetPlayer(player);
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            
        }
    }
}
