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
        public static int SignalColor = Blue;
        public static int BackgroundColor = White;

        // Visible index range of Y
        public IntRange Interval
        {
            get { return (IntRange)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(IntRange), typeof(Plot), new PropertyMetadata(new IntRange()));

        /// <summary>
        /// Index of PlotData.Y where mouse was located last time
        /// </summary>
        public int? CurrentIndex
        {
            get { return (int?)GetValue(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrentIndexProperty =
            DependencyProperty.Register("CurrentIndex", typeof(int?), typeof(Plot), new PropertyMetadata(null));

        public float? CurrentX
        {
            get { return (float?)GetValue(CurrentXProperty); }
            set { SetValue(CurrentXProperty, value); }
        }

        public static readonly DependencyProperty CurrentXProperty =
            DependencyProperty.Register("CurrentX", typeof(float?), typeof(Plot), new PropertyMetadata(null, CurrentXChanged));

        // Y[CurrentIndex]
        public float? CurrentValue
        {
            get { return (float?)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register("CurrentValue", typeof(float?), typeof(Plot), new PropertyMetadata(null));

        // Abs peak of visible data
        public float AbsPeak
        {
            get { return (float)GetValue(AbsPeakProperty); }
            set { SetValue(AbsPeakProperty, value); }
        }

        public static readonly DependencyProperty AbsPeakProperty =
            DependencyProperty.Register("AbsPeak", typeof(float), typeof(Plot), new PropertyMetadata(0f));

        // Visible X-range
        public FloatRange XRange
        {
            get { return (FloatRange)GetValue(XRangeProperty); }
            set { SetValue(XRangeProperty, value); }
        }

        public static readonly DependencyProperty XRangeProperty =
            DependencyProperty.Register("XRange", typeof(FloatRange), typeof(Plot), new PropertyMetadata(new FloatRange(0, 1), PropertyChanged));

        public IntRange? SelectedInterval
        {
            get { return (IntRange?)GetValue(SelectedIntervalProperty); }
            set { SetValue(SelectedIntervalProperty, value); }
        }

        public static readonly DependencyProperty SelectedIntervalProperty =
            DependencyProperty.Register("SelectedInterval", typeof(IntRange?), typeof(Plot), new PropertyMetadata(null));

        public FloatRange? SelectedXRange
        {
            get { return (FloatRange?)GetValue(SelectedXRangeProperty); }
            set { SetValue(SelectedXRangeProperty, value); }
        }

        public static readonly DependencyProperty SelectedXRangeProperty =
            DependencyProperty.Register("SelectedXRange", typeof(FloatRange?), typeof(Plot), new PropertyMetadata(null, PropertyChanged));

        public float? SelectedAbsPeak
        {
            get { return (float?)GetValue(SelectedAbsPeakProperty); }
            set { SetValue(SelectedAbsPeakProperty, value); }
        }

        public static readonly DependencyProperty SelectedAbsPeakProperty =
            DependencyProperty.Register("SelectedAbsPeak", typeof(float?), typeof(Plot), new PropertyMetadata(null));

        public string? UnitX { get; set; }
        public string? UnitY { get; set; }

        public readonly List<LinesDefinition> VerticalLines = new();
        public readonly List<LinesDefinition> HorizontalLines = new();

        static void CurrentXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                d.SetCurrentValue(CurrentIndexProperty, plot.CurrentX != null
                    ? plot.GetIntRange(new FloatRange(plot.CurrentX.Value, plot.CurrentX.Value)).Start
                    : null);
                d.SetCurrentValue(CurrentValueProperty, plot.GetCurrentValue());
            }
        }

        static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                plot.UpdateValues();
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

        IntRange GetIntRange(FloatRange xRange)
        {
            if (DataContext is PlotData plotData && plotData.XRange.Length > 0)
            {
                if (plotData.X == null)
                {
                    float ratio1 = (xRange.Start - plotData.XRange.Start) / plotData.XRange.Length;
                    float ratio2 = (xRange.End - plotData.XRange.Start) / plotData.XRange.Length;
                    int start = (int)(plotData.Y.Length * ratio1);

                    return new IntRange(
                        start,
                        (int)(plotData.Y.Length * ratio2)-start);
                }
            }
            return new IntRange(0,0);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is PlotData plotData)
            {
                XRange = plotData.XRange;
                SelectedInterval = null;
                AbsPeak = plotData.AbsPeak;
                UpdateValues();
            }
        }

        void UpdateValues()
        {
            if (DataContext is PlotData plotData)
            {
                SetCurrentValue(AbsPeakProperty, plotData.Y.GetAbsPeak(Interval.Start, Interval.Length));
                SetCurrentValue(IntervalProperty, GetIntRange(XRange));
                SetCurrentValue(SelectedIntervalProperty, SelectedXRange != null ? GetIntRange(SelectedXRange.Value) : null);
                SetCurrentValue(SelectedAbsPeakProperty, SelectedInterval == null
                    ? null
                    : plotData.Y.GetAbsPeak(SelectedInterval.Value.Start, SelectedInterval.Value.Length));
                RefreshPlot();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPlot();
        }

        void RefreshPlot()
        {
            if (!IsLoaded || grid.ActualHeight == 0 || grid.ActualWidth == 0 || Interval.Length <= 0)
                return;

            WriteableBitmap writeableBitmap = image.Source is WriteableBitmap wb &&
                (int)grid.ActualHeight == wb.PixelHeight &&
                (int)grid.ActualWidth == wb.PixelWidth
                    ? wb
                    : new WriteableBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Bgr32, null);

            if (DataContext is PlotData plotData)
            {
                List<PlotLine> plotLines = writeableBitmap.PlotSignal(plotData.Y,
                    Interval.Start, Math.Min(Interval.Length, plotData.Y.Length- Interval.Start),
                    XRange,
                    plotData.YRange,
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

        private void image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshPlot();
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (XRange.Length > 0 && DataContext is PlotData plotData)
            {
                float center = (float)(XRange.Start + e.GetPosition(image).X / image.ActualWidth * XRange.Length);
                float range = XRange.Length * (e.Delta > 0 ? 0.5f : 2f);
                if (range < 0.002f)
                    range = 0.002f;
                float start = center - range / 2;
                float end = center + range / 2;
                if (start < plotData.XRange.Start)
                    start = plotData.XRange.Start;
                if (end > plotData.XRange.End)
                    end = plotData.XRange.End;

                XRange = new FloatRange(start , end);
            }

            plotMoveXPressed = null;
        }

        float? plotSelectXPressed;
        double? plotMoveXPressed;

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            double x = e.GetPosition(image).X;

             if (x < 0)
                x = 0;

            if (x > image.ActualWidth)
                x = image.ActualWidth;
            
            if(DataContext is PlotData plotData)
            {
                float currentX = (float)(x / image.ActualWidth * XRange.Length) + XRange.Start;
                if (currentX < 0)
                    currentX = 0;
                else if (currentX > plotData.Y.Length)
                    currentX = plotData.Y.Length;
                CurrentX = currentX;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (plotSelectXPressed == null)
                        {
                            plotSelectXPressed = currentX;
                            SelectedXRange = new FloatRange(currentX, currentX);
                        }
                        else if(SelectedXRange != null)
                        {
                            SelectedXRange = new FloatRange(
                                Math.Min(SelectedXRange.Value.Start, currentX),
                                Math.Max(SelectedXRange.Value.End, currentX));
                        }
                    }
                    else
                    {
                        plotSelectXPressed = null;

                        if (plotMoveXPressed == null)
                        {
                            plotMoveXPressed = x;
                        }
                        else if (plotMoveXPressed != null)
                        {
                            float shift = (float)((x - plotMoveXPressed.Value)/image.ActualWidth*XRange.Length);
                            float start = XRange.Start - shift;
                            if (start < 0)
                                start = 0;
                            float end = start + XRange.Length;
                            if (end > plotData.XRange.End)
                            {
                                start = end - Math.Min(end - plotData.XRange.End, plotData.XRange.Length);
                                end = plotData.XRange.End;
                            }
                            plotMoveXPressed = x;
                            XRange = new FloatRange(start, end);
                        }
                    }
                }
                else
                {
                    plotSelectXPressed = null;
                }
            }   
        }

        private void image_MouseLeave(object sender, MouseEventArgs e)
        {
            if(plotSelectXPressed != null && SelectedInterval != null)
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
                    plotSelectXPressed = Interval.Start;
                    SelectedXRange = new FloatRange(XRange.Start, XRange.Start);
                }
                else if(x > image.ActualWidth-50)
                {
                    plotSelectXPressed = Interval.End;
                    SelectedXRange = new FloatRange(XRange.End, XRange.End);
                }
            }
        }

        private void image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedXRange = null;
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            plotMoveXPressed = null;
            plotSelectXPressed = null;
        }
    }
}
