using DAW.PitchDetector;
using DAW.Utils;
using NAudio.Gui;
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
        RecorderViewModel viewModel = new RecorderViewModel();

        public UserControl UserInterface => view ?? (view = new RecorderView() { DataContext = viewModel });

        public void Deactivate()
        {
            viewModel.Deactivate();
            view = null;
        }

        public void SetFile(string filename)
        {
            viewModel.AddFile(filename);
        }

        public void SetFolder(string folder)
        {
            viewModel.Folder = folder;
        }
    }
}
