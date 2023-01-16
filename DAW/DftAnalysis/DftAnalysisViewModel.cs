using DAW.PitchDetector;
using DAW.Utils;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAW.DftAnalysis
{
    internal class DftAnalysisViewModel : ViewModelBase
    {
        public static DftAnalysisViewModel DftAnalysis = new DftAnalysisViewModel();

        public SignalViewModel? Signal { get; private set; }
        public PlotData? DFT1 { get; private set; }
        public IPlayer? Player { get; private set; }

        public void SetSignal(SignalViewModel signal)
        {
            Signal = signal;
            OnPropertyChanged("Signal");
        }

        public void SetDFT1(PlotData dft1)
        {
            DFT1 = dft1;
            OnPropertyChanged("DFT1");
        }

        internal void SetPlayer(IPlayer player)
        {
            Player = player;
        }
    }
}
