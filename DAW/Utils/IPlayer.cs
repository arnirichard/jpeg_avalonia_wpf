using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    public interface IPlayer
    {
        void Play(float[] floats, int sampleRate, IntRange? range = null);
    }
}
