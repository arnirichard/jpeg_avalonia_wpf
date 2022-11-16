﻿using DAW.PitchDetector;
using DAW.Tasks;
using DAW.Utils;
using Microsoft.VisualBasic;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;

namespace DAW.Recorder
{
    internal class RecorderViewModel : ViewModelBase
    {
        MMDevice? device;
        public string? Folder;

        SignalViewModel? recordingSignalVM;
        int recordingIndex = 0;
        WaveFormat? captureFormat;
        
        public IPlayer? Player { get; private set; }

        public bool IsRecording => recordingSignalVM != null;
        public ObservableCollection<SignalViewModel> Records { get; set; } = new ObservableCollection<SignalViewModel>();

        public RecorderViewModel(IPlayer? player)
        {
            Player = player;           
        }

        public void StopRecording()
        {
            if (recordingSignalVM != null)
            {
                recordingSignalVM.SetRecording(false);
                SignalViewModel completedRecording = recordingSignalVM;
                int samples = recordingIndex;
                recordingSignalVM = null;
                OnPropertyChanged("IsRecording");
                

                int index = Records.IndexOf(completedRecording);
                if(index > -1)
                {
                    completedRecording = Records[index] = completedRecording.SetNewLength(samples, true);
                    CreateWave.WriteSingleChannelWave(completedRecording.File.FullName,
                            completedRecording.Format,
                            completedRecording.SignalPlotData.Y);
                }
            }
        }

        public void StartRecording(SignalViewModel signalViewModel)
        {
            WaveFormat? format = captureFormat;

            if (format != null)
            {
                var index = Records.IndexOf(signalViewModel);
                recordingIndex = 0;

                if (index > -1)
                {
                    recordingSignalVM = Records[index] = signalViewModel.SetNewLength(signalViewModel.Format.SampleRate * 5, false);
                }
                else
                {
                    recordingSignalVM = signalViewModel;
                }
                recordingSignalVM.SetRecording(true);
            }
        }

        public void OnCaptureSamplesAvailable(float[] samples, WaveFormat format)
        {
            try
            {
                captureFormat = format;

                SignalViewModel? recData = recordingSignalVM;

                if(recData != null)
                {
                    if (format.SampleRate == recData.Format.SampleRate)
                    {
                        PlotData plotData = recData.SignalPlotData;

                        if (recordingIndex + samples.Length > recData.SignalPlotData.Y.Length)
                        {
                            plotData = new PlotData(new float[recData.SignalPlotData.Y.Length + 5 * recData.Format.SampleRate], 
                                    new FloatRange(-1, 1), 
                                    new FloatRange(0, 5));
                            Array.Copy(recData.SignalPlotData.Y, plotData.Y, recordingIndex);
                        }

                        Array.Copy(samples, 0, recData.SignalPlotData.Y, recordingIndex, samples.Length);
                        recordingIndex += samples.Length;

                        recData.SignalChanged(plotData);
                    }
                    else
                    {
                        StopRecording();
                    }

                            

                }
            }
            catch 
            {
                StopRecording();
            }
                //});
        }      

        public void AddFile(string filename)
        {
            if (!Records.Any(r => r.File.FullName.ToLower() == filename.ToLower()))
            {
                Folder = Path.GetDirectoryName(filename);

                if (File.Exists(filename))
                {
                    AudioData? audioData = AudioData.ReadSamples(filename);
                    if (audioData != null)
                    {
                        SignalViewModel vs = new SignalViewModel(new FileInfo(filename),
                            new PlotData(audioData!.ChannelData[0], 
                                new FloatRange(-1, 1), 
                                new FloatRange(0, audioData.ChannelData[0].Length / (float)audioData.Format.SampleRate)),
                            CreatePitchPlotData.GetPitchPlotData(audioData!.ChannelData[0], audioData.Format.SampleRate),
                            audioData.Format);
                        Records.Add(vs);
                    }
                }
                else if(captureFormat != null)
                {
                    int seconds = 0;
                    float[] signal = new float[48000 * seconds];
                    SignalViewModel vs = new SignalViewModel(new FileInfo(filename),
                            new PlotData(signal, new FloatRange(-1, 1), new FloatRange(0, seconds)),
                            CreatePitchPlotData.GetPitchPlotData(new float[signal.Length], 48000),
                            captureFormat);
                    Records.Add(vs);
                }
            }
        }

        internal void TrimSignal(SignalViewModel record, IntRange interval)
        {
            SignalViewModel newRecord = record.Trim(interval.Start, interval.Length);
            CreateWave.WriteSingleChannelWave(newRecord.File.FullName, newRecord.Format, newRecord.SignalPlotData.Y);
            var index = Records.IndexOf(record);
            if(index > -1)
            {
                Records[index] = newRecord;
            }
        }

        internal void SetPlayer(IPlayer player)
        {
            Player = player;   
        }
    }
}
