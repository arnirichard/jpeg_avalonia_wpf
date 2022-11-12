using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public static int Black = int.Parse("000000", System.Globalization.NumberStyles.HexNumber);
        public static int White = int.Parse("FFFFFF", System.Globalization.NumberStyles.HexNumber);
        public static int Beige = int.Parse("DDDDDD", System.Globalization.NumberStyles.HexNumber);
        public static int Blue = int.Parse("0026FF", System.Globalization.NumberStyles.HexNumber);
        
        public static int SelectedIntervalColor = int.Parse("AAFF0000", System.Globalization.NumberStyles.HexNumber);

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

        public int IndexFrom
        {
            get { return (int)GetValue(IndexFromProperty); }
            set { SetValue(IndexFromProperty, value); }
        }

        public static readonly DependencyProperty IndexFromProperty =
            DependencyProperty.Register("IndexFrom", typeof(int), typeof(Plot), new PropertyMetadata(0, PropertyChanged));

        public int IndexTo
        {
            get { return (int)GetValue(IndexToProperty); }
            set { SetValue(IndexToProperty, value); }
        }

        public static readonly DependencyProperty IndexToProperty =
            DependencyProperty.Register("IndexTo", typeof(int), typeof(Plot), new PropertyMetadata(0, PropertyChanged));

        public int? CurrentIndex
        {
            get { return (int?)GetValue(CurrentIndexProperty); }
            set { SetValue(CurrentIndexProperty, value); }
        }

        public static readonly DependencyProperty CurrentIndexProperty =
            DependencyProperty.Register("CurrentIndex", typeof(int?), typeof(Plot), new PropertyMetadata(null, CurrentIndexChanged));


        static void CurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                d.SetCurrentValue(CurrentValueProperty, plot.GetCurrentValue());
                d.SetCurrentValue(CurrentXProperty, plot.GetX(plot.CurrentIndex));
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

        public float? FromX
        {
            get { return (float?)GetValue(FromXProperty); }
            set { SetValue(FromXProperty, value); }
        }

        public static readonly DependencyProperty FromXProperty =
            DependencyProperty.Register("FromX", typeof(float?), typeof(Plot), new PropertyMetadata(0f));

        public float? ToX
        {
            get { return (float?)GetValue(ToXProperty); }
            set { SetValue(ToXProperty, value); }
        }

        public static readonly DependencyProperty ToXProperty =
            DependencyProperty.Register("ToX", typeof(float?), typeof(Plot), new PropertyMetadata(0f));

        public int? SelectedStartIndex
        {
            get { return (int?)GetValue(SelectedStartIndexProperty); }
            set { SetValue(SelectedStartIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedStartIndexProperty =
            DependencyProperty.Register("SelectedStartIndex", typeof(int?), typeof(Plot), new PropertyMetadata());

        public int? SelectedEndIndex
        {
            get { return (int?)GetValue(SelectedEndIndexProperty); }
            set { SetValue(SelectedEndIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedEndIndexProperty =
            DependencyProperty.Register("SelectedEndIndex", typeof(int?), typeof(Plot), new PropertyMetadata(PropertyChanged));

        public int? SelectedIntervalLength
        {
            get { return (int?)GetValue(SelectedIntervalLengthProperty); }
            set { SetValue(SelectedIntervalLengthProperty, value); }
        }

        public static readonly DependencyProperty SelectedIntervalLengthProperty =
            DependencyProperty.Register("SelectedIntervalLength", typeof(int?), typeof(Plot), new PropertyMetadata(null));

        public string? UnitX { get; set; }
        public string? UnitY { get; set; }

        public readonly List<LinesDefinition> VerticalLines = new();
        public readonly List<LinesDefinition> HorizontalLines = new();

        static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Plot plot)
            {
                plot.RefreshPlot();
                d.SetCurrentValue(FromXProperty, plot.GetX(plot.IndexFrom));
                d.SetCurrentValue(ToXProperty, plot.GetX(plot.IndexTo));
                d.SetCurrentValue(SelectedIntervalLengthProperty, plot.GetSelectedIntervalLength());
            }
        }

        int? GetSelectedIntervalLength()
        {
            if(SelectedStartIndex != null && SelectedEndIndex != null)
                return Math.Abs((int)SelectedEndIndex - (int)SelectedStartIndex);

            return null;
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

        float? GetX(int? index)
        {
            if (DataContext is PlotData plotData &&
                index != null &&
                index >= 0 &&
                index <= plotData.Y.Length)
            {
                float ratio = (int)index / (float)plotData.Y.Length;
                return plotData.MinX*(1-ratio)+plotData.MaxX*ratio;
            }
            return null;
        }

        private void image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshPlot();
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is PlotData plotData)
            {
                int indexFrom = IndexFrom;
                int indexTo = IndexTo;
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

                IndexFrom = indexFrom;
                IndexTo = indexTo;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is PlotData plotData)
            {
                IndexFrom = 0;
                IndexTo = plotData.Y.Length;
                SelectedEndIndex = null;
            }
            RefreshPlot();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPlot();
        }

        void RefreshPlot()
        {
            if (!IsLoaded)
                return;

            WriteableBitmap writeableBitmap = image.Source is WriteableBitmap wb &&
                (int)grid.ActualHeight == wb.PixelHeight &&
                (int)grid.ActualWidth == wb.PixelWidth
                    ? wb
                    : new WriteableBitmap((int)grid.ActualWidth, (int)grid.ActualHeight, 96, 96, PixelFormats.Bgr32, null);

            if (DataContext is PlotData plotData)
            {
                float xRangeFrom = IndexFrom /(float) plotData.Y.Length * (plotData.MaxX- plotData.MinX);
                float xRangeTo = IndexTo / (float)plotData.Y.Length * (plotData.MaxX - plotData.MinX);

                List<PlotLine> plotLines = writeableBitmap.PlotSignal(plotData.Y,
                    IndexFrom, Math.Min(IndexTo - IndexFrom, plotData.Y.Length-IndexFrom),
                    xRangeFrom, xRangeTo,
                    plotData.MinY, plotData.MaxY,
                    BackgroundColor, SignalColor, SelectedIntervalColor,
                    VerticalLines, HorizontalLines,
                    SelectedStartIndex, SelectedEndIndex);

                verticalLabels.Children.Clear();
                horizontalLabels.Children.Clear();
                string? unit;

                foreach (PlotLine plotLine in plotLines)
                {
                    unit = plotLine.Vertical ? UnitY : UnitX;

                    if (!string.IsNullOrEmpty(unit))
                    {
                        unit = (plotLine.Vertical ? "\n" : " ") + unit;
                    }
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = plotLine.Value.ToString("0.###") +  unit,
                        TextAlignment= TextAlignment.Center,
                        Foreground = new SolidColorBrush(Colors.Black)
                    };

                    if(plotLine.Vertical) 
                    {
                        Canvas.SetRight(textBlock, 5);
                        Canvas.SetTop(textBlock, plotLine.Position);
                        verticalLabels.Children.Add(textBlock);
                    }
                    else
                    {
                        Canvas.SetLeft(textBlock, plotLine.Position-10);
                        Canvas.SetTop(textBlock, 3);
                        horizontalLabels.Children.Add(textBlock);
                    }
                }
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
            
            if(DataContext is PlotData plotData)
            {
                int? currentSample = (int)(x / image.ActualWidth * (IndexTo - IndexFrom)) + IndexFrom;
                if (currentSample < 0 || currentSample >= plotData.Y.Length)
                    currentSample = null;
                if(currentSample != CurrentIndex)
                    CurrentIndex = currentSample;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (plotSelectSamplePressed == null)
                        {
                            plotSelectSamplePressed = CurrentIndex;
                            SelectedStartIndex = plotSelectSamplePressed;
                            SelectedEndIndex = plotSelectSamplePressed;
                        }
                        else
                        {
                            SelectedEndIndex = CurrentIndex;
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
                                plotMoveSamplePressed = IndexFrom;
                            }
                        }
                        else if (plotMoveSamplePressed != null)
                        {
                            double shift = (plotMoveStartPoint.Value.X - x) / image.ActualWidth * (IndexTo - IndexFrom);
                            int diff = IndexTo - IndexFrom;
                            int sampleFrom = (int)(plotMoveSamplePressed.Value + shift);
                            if (sampleFrom < 0)
                                sampleFrom = 0;
                            int sampleTo = sampleFrom + diff;
                            if (sampleTo > plotData.Y.Length)
                            {
                                sampleTo = plotData.Y.Length;
                                sampleFrom = sampleTo - diff;
                            }
                            IndexFrom = sampleFrom;
                            IndexTo = sampleTo;
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
    }
}
