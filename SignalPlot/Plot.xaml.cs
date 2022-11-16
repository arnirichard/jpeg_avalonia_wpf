using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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
using static System.Net.Mime.MediaTypeNames;

namespace SignalPlot
{
    public partial class Plot : UserControl
    {
        public static int Red = int.Parse("FF0000", System.Globalization.NumberStyles.HexNumber);
        public static int Orange = int.Parse("FF6A00", System.Globalization.NumberStyles.HexNumber);
        public static int Black = int.Parse("000000", System.Globalization.NumberStyles.HexNumber);
        public static int White = int.Parse("FFFFFF", System.Globalization.NumberStyles.HexNumber);
        public static int Beige = int.Parse("DDDDDD", System.Globalization.NumberStyles.HexNumber);
        public static int Blue = int.Parse("0026FF", System.Globalization.NumberStyles.HexNumber);
        
        public static int SelectedIntervalColor = Orange;

        public int SignalColor
        {
            get { return (int)GetValue(SignalColorProperty); }
            set { SetValue(SignalColorProperty, value); }
        }

        public static readonly DependencyProperty SignalColorProperty =
            DependencyProperty.Register("SignalColor", typeof(int), typeof(Plot), new PropertyMetadata(Blue));

        public int BackgroundColor
        {
            get { return (int)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(int), typeof(Plot), new PropertyMetadata(White));

        public IntRange Interval
        {
            get { return (IntRange)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(IntRange), typeof(Plot), new PropertyMetadata(new IntRange(), PropertyChanged));

        public int? CurrentIndex
        {
            get { return (int?)GetValue(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrentIndexProperty =
            DependencyProperty.Register("CurrentIndex", typeof(int?), typeof(Plot), new PropertyMetadata(null, CurrentIndexChanged));

        public float MaxPeak
        {
            get { return (float)GetValue(MaxPeakProperty); }
            set { SetValue(MaxPeakProperty, value); }
        }

        public static readonly DependencyProperty MaxPeakProperty =
            DependencyProperty.Register("MaxPeak", typeof(float), typeof(Plot), new PropertyMetadata(0f));

        static void CurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                d.SetCurrentValue(CurrentValueProperty, plot.GetCurrentValue());
                int index = plot.CurrentIndex ?? -1;
                d.SetCurrentValue(CurrentXProperty, index > -1
                    ? plot.GetXRange(new IntRange(index, index)).Start
                    : null);
            }
        }

        public float? CurrentValue
        {
            get { return (float?)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register("CurrentValue", typeof(float?), typeof(Plot), new PropertyMetadata(null));

        public float? CurrentX
        {
            get { return (float?)GetValue(CurrentXProperty); }
            set { SetValue(CurrentXProperty, value); }
        }

        public static readonly DependencyProperty CurrentXProperty =
            DependencyProperty.Register("CurrentX", typeof(float?), typeof(Plot), new PropertyMetadata(null));

        public FloatRange XRange
        {
            get { return (FloatRange)GetValue(XRangeProperty); }
            set { SetValue(XRangeProperty, value); }
        }

        public static readonly DependencyProperty XRangeProperty =
            DependencyProperty.Register("XRange", typeof(FloatRange), typeof(Plot), new PropertyMetadata(null));

        public IntRange? SelectedInterval
        {
            get { return (IntRange?)GetValue(SelectedIntervalProperty); }
            set { SetValue(SelectedIntervalProperty, value); }
        }

        public static readonly DependencyProperty SelectedIntervalProperty =
            DependencyProperty.Register("SelectedInterval", typeof(IntRange?), typeof(Plot), new PropertyMetadata(null, PropertyChanged));

        public float? SelectedMaxPeak
        {
            get { return (float?)GetValue(SelectedMaxPeakProperty); }
            set { SetValue(SelectedMaxPeakProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedMaxPeak.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedMaxPeakProperty =
            DependencyProperty.Register("SelectedMaxPeak", typeof(float?), typeof(Plot), new PropertyMetadata(null));

        public string? UnitX { get; set; }
        public string? UnitY { get; set; }

        public readonly List<LinesDefinition> VerticalLines = new();
        public readonly List<LinesDefinition> HorizontalLines = new();

        static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot && plot.DataContext is PlotData plotData)
            {
                plot.RefreshPlot();
                d.SetCurrentValue(MaxPeakProperty, plotData.Y.GetMaxPeak(plot.Interval.Start, plot.Interval.Length));
                d.SetCurrentValue(XRangeProperty, plot.GetXRange(plot.Interval));
                d.SetCurrentValue(SelectedMaxPeakProperty, plot.SelectedInterval == null
                    ? null
                    : plotData.Y.GetMaxPeak(plot.SelectedInterval.Value.Start, plot.SelectedInterval.Value.Length));
            }
        }

        public Plot()
        {
            InitializeComponent();
        }

        float? GetCurrentValue()
        {
            if(DataContext is PlotData plotData && 
                CurrentIndex != null &&
                CurrentIndex >= 0 &&
                CurrentIndex < plotData.Y.Length)
            {
                return plotData.Y[(int)CurrentIndex];
            }
            return null;
        }

        FloatRange GetXRange(IntRange interval)
        {
            if (DataContext is PlotData plotData)
            {
                float ratio1 = interval.Start/ (float)plotData.Y.Length;
                float ratio2 = (interval.Start+interval.Length) / (float)plotData.Y.Length;

                return new FloatRange(plotData.MinX * (1 - ratio1) + plotData.MaxX * ratio1,
                    plotData.MinX * (1 - ratio2) + plotData.MaxX * ratio2);

            }
            return new FloatRange(0,0);
        }

        private void image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshPlot();
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is PlotData plotData)
            {
                int indexFrom = Interval.Start;
                int indexTo = Interval.End;
                double range = indexTo - indexFrom;
                double pos = indexFrom + e.GetPosition(image).X / image.ActualWidth * range;

                range *= e.Delta > 0 ? 0.5 : 2;

                if (range < 10)
                    range = 10;

                indexFrom = (int)(pos - range / 2);
                if (indexFrom < 0)
                    indexFrom = 0;

                indexTo = indexFrom + (int)range;
                if (indexTo > plotData.Y.Length)
                    indexTo = plotData.Y.Length;

                Interval = new IntRange(indexFrom, indexTo - indexFrom);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is PlotData plotData)
            {
                Interval = new IntRange(0, plotData.Y.Length);
                SelectedInterval = null;
                MaxPeak = plotData.MaxPeak;
            }
            RefreshPlot();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPlot();
        }

        void RefreshPlot()
        {
            if (!IsLoaded || grid.ActualHeight == 0 || grid.ActualWidth == 0)
                return;

            WriteableBitmap writeableBitmap = image.Source is WriteableBitmap wb &&
                (int)grid.ActualHeight == wb.PixelHeight &&
                (int)grid.ActualWidth == wb.PixelWidth
                    ? wb
                    : new WriteableBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Bgr32, null);

            if (DataContext is PlotData plotData)
            {
                float xRangeFrom = Interval.Start /(float) plotData.Y.Length * (plotData.MaxX- plotData.MinX);
                float xRangeTo = Interval.End / (float)plotData.Y.Length * (plotData.MaxX - plotData.MinX);

                List<PlotLine> plotLines = writeableBitmap.PlotSignal(plotData.Y,
                    Interval.Start, Math.Min(Interval.Length, plotData.Y.Length- Interval.Start),
                    xRangeFrom, xRangeTo,
                    plotData.MinY, plotData.MaxY,
                    BackgroundColor, SignalColor, SelectedIntervalColor,
                    VerticalLines, HorizontalLines,
                    SelectedInterval);

                verticalLabels.Children.Clear();
                horizontalLabels.Children.Clear();
                string? unit;

                foreach (PlotLine plotLine in plotLines)
                {
                    unit = plotLine.Vertical ? UnitX : UnitY;

                    if (!string.IsNullOrEmpty(unit))
                    {
                        unit = (plotLine.Vertical ? " " : "\n") + unit;
                    }
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = plotLine.Value.ToString("0.###") +  unit,
                        TextAlignment= TextAlignment.Center,
                        VerticalAlignment= VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.Black)
                    };

                    if(!plotLine.Vertical) 
                    {
                        Canvas.SetRight(textBlock, 5);
                        Canvas.SetTop(textBlock, plotLine.Position-15);
                        verticalLabels.Children.Add(textBlock);
                    }
                    else
                    {
                        Canvas.SetLeft(textBlock, plotLine.Position-10);
                        Canvas.SetTop(textBlock, 3);
                        horizontalLabels.Children.Add(textBlock);
                    }
                }

                if (VerticalLines.Any())
                    horizontalLabels.Height = 30;
                if(HorizontalLines.Any())
                    verticalLabels.Width = 40;
            }
            else
            {
                writeableBitmap.PaintColor(BackgroundColor);
            }

            image.Source = writeableBitmap;
        }


        int? plotSelectSamplePressed;

        Point? plotMoveStartPoint;
        int? plotMoveSamplePressed;

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            double x = e.GetPosition(image).X;

             if (x < 0)
                x = 0;
            if (x > image.ActualWidth)
                x = image.ActualWidth;
            
            if(DataContext is PlotData plotData)
            {
                int? currentSample = (int)(x / image.ActualWidth * Interval.Length) + Interval.Start;
                if (currentSample < 0)
                    currentSample = 0;
                else if (currentSample > plotData.Y.Length)
                    currentSample = plotData.Y.Length;
                if (currentSample != CurrentIndex)
                    CurrentIndex = currentSample;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (plotSelectSamplePressed == null)
                        {
                            plotSelectSamplePressed = CurrentIndex;
                            SelectedInterval = new IntRange(plotSelectSamplePressed.Value, 0);
                        }
                        else if(SelectedInterval != null)
                        {
                            int start = Math.Min(plotSelectSamplePressed.Value, CurrentIndex.Value);
                            SelectedInterval = new IntRange(start,
                                 Math.Max(plotSelectSamplePressed.Value, CurrentIndex.Value) - start);
                        }
                    }
                    else
                    {
                        plotSelectSamplePressed = null;

                        if (plotMoveStartPoint == null)
                        {
                            plotMoveStartPoint = e.GetPosition(image);

                            if (plotMoveStartPoint.Value.X < 0 ||
                                plotMoveStartPoint.Value.Y < 0 ||
                                plotMoveStartPoint.Value.X > image.ActualWidth ||
                                plotMoveStartPoint.Value.Y > image.ActualHeight)
                                plotMoveStartPoint = null;

                            if (plotMoveStartPoint.HasValue)
                            {
                                plotMoveSamplePressed = Interval.Start;
                            }
                        }
                        else if (plotMoveSamplePressed != null)
                        {
                            double shift = (plotMoveStartPoint.Value.X - x) / image.ActualWidth * Interval.Length;
                            int diff = Interval.Length;
                            int sampleFrom = (int)(plotMoveSamplePressed.Value + shift);
                            if (sampleFrom < 0)
                                sampleFrom = 0;
                            int sampleTo = sampleFrom + diff;
                            if (sampleTo > plotData.Y.Length)
                            {
                                sampleTo = plotData.Y.Length;
                                sampleFrom = sampleTo - diff;
                            }
                            Interval = new IntRange(sampleFrom, sampleTo- sampleFrom);
                        }
                    }
                }
                else
                {
                    plotMoveStartPoint = null;
                    plotSelectSamplePressed = null;
                }
            }   
        }

        private void image_MouseLeave(object sender, MouseEventArgs e)
        {
            if(plotSelectSamplePressed != null && SelectedInterval != null)
            {
                double x = e.GetPosition(image).X;
                if (x < 0)
                    SelectedInterval = new IntRange(Interval.Start, SelectedInterval.Value.End);
                if (x > image.ActualWidth)
                    SelectedInterval = new IntRange(SelectedInterval.Value.Start, Interval.End);
            }
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                double x = e.GetPosition(image).X;

                if(x < 50)
                {
                    plotSelectSamplePressed = Interval.Start;
                    SelectedInterval = new IntRange(Interval.Start, Interval.Start);
                }
                else if(x > image.ActualWidth-50)
                {
                    plotSelectSamplePressed = Interval.End;
                    SelectedInterval = new IntRange(Interval.End, Interval.End);
                }
            }
        }

        private void image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedInterval = null;
        }
    }
}
