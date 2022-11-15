using DAW.PitchDetector;
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
        int sampleRate;
        int sampleTypeIndex; // 0: IEEE Float, 1: PCM
        int bitDepth;
        MMDevice? device;
        private WasapiCapture? capture;
        public string? Folder;

        SignalViewModel? recordingSignalVM;
        int recordingIndex = 0;
        JobHandler recordingHandler = new JobHandler(1);

        public bool IsRecording => recordingSignalVM != null;
        public ObservableCollection<SignalViewModel> Records { get; set; } = new ObservableCollection<SignalViewModel>();

        public IEnumerable<MMDevice> CaptureDevices { get; }
        public MMDevice? SelectedDevice
        {
            get => device;
            set
            {
                if (device != value)
                {
                    device = value;
                    OnPropertyChanged("SelectedDevice");
                    if (SelectedDevice != null)
                        StartCapture();
                }
            }
        }
        public int ShareModeIndex { get; set; }

        public int SampleTypeIndex
        {
            get => sampleTypeIndex;
            set
            {
                if (sampleTypeIndex != value)
                {
                    sampleTypeIndex = value;
                    OnPropertyChanged("SampleTypeIndex");
                    BitDepth = sampleTypeIndex == 1 ? 16 : 32;
                    OnPropertyChanged("IsBitDepthConfigurable");
                    if (SelectedDevice != null)
                        StartCapture();
                }
            }
        }
        public int SampleRate
        {
            get => sampleRate;
            set
            {
                if (sampleRate != value)
                {
                    sampleRate = value;
                    OnPropertyChanged("SampleRate");
                    if (SelectedDevice != null)
                        StartCapture();
                }
            }
        }
        public int BitDepth
        {
            get => bitDepth;
            set
            {
                if (bitDepth != value)
                {
                    bitDepth = value;
                    OnPropertyChanged("BitDepth");
                    if (SelectedDevice != null)
                        StartCapture();
                }
            }
        }

        public RecorderViewModel()
        {
            var enumerator = new MMDeviceEnumerator();
            CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            if (defaultDevice != null)
            {
                SetDefault(defaultDevice);
                SelectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
            }
        }

        void SetDefault(MMDevice device)
        {
            using (var c = new WasapiCapture(device))
            {
                SampleTypeIndex = c.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat ? 0 : 1;
                SampleRate = c.WaveFormat.SampleRate;
                BitDepth = c.WaveFormat.BitsPerSample;
            }
        }

        private void StartCapture()
        {
            StopCapture();

            if (SampleRate <= 0 || SelectedDevice == null)
                return;

            try
            {
                capture = new WasapiCapture(SelectedDevice);
                capture.ShareMode = ShareModeIndex == 0 ? AudioClientShareMode.Shared : AudioClientShareMode.Exclusive;
                capture.WaveFormat = Format;
                capture.StartRecording();
                capture.DataAvailable += CaptureOnDataAvailable;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        public void StopRecording()
        {
            if (recordingSignalVM != null)
            {
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

        void RunOnUIThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        public void StartRecording(SignalViewModel signalViewModel)
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
        }

        public WaveFormat Format => SampleTypeIndex == 0
                    ? WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1)
                    : new WaveFormat(SampleRate, BitDepth, 1);

        private void CaptureOnDataAvailable(object? sender, WaveInEventArgs waveInEventArgs)
        {
            if (recordingSignalVM != null)
                recordingHandler.AddJob(delegate
                {
                    try
                    {
                        SignalViewModel? recData = recordingSignalVM;

                        if (recData == null)
                            return;

                        if(recData != null && 
                            recData.Format.Channels == 1 &&
                            recordingIndex < recData.SignalPlotData.Y.Length)
                        {
                            byte[] bytes = waveInEventArgs.Buffer;
                            int samples = bytes.Length / recData.Format.BitsPerSample * 4;
                            PlotData? newPlotData = null;
                            if (recordingIndex + samples > recData.SignalPlotData.Y.Length)
                            {
                                newPlotData = new PlotData(new float[recData.SignalPlotData.Y.Length+5*recData.Format.SampleRate], -1, 1, 0, 5);
                                Array.Copy(recData.SignalPlotData.Y, newPlotData.Y, recordingIndex);
                            }

                            float[] floats = recData.SignalPlotData.Y;

                            if (recordingIndex < floats.Length)
                            {
                                int offset = 0;
                                for (int i = 0; i < samples; i++)
                                {
                                    if (recData.Format.BitsPerSample == 16)
                                    {
                                        floats[recordingIndex++] = BitConverter.ToInt16(bytes, offset) / 32768f;
                                        offset += 2;
                                    }
                                    else if (recData.Format.BitsPerSample == 24)
                                    {
                                        floats[recordingIndex++] = (((sbyte)bytes[offset + 2] << 16) | (bytes[offset + 1] << 8) | bytes[offset]) / 8388608f;
                                        offset += 3;
                                    }
                                    else if (recData.Format.BitsPerSample == 32 && recData.Format.Encoding == WaveFormatEncoding.IeeeFloat)
                                    {
                                        floats[recordingIndex++] = BitConverter.ToSingle(bytes, offset);
                                        offset += 4;
                                    }
                                    else if (recData.Format.BitsPerSample == 32)
                                    {
                                        floats[recordingIndex++] = BitConverter.ToInt32(bytes, offset) / (Int32.MaxValue + 1f);
                                        offset += 4;
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException("Unsupported bit depth");
                                    }
                                }

                                recData.SignalChanged(newPlotData);
                            }
                        }
                    }
                    catch 
                    {
                        StopRecording();
                    }
                });
        }

        internal void Deactivate()
        {
            StopCapture();
        }

        void StopCapture()
        {
            if (capture != null)
            {
                capture.StopRecording();
                capture.DataAvailable -= CaptureOnDataAvailable;
                capture = null;
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
                        SignalViewModel vs = new SignalViewModel(new FileInfo(filename),
                            new PlotData(audioData!.ChannelData[0], -1f, 1f, 0f, audioData.ChannelData[0].Length / (float)audioData.Format.SampleRate),
                            CreatePitchPlotData.GetPitchPlotData(audioData!.ChannelData[0], audioData.Format.SampleRate),
                            audioData.Format);
                        Records.Add(vs);
                    }
                }
                else if (SampleRate > 0)
                {
                    int seconds = 0;
                    float[] signal = new float[SampleRate * seconds];
                    SignalViewModel vs = new SignalViewModel(new FileInfo(filename),
                            new PlotData(signal, -1f, 1f, 0f, seconds),
                            CreatePitchPlotData.GetPitchPlotData(new float[signal.Length], SampleRate),
                            Format);
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
    }
}
