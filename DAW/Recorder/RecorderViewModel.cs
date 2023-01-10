using DAW.PitchDetector;
using DAW.Tasks;
using DAW.Utils;
using Microsoft.VisualBasic;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PitchDetector;
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


namespace DAW.Recorder
{
    internal class RecorderViewModel : ViewModelBase
    {
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
                signalViewModel.SetWaveFormat(format);
                recordingIndex = 0;

                if (index > -1)
                {
                    recordingSignalVM = Records[index] = signalViewModel.SetNewLength(signalViewModel.Format.SampleRate * 5, false);
                }
                else
                {
                    recordingSignalVM = signalViewModel;
                }
                recordingSignalVM?.SetRecording(true);
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
                            int newLength = recData.SignalPlotData.Y.Length + 5 * recData.Format.SampleRate;
                            plotData = new PlotData(new float[newLength], 
                                    new FloatRange(-1, 1), 
                                    new FloatRange(0, newLength/ (float)recData.Format.SampleRate));
                            Array.Copy(recData.SignalPlotData.Y, plotData.Y, recordingIndex);
                            recData.SignalChanged(plotData);
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
            catch (Exception ex)
            {
                StopRecording();
            }
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
                        //PitchTracker pitchTracker = new PitchTracker(audioData.Format.SampleRate / 300, audioData.Format.SampleRate / 80, audioData.Format.SampleRate);
                        //float[] samples = audioData.ChannelData[0];

                        //for (int i = 0; i < samples.Length; i++)
                        //{
                        //    pitchTracker.AddSample(samples[i]);
                        //}

                        SignalViewModel vs = new SignalViewModel(new FileInfo(filename), audioData.Format,
                            new PlotData(audioData!.ChannelData[0], 
                                new FloatRange(-1, 1), 
                                new FloatRange(0, audioData.ChannelData[0].Length / (float)audioData.Format.SampleRate)));
                        Records.Add(vs);
                    }
                }
                else if(captureFormat != null)
                {
                    int seconds = 0;
                    float[] signal = new float[captureFormat.SampleRate * seconds];
                    SignalViewModel vs = new SignalViewModel(new FileInfo(filename), captureFormat,
                            new PlotData(signal, new FloatRange(-1, 1), new FloatRange(0, seconds)));
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
