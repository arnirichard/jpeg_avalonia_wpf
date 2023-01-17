using DAW.Equalization;
using DAW.Utils;
using MathNet.Numerics.LinearAlgebra;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DAW.FilterDesign
{
    /// <summary>
    /// Interaction logic for FilterDesignView.xaml
    /// </summary>
    public partial class FilterDesignView : UserControl
    {
        public FilterDesignView()
        {
            InitializeComponent();

            responsePlot.HorizontalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(0, 10f, false, Plot.Beige, 20),
                new LinesDefinition(0, 5f, false, Plot.Beige, 20),
                new LinesDefinition(0, 1f, false, Plot.Beige, 10),
            });
            responsePlot.VerticalLines.AddRange(new List<LinesDefinition>()
            {
                new LinesDefinition(100, 100f, false, Plot.Beige, 40),
                new LinesDefinition(500, 500f, false, Plot.Beige, 40),
                new LinesDefinition(1000, 1000f, false, Plot.Beige, 40),
            });
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilterDesignViewModel vm)
            {
                vm.Zeros.Clear();
                vm.Poles.Clear();
                GenerateResponse();
            }
        }

        private void AddZero_Click(object sender, RoutedEventArgs e)
        {
            float zero;
            if(float.TryParse(zeroTextBox.Text, out zero) &&
                DataContext is FilterDesignViewModel vm)
            {
                vm.Zeros.Add(zero);
                zeroTextBox.Text = "";
                GenerateResponse();
            }
        }

        private void AddPole_Click(object sender, RoutedEventArgs e)
        {
            float zero;
            if (float.TryParse(poleTextBox.Text, out zero) &&
                DataContext is FilterDesignViewModel vm)
            {
                vm.Poles.Add(zero);
                poleTextBox.Text = "";
                GenerateResponse();
            }
        }

        void GenerateResponse()
        {
            if (DataContext is FilterDesignViewModel vm)
            {
                IIRFilter filter = new IIRFilter(vm.Poles.ToList(), vm.Zeros.ToList());
                float freq = 20;
                List<float> xList = new();
                List<float> yList = new();
                float[] tone;
                int period;
                float maxAbs, abs;
                float initialAmp = 0.1f;
                float y;
                float minY = 0, maxY = 0;
                while (freq < 20000)
                {
                    period = (int)(vm.SampleRate / freq);
                    tone = Tone.GenerateTone(freq, vm.SampleRate, (int)(vm.SampleRate / freq * 5), initialAmp);
                    filter.Filter(tone);

                    maxAbs = 0;
                    for (int i = tone.Length - 1; i > tone.Length - period - 1; i--)
                    {
                        abs = tone[i];
                        if (abs < 0)
                            abs = -1;
                        if (abs > maxAbs)
                            maxAbs = abs;
                    }

                    xList.Add(freq);
                    y = (float)(20 * Math.Log10(maxAbs / initialAmp));
                    yList.Add(y);
                    if (y < minY)
                        minY = y;
                    if (y > maxY)
                        maxY = y;
                    freq *= 1.02f;
                }
                if (minY < -30)
                    minY = -30;

                responsePlot.DataContext = new PlotData(yList.ToArray(), new FloatRange(minY - 1, maxY + 1),
                    new FloatRange(xList[0], xList.Last()), xList.ToArray());
            }
        }
    }
}
