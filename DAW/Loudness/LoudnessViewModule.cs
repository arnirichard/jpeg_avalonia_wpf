using DAW.Equalization;
using DAW.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Loudness
{
    static class Data
    {
        public static int[] Frequencies = new int[] { 20, 40, 60, 80, 100, 125, 150, 200, 300, 400, 500, 630, 800, 1000, 1200, 1500, 2000, 2500, 3000, 4000, 5000, 6000, 8000, 10000, 12500, 15000, 17500, 20000 };
        public const string LoudnessFileName = "LOUDNESS.txt";
    }
    

    internal class LoudnessViewModule : ViewModelBase
    {
        public ObservableCollection<Gain> GainMap { get; private set; } = new();

        public int SampleRate = 48000;
        public float DefaultAmplitude = 0.005f;
        public IPlayer? Player { get; set; }
        

        public LoudnessViewModule()
        {
            foreach (var f in Data.Frequencies)
                GainMap.Add(new Gain(f, 0));
            LoadGainMap();
        }

        internal void SaveGainMap()
        {
            File.WriteAllLines(Data.LoudnessFileName, GainMap.Select(g => string.Format("{0}:{1}", g.Frequency, g.Decibel)));
        }

        internal void LoadGainMap()
        {
            if (!File.Exists(Data.LoudnessFileName))
                return;

            var lines = File.ReadAllLines(Data.LoudnessFileName);
            int freq;
            double db;
            foreach(var line in lines)
            {
                int ind = line.IndexOf(":");
                if(ind > 0 && 
                    int.TryParse(line.Substring(0, ind), out freq) &&
                    double.TryParse(line.Substring(ind+1), out db))
                {
                    var g = GainMap.FirstOrDefault(g => g.Frequency == freq);
                    if(g != null)
                        g.Decibel = db;
                }
            }
        }
    }
}
