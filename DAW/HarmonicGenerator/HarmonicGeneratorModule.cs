using DAW.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW.HarmonicGenerator
{
    class HarmonicGeneratorModule : IModule
    {
        public string Name => "Harmonic Generator";
        IPlayer? player;
        HarmoicGeneratorViewModel? vm;
        HarmonicGeneratorView? view;

        public UserControl UserInterface => view ?? (view = new HarmonicGeneratorView()
        {
            DataContext = vm=  new HarmoicGeneratorViewModel()
            {
                Player = player
            }
        });

        public void Deactivate()
        {
            
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
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
            this.player = player;
            if (vm != null)
                vm.Player = player;
        }
    }
}
