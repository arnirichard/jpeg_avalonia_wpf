using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

namespace JpegWpf
{
    public enum ColorChannel : int
    {
        None = -1,
        Red = 16,
        Green = 8,
        Blue = 0,
        Abs = 1
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

        public static int NumRedraws;
        public static long TotRedrawsTicks;

        public GridPlot()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is int[] values)
            {
                Redraw(values);
            }
        }

        private void grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is int[] values)
            {
                Redraw(values);
            }
        }

        void Redraw(int[] values)
        {
            if (grid.ActualWidth == 0 || grid.ActualHeight == 0)
                return;

            Stopwatch stopwatch = Stopwatch.StartNew();

            int width = (int)grid.ActualWidth;
            int height = (int)grid.ActualHeight;

            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height,
                96, 96, PixelFormats.Bgr32, null);          

            int rows = values.Length / NumColumns + ((values.Length % NumColumns) > 0 ? 1 : 0);
            double columnWidth = width / (double)NumColumns;
            double rowHeight = height / (double)rows;
            int value;
            int colorValue;
            long display = 0;
            int channelValue = (int)Channel;
            canvas.Children.Clear();
            double maxAbs = Channel == ColorChannel.Abs ? values.Select(v => Math.Abs(v)).Max() : 255;
            bool black = false;
            if (maxAbs == 0)
                maxAbs = 1;
            double x = 0, y = -rowHeight;
            int posX, posY;
            writeableBitmap.Lock();

            for (int i = 0; i < values.Length; i++)
            {
                if (i % NumColumns == 0)
                {
                    y += rowHeight;
                    x = 0;
                }

                value = values[i];

                if (Channel == ColorChannel.Abs)
                {
                    display = value < 0 ? -value : value;
                    colorValue = (int)((display / maxAbs) * 255);
                    black = colorValue > 127;
                    colorValue = (int)(colorValue << 16 | colorValue << 8 | colorValue | 0xff000000);
                    canvas.Children.Add(GetTextBlock(display, x, y + 0.2 * rowHeight, black, columnWidth));
                }
                else if (Channel != ColorChannel.None)
                {
                    display = (value & (255 << channelValue)) >> channelValue;

                    if (GrayScale)
                    {
                        colorValue = (int)(display << 16 | display << 8 | display | 0xff000000);
                    }
                    else
                    {
                        colorValue = (int)((display << channelValue) | 0xff000000);
                    }

                    black = GrayScale && display > 127;
                    canvas.Children.Add(GetTextBlock(display, x, y + 0.2 * rowHeight, GrayScale && display > 127, columnWidth));
                }
                else
                {
                    colorValue = value;
                }

                posX = (int)x;
                posY = (int)y;

                if (colorValue != Black)
                    writeableBitmap.PaintRect(colorValue,
                        posX % writeableBitmap.PixelWidth,
                        posY % writeableBitmap.PixelHeight,
                        (int)(x + columnWidth) - posX,
                        (int)(y + rowHeight) - posY);

                x += (int)(x + columnWidth) - posX;
            }
            
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            writeableBitmap.Unlock();

            image.Source = writeableBitmap;
            NumRedraws++;
            TotRedrawsTicks += stopwatch.ElapsedTicks;
        }

        TextBlock GetTextBlock(long display, double left, double top, bool black, double width)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = display.ToString(),
                Width = width,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(black ? Colors.Black : Colors.White),
                FontSize = 10,
                Margin = new Thickness(0)
            };
            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, top);
            return textBlock;
        }
    }
}
