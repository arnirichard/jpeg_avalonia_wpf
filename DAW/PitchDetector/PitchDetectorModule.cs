﻿using DAW.Utils;
using PitchDetector;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW.PitchDetector
{
    internal class PitchDetectorModule : IModule
    {
        public string Name => "Pitch Detector";

        PitchDetectorView? view;

        public UserControl UserInterface => view ?? (view = new PitchDetectorView());

        public void Deactivate()
        {
            
        }

        public void SetFile(string filename)
        {
            if(File.Exists(filename) && view != null)
            {
                AudioData? audioData = AudioData.ReadSamples(filename);
                if(audioData != null)
                {
                    PitchDetectorViewModel vs = new PitchDetectorViewModel(new FileInfo(filename),
                        new PlotData(audioData!.ChannelData[0], -1f, 1f, 0f, audioData.ChannelData[0].Length/(float)audioData.Format.SampleRate),
                        CreatePitchPlotData.GetPitchPlotData(audioData!.ChannelData[0], audioData.Format.SampleRate),
                        audioData.Format);
                    view.DataContext = vs;
                }
            }
        }

        public void SetFolder(string folder)
        {
            
        }
    }
}
