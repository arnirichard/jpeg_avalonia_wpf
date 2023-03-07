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

        // Visible X-range
        public FloatRange XRange
        {
            get { return (FloatRange)GetValue(XRangeProperty); }
            set { SetValue(XRangeProperty, value); }
        }

        public static readonly DependencyProperty XRangeProperty =
            DependencyProperty.Register("XRange", typeof(FloatRange), typeof(Plot), new PropertyMetadata(new FloatRange(0, 1), XRangeChanged));

        public float? CurrentX
        {
            get { return (float?)GetValue(CurrentXProperty); }
            set { SetValue(CurrentXProperty, value); }
        }

        public static readonly DependencyProperty CurrentXProperty =
            DependencyProperty.Register("CurrentX", typeof(float?), typeof(Plot), new PropertyMetadata(null, CurrentXChanged));

        // Visible index range of Y
        public IntRange Interval
        {
            get { return (IntRange)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(IntRange), typeof(Plot), new PropertyMetadata(new IntRange()));

        public DataPoint? CurrentDataPoint
        {
            get { return (DataPoint?)GetValue(CurrentDataPointProperty); }
            set { SetValue(CurrentDataPointProperty, value); }
        }

        public static readonly DependencyProperty CurrentDataPointProperty =
            DependencyProperty.Register("CurrentDataPoint", typeof(DataPoint), typeof(Plot), new PropertyMetadata(null));

        // Abs peak of visible data
        public float AbsPeak
        {
            get { return (float)GetValue(AbsPeakProperty); }
            set { SetValue(AbsPeakProperty, value); }
        }

        public static readonly DependencyProperty AbsPeakProperty =
            DependencyProperty.Register("AbsPeak", typeof(float), typeof(Plot), new PropertyMetadata(0f));

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
            DependencyProperty.Register("SelectedXRange", typeof(FloatRange?), typeof(Plot), new PropertyMetadata(null, XRangeChanged));

        public float? SelectedAbsPeak
        {
            get { return (float?)GetValue(SelectedAbsPeakProperty); }
            set { SetValue(SelectedAbsPeakProperty, value); }
        }

        public static readonly DependencyProperty SelectedAbsPeakProperty =
            DependencyProperty.Register("SelectedAbsPeak", typeof(float?), typeof(Plot), new PropertyMetadata(null));

        public List<IntRange> Gaps
        {
            get { return (List<IntRange>)GetValue(GapsProperty); }
            set { SetValue(GapsProperty, value); }
        }

        public static readonly DependencyProperty GapsProperty =
            DependencyProperty.Register("Gaps", typeof(List<IntRange>), typeof(Plot), new PropertyMetadata(new List<IntRange>(), GapsChanged));



        public bool DoNotResetRange
        {
            get { return (bool)GetValue(DoNotResetRangeProperty); }
            set { SetValue(DoNotResetRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoNotResetRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DoNotResetRangeProperty =
            DependencyProperty.Register("DoNotResetRange", typeof(bool), typeof(Plot), new PropertyMetadata(false));



        public string? UnitX { get; set; }
        public string? UnitY { get; set; }
        public float? DefaultXEnd { get; set; }

        public readonly List<LinesDefinition> VerticalLines = new();
        public readonly List<LinesDefinition> HorizontalLines = new();

        float? plotSelectXPressed;
        double? plotMoveXPressed;

        public Plot()
        {
            InitializeComponent();
        }

        static void CurrentXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                d.SetCurrentValue(CurrentDataPointProperty, plot.GetCurrentDataPoint());
            }
        }

        static void XRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                plot.UpdateValues();
            }
        }

        static void GapsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                plot.RefreshPlot();
            }
        }

        DataPoint? GetCurrentDataPoint()
        {
            if(DataContext is PlotData plotData)
            {
                if (plotData.X == null &&
                    CurrentX != null)
                {
                    int? index = GetIntRange(new FloatRange(CurrentX.Value, CurrentX.Value)).Start;
                    if(index != null)
                        return new DataPoint(CurrentX.Value, plotData.Y[index.Value], index.Value, plotData.Data?[index.Value]);
                }
                else if(plotData.X != null &&
                    CurrentX != null)
                {
                    return plotData.GetDataPoint(CurrentX.Value, 0.01f);
                }
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
                    ratio1 = Math.Min(Math.Max(ratio1, 0), 1);
                    float ratio2 = (xRange.End - plotData.XRange.Start) / plotData.XRange.Length;
                    ratio2 = Math.Min(Math.Max(ratio2, 0), 1); 
                    int start = (int)(plotData.Y.Length * ratio1);
                    if (start >= plotData.Y.Length)
                        start = plotData.Y.Length - 1;

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
                if (!DoNotResetRange)
                {
                    XRange = DefaultXEnd != null && plotData.XRange.IsWithinRange(DefaultXEnd.Value)
                        ? new FloatRange(plotData.XRange.Start, DefaultXEnd.Value)
                        : plotData.XRange;
                    SelectedInterval = null;
                    SelectedXRange = null;
                }
                AbsPeak = plotData.AbsPeak;
                UpdateValues();
            }
        }

        void UpdateValues()
        {
            if (DataContext is PlotData plotData && plotData.Y.Length > 0)
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
                e.Handled = true;
            }

            plotMoveXPressed = null;
        }

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
                        //if (plotSelectXPressed == null)
                        //{
                        //    plotSelectXPressed = currentX;
                        //    SelectedXRange = new FloatRange(currentX, currentX);
                        //}
                        if(plotSelectXPressed != null && SelectedXRange != null)
                        {
                            SelectedXRange = new FloatRange(
                                Math.Min(plotSelectXPressed.Value, currentX),
                                Math.Max(plotSelectXPressed.Value, currentX));
                        }
                    }
                    else if(XRange.Length < plotData.XRange.Length)
                    {
                        plotSelectXPressed = null;

                        if (plotMoveXPressed == null)
                        {
                            plotMoveXPressed = x;
                        }
                        else if (plotMoveXPressed != null)
                        {
                            float shift = (float)((x - plotMoveXPressed.Value)/image.ActualWidth*XRange.Length);
                            if (shift == 0)
                                return;
                            float start = XRange.Start - shift;
                            if (start < 0)
                                start = 0;
                            
                            float end = start + XRange.Length;
                            if (end > plotData.XRange.End)
                            {
                                start -= plotData.XRange.End-end;
                                end = plotData.XRange.End;
                                if (start < 0)
                                    start = 0;
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

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double x = e.GetPosition(image).X;

            if (x < 0)
                x = 0;

            if (x > image.ActualWidth)
                x = image.ActualWidth;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && 
                DataContext is PlotData plotData)
            {
                float currentX = (float)(x / image.ActualWidth * XRange.Length) + XRange.Start;
                if (currentX < 0)
                    currentX = 0;
                else if (currentX > plotData.Y.Length)
                    currentX = plotData.Y.Length;

                if (plotSelectXPressed == null)
                {
                    plotSelectXPressed = currentX;
                    SelectedXRange = new FloatRange(currentX, currentX);
                }
                else if (SelectedXRange != null)
                {
                    SelectedXRange = new FloatRange(
                        Math.Min(plotSelectXPressed.Value, currentX),
                        Math.Max(plotSelectXPressed.Value, currentX));
                }
            }
        }

        private void image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedXRange = null;
            Gaps = new();
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            plotMoveXPressed = null;
            plotSelectXPressed = null;
        }

        void RefreshPlot()
        {
            if (!IsLoaded || grid.ActualHeight == 0 || grid.ActualWidth == 0 || XRange.Length <= 0)
                return;

            WriteableBitmap writeableBitmap = image.Source is WriteableBitmap wb &&
                (int)grid.ActualHeight == wb.PixelHeight &&
                (int)grid.ActualWidth == wb.PixelWidth
                    ? wb
                    : new WriteableBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Bgr32, null);

            if (DataContext is PlotData plotData)
            {
                List<PlotLine> plotLines = plotData.X == null
                    ? writeableBitmap.PlotSignal(plotData.Y,
                        Interval,
                        XRange, plotData.YRange,
                        BackgroundColor, SignalColor, SelectedIntervalColor,
                        VerticalLines, HorizontalLines,
                        SelectedInterval,
                        Gaps,
                        plotData.Distributions)
                    : writeableBitmap.Plot(plotData.X, plotData.Y, XRange, plotData.YRange,
                        BackgroundColor, SignalColor, SelectedIntervalColor,
                        VerticalLines, HorizontalLines, SelectedXRange);

                verticalLabels.Children.Clear();
                horizontalLabels.Children.Clear();
                string? unit;

                foreach (PlotLine plotLine in plotLines)
                {
                    unit = plotLine.Vertical ? UnitX : UnitY;

                    if (!string.IsNullOrEmpty(unit))
                    {
                        unit = (plotLine.Vertical ? "" : "\n") + unit;
                    }
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = plotLine.Value.ToString("0.###") + unit,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.Black)
                    };

                    if (!plotLine.Vertical)
                    {
                        Canvas.SetRight(textBlock, 5);
                        Canvas.SetTop(textBlock, plotLine.Position - 15);
                        verticalLabels.Children.Add(textBlock);
                    }
                    else
                    {
                        Canvas.SetLeft(textBlock, plotLine.Position-2);
                        Canvas.SetTop(textBlock, 3);
                        horizontalLabels.Children.Add(textBlock);
                    }
                }

                if (VerticalLines.Any())
                    horizontalLabels.Height = 30;
                if (HorizontalLines.Any())
                    verticalLabels.Width = 40;
            }
            else
            {
                writeableBitmap.PaintColor(BackgroundColor);
            }

            image.Source = writeableBitmap;
        }
    }
}
