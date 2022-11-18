﻿using DAW.Utils;
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

namespace DAW.PitchDetector
{
    internal class PitchDetectorModule : IModule
    {
        public string Name => "Pitch Detector";

        PitchDetectorView? view;
        IPlayer? player;

        public UserControl UserInterface => view ?? (view = new PitchDetectorView(player));

        public PitchDetectorModule()
        {
            var u = UserInterface;
        }

        public void Deactivate()
        {
            
        }

        public void SetFile(string filename)
        {
            if(File.Exists(filename) && view != null)
            {
                AudioData? audioData = null;

                try
                {
                    audioData = AudioData.ReadSamples(filename);
                }
                catch(Exception ex)
                {

                }
                if(audioData != null)
                {
                    SignalViewModel vs = new SignalViewModel(new FileInfo(filename), audioData.Format,
                        new PlotData(audioData!.ChannelData[0], new FloatRange(-1, 1),
                            new FloatRange(0, audioData.ChannelData[0].Length/(float)audioData.Format.SampleRate)));
                    vs.SetPitchData();
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
            view?.SetPlayer(player);
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            
        }
    }
}
