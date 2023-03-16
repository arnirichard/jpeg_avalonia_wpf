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

namespace JpegAvalonia
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

        public GridPlot()
        {
            InitializeComponent();

            grid.GetObservable(BoundsProperty).Subscribe(value =>
            {
                if (DataContext is int[] values)
                {
                    Redraw(values);
                }
            });

            DataContextChanged += GridPlot_DataContextChanged;
        }

        private void GridPlot_DataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is int[] values)
            {
                Redraw(values);
            }
        }

        void Redraw(int[] values)
        {
            if(grid.Bounds.Width == 0 || grid.Bounds.Height == 0) 
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();

            int width = (int)grid.Bounds.Width;
            int height = (int)grid.Bounds.Height;

            WriteableBitmap? wbitmap = CreateBitmap(values, width, height);

            if (values == DataContext && 
                wbitmap != null)
            {
                image.Source = wbitmap;
                NumRedraws++;
                TotRedrawsTicks += stopwatch.ElapsedTicks;
            }
        }

        WriteableBitmap? CreateBitmap(int[] values, int width, int height)
        {
            try
            {
                WriteableBitmap writeableBitmap = new WriteableBitmap(
                    new PixelSize(width, height),
                    new Vector(96, 96),
                    Avalonia.Platform.PixelFormat.Bgra8888,
                    Avalonia.Platform.AlphaFormat.Unpremul);

                int rows = (values.Length+ NumColumns-1) / NumColumns;
                double columnWidth = width / (double)NumColumns;
                double rowHeight = height / (double)rows;
                int value;
                uint colorValue;
                long display;
                int channelValue = (int)Channel;
                canvas.Children.Clear();                
                bool black;
                double fontSize = height / NumColumns * 0.3;
                int posX, posY;
                double x = 0, y = -rowHeight;

                double maxRange = Channel == ColorChannel.Range ? values.Max() : 255;
                double minRange = Channel == ColorChannel.Range ? Math.Min(0, values.Min()) : 0;

                for (int i = 0; i < values.Length; i++)
                {
                    if (i % NumColumns == 0)
                    {
                        y += rowHeight;
                        x = 0;
                    }

                    value = values[i];

                    if (Channel == ColorChannel.Range)
                    {
                        display = value;
                        colorValue = (uint)(((display - minRange) / (maxRange-minRange)) * 255);
                        black = colorValue > 127;
                        colorValue = colorValue << 16 | colorValue << 8 | colorValue | 0xff000000;
                        canvas.Children.Add(GetTextBlock(display+AddDisplayValue, x, y + 0.3 * rowHeight, black, fontSize, columnWidth));
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

                        canvas.Children.Add(GetTextBlock(display+ AddDisplayValue, x, y + 0.3 * rowHeight, GrayScale && display > 127, fontSize, columnWidth));
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
                    
                return writeableBitmap;
            }
            catch
            {
                return null;
            }
        }

        TextBlock GetTextBlock(long display, double left, double top, bool black, double fontSize, double width)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = display.ToString(),
                Width = width,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(black ? Colors.Black : Colors.White),
                FontSize = fontSize,
                Margin = new Thickness(0)
            };
            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, top);
            return textBlock;
        }
    }
}
