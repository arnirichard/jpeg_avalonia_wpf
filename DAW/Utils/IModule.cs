using DAW.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DAW
{
    public interface IModule
    {
        string Name { get; }
        UserControl UserInterface { get; }
        void Deactivate();
        void SetFile(string filename);
        void SetFolder(string folder);
        void SetPlayer(IPlayer player);
        void OnCaptureSamplesAvailable(float[] samples, WaveFormat format);
    }
}
