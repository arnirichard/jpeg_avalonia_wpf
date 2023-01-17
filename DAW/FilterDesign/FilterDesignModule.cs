using DAW.DftAnalysis;
using DAW.Utils;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW.FilterDesign
{
    class FilterDesignModule : IModule
    {
        public string Name => "Filter Design";
        FilterDesignViewModel viewModel = new FilterDesignViewModel();

        public UserControl UserInterface => new FilterDesignView() { DataContext = viewModel };

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
            
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            
        }
    }
}
