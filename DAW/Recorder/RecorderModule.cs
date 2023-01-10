using DAW.PitchDetector;
using DAW.Utils;
using NAudio.CoreAudioApi;
using NAudio.Gui;
using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW.Recorder
{
    internal class RecorderModule : IModule
    {
        public string Name => "Recorder";

        RecorderView? view;
        RecorderViewModel? viewModel;
        IPlayer? player;
        string? folder;

        public UserControl UserInterface => view ?? (view = new RecorderView() 
        {
            DataContext = viewModel = new RecorderViewModel(player) { Folder= folder }
        });

        public RecorderModule() 
        {
            
        }

        public void Deactivate()
        {
            view = null;
            viewModel = null;
        }

        public void SetFile(string filename)
        {
            viewModel?.AddFile(filename);
        }

        public void SetFolder(string folder)
        {
            this.folder = folder;

            if (viewModel!= null)
                viewModel.Folder = folder;
        }

        public void SetPlayer(IPlayer player)
        {
            this.player = player;
            viewModel?.SetPlayer(player);
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            viewModel?.OnCaptureSamplesAvailable(samples, format);
        }
    }
}
