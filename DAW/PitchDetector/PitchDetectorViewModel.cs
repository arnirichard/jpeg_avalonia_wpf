using PitchDetector;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.PitchDetector
{
    internal class PitchDetectorViewModel
    {
        public FileInfo File { get; private set; }
        public PlotData SignalPlotData { get; private set; }
        public PlotData PitchPlotData { get; private set; }

        public PitchDetectorViewModel(FileInfo file, PlotData signalPlotData, PlotData pitchPlotData)
        {
            File = file;
            SignalPlotData = signalPlotData;
            PitchPlotData = pitchPlotData;
        }
    }
}
