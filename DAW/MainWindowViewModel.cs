using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DAW.Tasks;
using DAW.Utils;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using SignalPlot;
using static System.Windows.Forms.DataFormats;

namespace DAW
{
    class AudioDevice
    {
        public MMDevice Device { get; private set; }
        public string Name { get; }

        public AudioDevice(MMDevice device)
        {
            Device = device;
            Name = device.ToString();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class MainWindowViewModel : ViewModelBase, IPlayer, IMMNotificationClient
    {
        IModule? selectedModule;
        string? folder;
        List<IModule>? moduleList;
        AudioDevice? captureDevice, playbackDevice;
        WasapiCapture? capture;
        WaveFormat? captureFormat;
        JobHandler recordingHandler = new JobHandler(1);
        MMDeviceEnumerator? deviceEnumerator;

        public MainWindowViewModel(IEnumerable<IModule> modules)
        {
            SetDevices();

            Modules = modules.OrderBy(m => m.Name).ToList();
            moduleList = Modules.ToList();
            foreach (var module in Modules)
            {
                module.SetPlayer(this);
            }
            if (Modules.Count > 0)
            {
                SelectedModule = Modules[0];
            }
            deviceEnumerator?.RegisterEndpointNotificationCallback(this);
        }

        public List<IModule> Modules { get; }
        public ObservableCollection<FileInfo> Files { get; } = new ObservableCollection<FileInfo>();
        public ObservableCollection<AudioDevice> PlaybackDevices { get; } = new();
        public ObservableCollection<AudioDevice> CaptureDevices { get; } = new();

        public AudioDevice? SelectedCaptureDevice
        {
            get => captureDevice;
            set
            {
                if (captureDevice != value)
                {
                    captureDevice = value;
                    OnPropertyChanged("SelectedCaptureDevice");
                    StartCapture();
                }
            }
        }

        public AudioDevice? SelectedPlaybackDevice
        {
            get => playbackDevice;
            set
            {
                if (playbackDevice != value)
                {
                    playbackDevice = value;
                    OnPropertyChanged("SelectedPlaybackDevice");
                }
            }
        }

        public string? Folder 
        {
            get => folder;
            set
            {
                if (folder != value)
                {
                    folder = value;
                    OnPropertyChanged("Folder");
                }
            }
        }

        public IModule? SelectedModule
        {
            get => selectedModule;
            set
            {
                if (value != selectedModule)
                {
                    selectedModule = value;
                    OnPropertyChanged("SelectedModule");
                    OnPropertyChanged("UserInterface");
                }
            }
        }

        public UserControl? UserInterface => SelectedModule?.UserInterface;

        public void Play(float[] samples, int sampleRate, IntRange? range)
        {
            if(playbackDevice != null)
            {
                IWaveProvider provider = new RawSourceWaveStream(
                    new MemoryStream(PlayFloats.Get32BitSamplesWaveData(samples, range)),
                    new WaveFormat(sampleRate, 32, 1));

                WasapiOut wasapiOut = new WasapiOut(playbackDevice.Device, AudioClientShareMode.Shared, false, 0);
                wasapiOut.Init(provider);
                wasapiOut.Play();
            }
        }

        void SetDevices()
        {
            deviceEnumerator = new MMDeviceEnumerator();
            PlaybackDevices.Clear();
            CaptureDevices.Clear();
            foreach (var d in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                PlaybackDevices.Add(new AudioDevice(d));
            foreach (var d in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                CaptureDevices.Add(new AudioDevice(d));

            if (playbackDevice == null || !PlaybackDevices.Any(d => d.Device.ID == playbackDevice.Device.ID))
            {
                var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                if (defaultDevice != null)
                    SelectedPlaybackDevice = PlaybackDevices.FirstOrDefault(c => c.Device.ID == defaultDevice.ID);
            }

            if (captureDevice == null || !CaptureDevices.Any(d => d.Device.ID == captureDevice.Device.ID))
            {
                var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                if (defaultDevice != null)
                    SelectedCaptureDevice = CaptureDevices.FirstOrDefault(c => c.Device.ID == defaultDevice.ID);
            }
        }

        void StartCapture()
        {
            StopCapture();

            if (captureDevice == null)
                return;

            try
            {
                capture = new WasapiCapture(captureDevice.Device);
                capture.ShareMode = AudioClientShareMode.Shared;
                capture.WaveFormat = captureFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 1);
                capture.StartRecording();
                capture.DataAvailable += CaptureOnDataAvailable;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
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

        private void CaptureOnDataAvailable(object? sender, WaveInEventArgs waveInEventArgs)
        {
            recordingHandler.AddJob(delegate
            {
                WaveFormat? waveFormat = captureFormat;

                if (waveFormat != null)
                {
                    float[] samples = new float[waveInEventArgs.BytesRecorded / waveFormat.BitsPerSample * 8];
                    int offset = 0;
                    byte[] bytes = waveInEventArgs.Buffer;

                    for (int i = 0; i < samples.Length; i++)
                    {
                        if (waveFormat.BitsPerSample == 16)
                        {
                            samples[i] = BitConverter.ToInt16(bytes, offset) / 32768f;
                            offset += 2;
                        }
                        else if (waveFormat.BitsPerSample == 24)
                        {
                            samples[i] = (((sbyte)bytes[offset + 2] << 16) | (bytes[offset + 1] << 8) | bytes[offset]) / 8388608f;
                            offset += 3;
                        }
                        else if (waveFormat.BitsPerSample == 32 && waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        {
                            samples[i] = BitConverter.ToSingle(bytes, offset);
                            offset += 4;
                        }
                        else if (waveFormat.BitsPerSample == 32)
                        {
                            samples[i] = BitConverter.ToInt32(bytes, offset) / (Int32.MaxValue + 1f);
                            offset += 4;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported bit depth");
                        }
                    }

                    var modules = moduleList;

                    if(modules != null)
                        foreach (var m in modules)
                            m.OnCaptureSamplesAvailable(samples, waveFormat);
                }
            });
        }

        internal void Deactivate()
        {
              deviceEnumerator?.UnregisterEndpointNotificationCallback(this);
            StopCapture();
            foreach (var m in Modules)
                m.Deactivate();
        }

        void UpdateDevicesUiThread()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    SetDevices();
                }
                catch { }
            }));
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            UpdateDevicesUiThread();
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            UpdateDevicesUiThread();
        }

        public void OnDeviceRemoved(string deviceId)
        {
            UpdateDevicesUiThread();
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            UpdateDevicesUiThread();
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            
        }
    }
}
