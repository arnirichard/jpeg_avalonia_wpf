using DAW.PitchDetector;
using DAW.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                        CreateCapture();
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
                        CreateCapture();
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
                        CreateCapture();
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
                        CreateCapture();
                }
            }
        }

        public ObservableCollection<PitchDetectorViewModel> Records { get; set; } = new ObservableCollection<PitchDetectorViewModel>();

        public RecorderViewModel()
        {
            var enumerator = new MMDeviceEnumerator();
            CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            if(defaultDevice != null)
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

        private void CreateCapture()
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

        public WaveFormat Format => SampleTypeIndex == 0
                    ? WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1)
                    : new WaveFormat(SampleRate, BitDepth, 1);

        private void CaptureOnDataAvailable(object? sender, WaveInEventArgs waveInEventArgs)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {

                }
                catch { }
            });
        }

        internal void Deactivate()
        {
            
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
                        PitchDetectorViewModel vs = new PitchDetectorViewModel(new FileInfo(filename),
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
                    PitchDetectorViewModel vs = new PitchDetectorViewModel(new FileInfo(filename),
                            new PlotData(signal, -1f, 1f, 0f, seconds),
                            CreatePitchPlotData.GetPitchPlotData(new float[signal.Length], SampleRate),
                            Format);
                    Records.Add(vs);
                }
            }
        }
    }
}
