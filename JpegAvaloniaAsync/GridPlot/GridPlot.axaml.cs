using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DynamicData;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace JpegAvaloniaAsync
{
    public enum ColorChannel : int
    {
        None = -1,
        Red = 16,
        Green = 8,
        Blue = 0,
        Range = 1
    }

    public partial class GridPlot : UserControl
    {
        public static int Red = int.Parse("FFFF0000", System.Globalization.NumberStyles.HexNumber);
        public static int Orange = int.Parse("FFFF6A00", System.Globalization.NumberStyles.HexNumber);
        public static int Black = int.Parse("FF000000", System.Globalization.NumberStyles.HexNumber);
        public static int White = int.Parse("FFFFFFFF", System.Globalization.NumberStyles.HexNumber);
        public static int Beige = int.Parse("FFDDDDDD", System.Globalization.NumberStyles.HexNumber);
        public static int Blue = int.Parse("FF0000FF", System.Globalization.NumberStyles.HexNumber);

        public int NumColumns { get; set; }
        public ColorChannel Channel { get; set; } = ColorChannel.None;
        public bool GrayScale { get; set; }
        public int AddDisplayValue { get; set; }

        public static int NumRedraws;
        public static long TotRedrawsTicks;

        int[]? currentValues;
        int currentWidth, currentHeight;

        public GridPlot()
        {
            InitializeComponent();

            grid.GetObservable(BoundsProperty).Subscribe(value =>
            {
                if (DataContext is int[] values)
                {
                    _ = Redraw(values);
                }
            });

            DataContextChanged += GridPlot_DataContextChanged;
        }

        private void GridPlot_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is int[] values)
            {
                _= Redraw(values);
            }
        }

        async Task Redraw(int[] values)
        {
            if(grid.Bounds.Width == 0 || grid.Bounds.Height == 0) 
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();

            int width = currentWidth = (int)grid.Bounds.Width;
            int height = currentHeight = (int)grid.Bounds.Height;

            if(width == 0 || height == 0) 
                return;

            currentValues = values;

            WriteableBitmap? wbitmap = await CreateBitmap(values, width, height);

            if (values == DataContext && 
                wbitmap != null &&
                width == currentWidth &&
                height == currentHeight &&
                currentValues == values)
            {
                image.Source = wbitmap;
                NumRedraws++;
                TotRedrawsTicks += stopwatch.ElapsedTicks;
            }
        }

        async Task<WriteableBitmap?> CreateBitmap(int[] values, int width, int height)
        {
            TaskCompletionSource<WriteableBitmap?> taskCompletionSource =
                new TaskCompletionSource<WriteableBitmap?>();

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    WriteableBitmap writeableBitmap = new WriteableBitmap(
                        new PixelSize(width, height),
                        new Vector(96, 96),
                        Avalonia.Platform.PixelFormat.Bgra8888,
                        Avalonia.Platform.AlphaFormat.Unpremul);

                    int rows = (values.Length + NumColumns - 1) / NumColumns;
                    int columnWidth = width / NumColumns;
                    int rowHeight = height / rows;
                    int value;
                    uint colorValue;
                    long display;
                    int channelValue = (int)Channel;
                    bool black;
                    int posX, posY;
                    double x = 0, y = -rowHeight;

                    double maxRange = Channel == ColorChannel.Range ? values.Max() : 255;
                    double minRange = Channel == ColorChannel.Range ? Math.Min(0, values.Min()) : 0;

                    double fontSize = height / NumColumns * 0.3;

                    for (int i = 0; i < values.Length; i++)
                    {
                        if(values != currentValues)
                        {
                            taskCompletionSource.SetResult(null);
                            break;
                        }

                        if (i % NumColumns == 0)
                        {
                            y += rowHeight;
                            x = 0;
                        }

                        value = values[i];

                        if (Channel == ColorChannel.Range)
                        {
                            display = value < 0 ? -value : value;
                            colorValue = (uint)(((display - minRange) / (maxRange - minRange)) * 255);
                            black = colorValue > 127;
                            colorValue = colorValue << 16 | colorValue << 8 | colorValue | 0xff000000;
                        }
                        else if (Channel != ColorChannel.None)
                        {
                            display = (value & (255 << channelValue)) >> channelValue;

                            if (GrayScale)
                            {
                                colorValue = (uint)(display << 16 | display << 8 | display | 0xff000000);
                            }
                            else
                            {
                                colorValue = (uint)((display << channelValue) | 0xff000000);
                            }
                        }
                        else
                        {
                            colorValue = (uint)value;
                        }

                        posX = (int)x;
                        posY = (int)y;

                        writeableBitmap.PaintRect(colorValue,
                            posX % width,
                            posY % height,
                            (int)Math.Ceiling(x + columnWidth) - posX,
                            (int)Math.Ceiling(y + rowHeight) - posY);

                        x += columnWidth;
                    }

                    taskCompletionSource.SetResult(writeableBitmap);
                }
                catch
                {
                    taskCompletionSource.SetResult(null);
                }
            });

            return await taskCompletionSource.Task;
        }
    }
}
