using DAW.Utils;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DAW.FilterDesign
{
    class ZeroPole
    {
        public Complex Position { get; }
        public bool IsPole { get; }
        public double Left => 145 + Position.Real * 125;
        public double Top => 145 - Position.Imaginary * 125;

        public ZeroPole(Complex position, bool isPole)
        {
            Position = position;
            IsPole = isPole;
        }
    }


    class FilterDesignViewModel : ViewModelBase
    {
        public int SampleRate = 48000;
        public ObservableCollection<ZeroPole> ZeroPoles { get; } = new ObservableCollection<ZeroPole>();
        public IIRFilter Filter { get; private set; }

        public FilterDesignViewModel()
        {
            float[] aco = new float[] { 0.9696653590187516f, -1.9528145988251429f, 1};
            float[] bco = new float[] { 1.0150954633340554f, -1.9528145988251429f, 0.95456989568469641f };

            //biquad = { 1.0150954633340554x_n + -1.9528145988251429x_n - 1 + 0.9545698956846964x_n - 2 - -1.9528145988251429y_n - 1 - 0.9696653590187516y_n - 2}
            var poles = Polynomial.GetZeros(aco[2], aco[1], aco[0]);
            var zeros = Polynomial.GetZeros(bco[2], bco[1], bco[0]);

            //List<Complex> poles = new List<Complex>();
            //poles.Add(new Complex(0.95, -0.03));
            //poles.Add(new Complex(0.95, 0.03));
            //poles.Add(new Complex(0.98, .02));
            //poles.Add(new Complex(0.98, -.02));

            //List<Complex> zeros = new List<Complex>();
            //zeros.Add(new Complex(-1, 0));
            //zeros.Add(new Complex(-1, 0));
            //zeros.Add(new Complex(1, 0));
            //zeros.Add(new Complex(1, 0));

            foreach (var p in poles)
                ZeroPoles.Add(new ZeroPole(p, true));

            foreach (var z in zeros)
                ZeroPoles.Add(new ZeroPole(z, false));


            Filter = new IIRFilter(poles, zeros);
        }
    }
}
