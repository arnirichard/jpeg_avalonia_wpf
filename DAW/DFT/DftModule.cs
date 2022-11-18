using DAW.PitchDetector;
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

namespace DAW.DFT
{
    class DftModule : IModule
    {
        public string Name => "DFT";

        DftView? view;
        IPlayer? player;
        DftViewModel viewModel = new DftViewModel();

        public UserControl UserInterface => view ?? (view = new DftView() { DataContext = viewModel });

        public void Deactivate()
        {

        }

        public void SetFile(string filename)
        {
            if (File.Exists(filename) && view != null)
            {
                AudioData? audioData = AudioData.ReadSamples(filename);
                if (audioData != null)
                {
                    SignalViewModel vs = new SignalViewModel(new FileInfo(filename), audioData.Format,
                        new PlotData(audioData!.ChannelData[0], new FloatRange(-1, 1),
                        new FloatRange(0, audioData.ChannelData[0].Length / (float)audioData.Format.SampleRate)));
                    view.DataContext = vs;
                }
            }
        }

        public void SetFolder(string folder)
        {

        }

        public void SetPlayer(IPlayer player)
        {
            this.player = player;
            viewModel.SetPlayer(player);
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {

        }
    }
}
