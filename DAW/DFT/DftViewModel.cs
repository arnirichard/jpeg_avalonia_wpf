using DAW.PitchDetector;
using DAW.Utils;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.DFT
{
    class DftViewModel : ViewModelBase
    {
        public IPlayer? Player { get; private set; }

        public SignalViewModel? Signal { get; private set; }

        public PlotData? DFT { get; private set; }

        public DftViewModel()
        {
            
        }

        internal void SetPlayer(IPlayer player)
        {
            Player = player;
        }

        public void SetSignal(SignalViewModel? signal)
        {
            Signal = signal;
            OnPropertyChanged("Signal");
        }

        public void SetDft(PlotData dft) 
        { 
            DFT = dft;
            OnPropertyChanged("DFT");
        }
    }
}
