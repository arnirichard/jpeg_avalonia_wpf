using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using DAW.Tasks;
using DAW.Utils;
using NAudio.CoreAudioApi;
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

    class MainWindowViewModel : ViewModelBase, IPlayer
    {
        IModule? selectedModule;
        string? folder;
        List<IModule>? moduleList;
        AudioDevice? captureDevice, playbackDevice;
        DevicesChangedImpl devicesChangedImpl;
        WasapiCapture? capture;
        WaveFormat? captureFormat;
        JobHandler recordingHandler = new JobHandler(1);

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
            devicesChangedImpl = new DevicesChangedImpl(this);
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

        //public int ShareModeIndex { get; set; }

        //public int SampleTypeIndex
        //{
        //    get => sampleTypeIndex;
        //    set
        //    {
        //        if (sampleTypeIndex != value)
        //        {
        //            sampleTypeIndex = value;
        //            OnPropertyChanged("SampleTypeIndex");
        //            BitDepth = sampleTypeIndex == 1 ? 16 : 32;
        //            OnPropertyChanged("IsBitDepthConfigurable");
        //            if (SelectedDevice != null)
        //                StartCapture();
        //        }
        //    }
        //}
        //public int SampleRate
        //{
        //    get => sampleRate;
        //    set
        //    {
        //        if (sampleRate != value)
        //        {
        //            sampleRate = value;
        //            OnPropertyChanged("SampleRate");
        //            if (SelectedDevice != null)
        //                StartCapture();
        //        }
        //    }
        //}

        //public int BitDepth
        //{
        //    get => bitDepth;
        //    set
        //    {
        //        if (bitDepth != value)
        //        {
        //            bitDepth = value;
        //            OnPropertyChanged("BitDepth");
        //            if (SelectedDevice != null)
        //                StartCapture();
        //        }
        //    }
        //}

        //public WaveFormat Format => SampleTypeIndex == 0
        //            ? WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1)
        //            : new WaveFormat(SampleRate, BitDepth, 1);

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
            var enumerator = new MMDeviceEnumerator();
            PlaybackDevices.Clear();
            CaptureDevices.Clear();
            foreach (var d in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                PlaybackDevices.Add(new AudioDevice(d));
            foreach (var d in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                CaptureDevices.Add(new AudioDevice(d));

            if (playbackDevice == null || !PlaybackDevices.Any(d => d.Device.ID == playbackDevice.Device.ID))
            {
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                if (defaultDevice != null)
                    SelectedPlaybackDevice = PlaybackDevices.FirstOrDefault(c => c.Device.ID == defaultDevice.ID);
            }

            if (captureDevice == null || !CaptureDevices.Any(d => d.Device.ID == captureDevice.Device.ID))
            {
                var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
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
            StopCapture();
            foreach (var m in Modules)
                m.Deactivate();
        }

        class DevicesChangedImpl : NAudio.CoreAudioApi.Interfaces.IMMNotificationClient
        {
            public MainWindowViewModel vm;

            public DevicesChangedImpl(MainWindowViewModel vm)
            {
                this.vm = vm;
            }

            public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
            {
                vm.SetDevices();
            }

            public void OnDeviceAdded(string pwstrDeviceId)
            {
                vm.SetDevices();
            }

            public void OnDeviceRemoved(string deviceId)
            {
                vm.SetDevices();
            }

            public void OnDeviceStateChanged(string deviceId, DeviceState newState)
            {
                vm.SetDevices();
            }

            public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
            {
                vm.SetDevices();
            }
        }
    }
}
