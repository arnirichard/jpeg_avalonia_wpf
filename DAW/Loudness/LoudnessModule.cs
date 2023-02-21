using DAW.FilterDesign;
using DAW.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW.Loudness
{
    internal class LoudnessModule : IModule
    {
        public string Name => "Loudness";

        public UserControl UserInterface => new LoudnessView() { DataContext = viewModel };

        LoudnessViewModule viewModel = new LoudnessViewModule();

        public void Deactivate()
        {
            
        }

        public void SetFile(string filename)
        {
            
        }

        public void SetFolder(string folder)
        {
            
        }

        public void SetPlayer(IPlayer player)
        {
            viewModel.Player = player;
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            
        }
    }
}
