using DAW.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DAW.FilterDesign
{
    class FilterDesignViewModel : ViewModelBase
    {
        public int SampleRate = 48000;
        public ObservableCollection<Complex> Zeros { get; } = new ObservableCollection<Complex>();
        public ObservableCollection<Complex> Poles { get; } = new ObservableCollection<Complex>();
        public IIRFilter Filter { get; private set; }


    }
}
