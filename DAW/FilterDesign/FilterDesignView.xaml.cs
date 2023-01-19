using DAW.Equalization;
using DAW.Tasks;
using DAW.Utils;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using SignalPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
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
        DateTime filterChanged = DateTime.MinValue;
        JobHandler caclResponseHandler = new JobHandler(1);

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
                vm.ZeroPoles.Clear();
                GenerateResponse();
            }
        }

        private void AddZero_Click(object sender, RoutedEventArgs e)
        {
            float zero;
            if(float.TryParse(zeroTextBox.Text, out zero) &&
                DataContext is FilterDesignViewModel vm)
            {
                //vm.Zeros.Add(zero);
                //zeroTextBox.Text = "";
                //GenerateResponse();
            }
        }

        private void AddPole_Click(object sender, RoutedEventArgs e)
        {
            float zero;
            if (float.TryParse(poleTextBox.Text, out zero) &&
                DataContext is FilterDesignViewModel vm)
            {
                //vm.ZeroPoles.Add(zero);
                //poleTextBox.Text = "";
                //GenerateResponse();
            }
        }

        void GenerateResponse()
        {
            filterChanged = DateTime.UtcNow;

            if (DataContext is FilterDesignViewModel vm)
            {
                List<Complex> poles = vm.ZeroPoles.Where(p => p.IsPole).Select(p => p.Position).ToList();
                List<Complex> zeros = vm.ZeroPoles.Where(p => !p.IsPole).Select(p => p.Position).ToList();
                int sampleRate = vm.SampleRate;
                DateTime lastChanged = filterChanged;

                caclResponseHandler.AddJob(delegate
                {
                    PlotData? plotData = CreateResponse(poles, zeros, sampleRate, lastChanged);

                    if(plotData != null)
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if(lastChanged == filterChanged)
                                responsePlot.DataContext = plotData;
                        }));
                });
            }
        }

        PlotData? CreateResponse(List<Complex> poles, List<Complex> zeros, int sampleRate, DateTime lastChanged)
        {
            IIRFilter filter = new IIRFilter(poles, zeros);
            float freq = 20;
            List<float> xList = new();
            List<float> yList = new();
            float[] tone;
            int period;
            float maxAbs, abs;
            float initialAmp = 0.1f;
            float y;
            float minY = 0, maxY = 0;
            var biquad = BiquadFilter.PeakingEQ(48000, 1000, 3, 6);

            while (freq < 20000)
            {
                period = (int)(sampleRate / freq);
                tone = Tone.GenerateTone(freq, sampleRate, (int)(sampleRate / freq * 5), initialAmp);
                filter.Filter(tone);

                //for (int i = 0; i < tone.Length; i++)
                //    tone[i] = biquad.Transform(tone[i]);

                maxAbs = 0;
                for (int i = tone.Length - 1; i > tone.Length - period - 1; i--)
                {
                    abs = tone[i];
                    if (abs < 0)
                        abs = -1;
                    if (abs > maxAbs)
                        maxAbs = abs;
                }

                if (lastChanged != filterChanged)
                    return null;

                xList.Add(freq);
                y = (float)(20 * Math.Log10(maxAbs / initialAmp));
                yList.Add(y);
                if (y < minY)
                    minY = y;
                if (y > maxY)
                    maxY = y;
                freq *= 1.015f;
            }
            if (minY < -30)
                minY = -30;

            return new PlotData(yList.ToArray(), new FloatRange(minY - 1, maxY + 1),
                new FloatRange(xList[0], xList.Last()), xList.ToArray());
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GenerateResponse();
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void PreviewNumberInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void Zero_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if(textBox != null && DataContext is FilterDesignViewModel vm)
            {
                //WrapPanel? parent = VisualTreeHelper.GetParent(textBox) as WrapPanel;
                //if(parent != null)
                //{
                //    bool isPole = parent.Name == "poleWrap";
                //    Complex complex = (Complex)parent.DataContext;
                //    Complex? newValue = GetNewValue(parent);
                //    if(newValue != null)
                //    {
                //        ObservableCollection<Complex> coll = isPole ? vm.Poles : vm.Zeros;
                //        int index = coll.IndexOf(complex);
                //        if (index > -1)
                //            coll[index] = newValue.Value;
                //        GenerateResponse();
                //    }
                //}
            }
        }

        Complex? GetNewValue(WrapPanel wrapPanel)
        {
            TextBox? real = null, imag = null;
            foreach (var child in wrapPanel.Children)
            {
                if (child is TextBox tb)
                {
                    if (tb.Name == "imag")
                        imag = tb;
                    else if (tb.Name == "real")
                        real = tb;
                }
            }
            double realF, imagF;
            if (real != null && imag != null &&
                double.TryParse(real.Text, out realF) &&
                double.TryParse(imag.Text, out imagF))
            {
                return new Complex(realF, imagF);
            }

            return null;
        }

        private void DragCanvas_ControlMoved(object sender, DragCanvas.ControlMovedArgs e)
        {
            if(e.MovedControl is ContentPresenter cp &&
                cp.DataContext is ZeroPole c &&
                DataContext is FilterDesignViewModel vm &&
                sender is DragCanvas canvas)
            {
                int index = vm.ZeroPoles.IndexOf(c);
                if(index > -1)
                {
                    Complex jg = c.Position.Conjugate();
                    int jgIndex = -1;
                    for(int i = 0; i < vm.ZeroPoles.Count; i++)
                    {
                        if (i != index &&
                            vm.ZeroPoles[i].IsPole == c.IsPole &&
                            vm.ZeroPoles[i].Position == jg)
                        {
                            jgIndex = i;
                        }
                    }

                    //double left = Canvas.GetLeft(cp);
                    //double top = Canvas.GetTop(cp);
                    double real = (e.Left - 5 - 150) / 150.0;
                    double imag = -(e.Top - 5 - 150) / 150.0;
                    Complex newC = new Complex(real, imag);
                    vm.ZeroPoles[index] = new ZeroPole(newC, c.IsPole);
                    if (jgIndex > -1)
                        vm.ZeroPoles[jgIndex] = new ZeroPole(newC.Conjugate(), c.IsPole);
                    GenerateResponse();
                }
            }
        }
    }
}
