using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Schema;

namespace SignalPlot
{
    public static class PlotExtensions
    {
        public static int LightBlue = int.Parse("AAFFFF", System.Globalization.NumberStyles.HexNumber);

        public static List<PlotLine> PlotSignal(this WriteableBitmap writeableBitmap,
            float[] values, 
            IntRange interval,
            FloatRange xRange,
            FloatRange yRange,
            int backgroundcolor, int color, int selectedColor,
            List<LinesDefinition> verticalLines,
            List<LinesDefinition> horizontalLines,
            IntRange? selectedInterval,
            List<IntRange>? gaps = null,
            ValueDistribution[]? distributions = null)
        {
            List<PlotLine> result = new List<PlotLine>();

            try
            {
                writeableBitmap.Lock();

                writeableBitmap.PaintColor(backgroundcolor);

                if (writeableBitmap.Width < 2 || interval.Length <= 0)
                    return result;

                if(gaps?.Count > 0)
                {
                    foreach(var gap in gaps)
                    {
                        int posStart = (int)(writeableBitmap.PixelWidth * ((gap.Start - interval.Start) / (double)interval.Length));
                        int posEnd = (int)(writeableBitmap.PixelWidth * ((gap.End - interval.Start) / (double)interval.Length));
                        writeableBitmap.PaintVerticalSegment(LightBlue, posStart, posEnd - posStart);
                    }
                }

                // Paint selected interval
                if (selectedInterval != null && selectedInterval.Value.Length > 0)
                {
                    int posStart =(int)(writeableBitmap.PixelWidth * ((selectedInterval.Value.Start - interval.Start) / (double)interval.Length));
                    int posEnd =(int)(writeableBitmap.PixelWidth * ((selectedInterval.Value.End- interval.Start) / (double)interval.Length));
                    writeableBitmap.PaintVerticalSegment(selectedColor, posStart, posEnd - posStart);
                }

                if (distributions != null)
                    writeableBitmap.PaintDistributions(distributions, interval, yRange);

                // Paint selected interval
                HashSet<int> points = new(); // Keeps track of which points already have been painted

                foreach(var line in verticalLines)
                    result.AddRange(writeableBitmap.PaintVerticalLines(line, xRange, points));

                points.Clear();

                foreach (var line in horizontalLines)
                    result.AddRange(writeableBitmap.PaintHorizontalLines(line, yRange, points));

                // Paint data
            
                int height = writeableBitmap.PixelHeight;
                double width = writeableBitmap.PixelWidth;
                double sample = interval.Start;
                double deltaSample = interval.Length / (writeableBitmap.Width - 1);

                IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                unsafe
                {
                    float yvalue;
                    int row;
                    float range = yRange.Length;
                    int sign;
                    yvalue = values[(int)sample];
                    int prevRow = (int)(height - height * (yvalue - yRange.Start) / range);
                    pBackBuffer += prevRow * writeableBitmap.BackBufferStride;

                    for (int i = 1; i < width; i++)
                    {
                        yvalue = values[(int)sample];
                        row = (int)(height - height * (yvalue - yRange.Start) / range);

                        if (row < 0 || row >= writeableBitmap.PixelHeight)
                        {
                            pBackBuffer += (row - prevRow) * writeableBitmap.BackBufferStride;
                            prevRow = row;
                        }
                        else if (prevRow < 0 || prevRow >= writeableBitmap.PixelHeight)
                        {
                            pBackBuffer += (row - prevRow) * writeableBitmap.BackBufferStride;
                            *((int*)pBackBuffer) = color;
                            prevRow = row;
                        }
                        else 
                        {
                            sign = Math.Sign(row - prevRow);
                            while (prevRow != row)
                            {
                                pBackBuffer += sign * writeableBitmap.BackBufferStride;
                                prevRow += sign;
                                if (prevRow > -1 && prevRow < writeableBitmap.PixelHeight)
                                    *(int*)pBackBuffer = color;
                            }
                            *((int*)pBackBuffer) = color;
                        }

                        pBackBuffer += 4;
                        sample = sample + deltaSample;
                    }
                }
            }
            finally
            {
                writeableBitmap.Unlock();
            }

            return result;
        }

        static void PaintDistributions(this WriteableBitmap writeableBitmap, ValueDistribution[] distributions,
            IntRange interval, FloatRange yRange)
        {
            ValueDistribution distribution;

            for (int i = interval.Start; i < interval.End; i++)
            {
                distribution = distributions[i];
                writeableBitmap.PaintDistribtution(distribution,
                    (i - interval.Start) * writeableBitmap.PixelWidth / interval.Length,
                    writeableBitmap.PixelWidth / interval.Length,
                    yRange,
                    Plot.Red);
            }
        }

        static void PaintDistribtution(this WriteableBitmap writeableBitmap, ValueDistribution distribution,
            int columnStart, int width, FloatRange yRange, int color)
        {
            IntPtr pBackBuffer;
            float start, end;
            int startRow, endRow;

            unsafe
            {
                byte aDelta = (byte)(127 / distribution.Quantiles.Length);
                byte a = 0;
                Color c = Color.FromArgb(color);
                int paintColor;
                int[] colors = new int[] { Plot.Red, Plot.Orange };
                int index = 0;
                foreach (var quantile in distribution.Quantiles)
                {
                    a += aDelta;
                    var co = Color.FromArgb(100, c);
                    paintColor = colors[index% colors.Length];
                    start = quantile.Start > yRange.Start ? quantile.Start : yRange.Start;
                    end = quantile.End < yRange.End ? quantile.End : yRange.End;

                    if (start >= end)
                        continue;

                    startRow = (int)((yRange.End - end) * writeableBitmap.Height / yRange.Length);
                    endRow = (int)((yRange.End - start) * writeableBitmap.Height / yRange.Length);

                    pBackBuffer =  writeableBitmap.BackBuffer + 4 * columnStart +
                        startRow * writeableBitmap.BackBufferStride;

                    for(int i = startRow; i < endRow; i++)
                    {
                        for(int j = 0; j < width; j++)
                        {
                            *(int*)pBackBuffer = paintColor;
                            pBackBuffer += 4;
                        }
                        pBackBuffer += 4 * (writeableBitmap.PixelWidth-width);
                    }
                    index++;
                    //break;
                }
            }
        }

        internal static List<PlotLine> PaintVerticalLines(this WriteableBitmap writeableBitmap, 
            LinesDefinition linesDefinition, FloatRange range, HashSet<int> points)
        {
            List<PlotLine> result = new();

            var spacing = writeableBitmap.PixelWidth *
                (linesDefinition.Interval > 0 ? linesDefinition.Interval : range.Length)
                / range.Length;

            if (range.Length > 0 && spacing >= linesDefinition.MinPointsSpacing)
            {
                float val = linesDefinition.Value + linesDefinition.Interval * (int)((linesDefinition.Value - range.Start) / linesDefinition.Interval);
                int pos;

                while (val < range.End)
                {
                    if (val > range.Start)
                    {
                        pos = (int)(writeableBitmap.PixelWidth * (val - range.Start) / range.Length);

                        if (!points.Contains(pos) &&
                            writeableBitmap.PaintVerticalLine(linesDefinition.Color,
                                pos,
                                linesDefinition.Solid ? writeableBitmap.PixelHeight : 8,
                                linesDefinition.Solid ? 0 : 4))
                        {
                            result.Add(new PlotLine(true, pos, val, linesDefinition.Solid));
                            points.Add(pos);
                        }
                    }

                    if (linesDefinition.Interval <= 0)
                        break;

                    val += linesDefinition.Interval;
                }
            }

            return result;
        }

        internal static List<PlotLine> PaintHorizontalLines(this WriteableBitmap writeableBitmap, 
            LinesDefinition linesDefinition, FloatRange range, HashSet<int> points)
        {
            List<PlotLine> result = new();

            var spacing = writeableBitmap.PixelHeight *
                (linesDefinition.Interval > 0 ? linesDefinition.Interval : range.Length)
                / range.Length;

            if (range.Length > 0 && spacing >= linesDefinition.MinPointsSpacing)
            {
                float val = linesDefinition.Value - linesDefinition.Interval * (int)((linesDefinition.Value - range.Start) / linesDefinition.Interval);
                int pos;

                while (val < range.End)
                {
                    if (val > range.Start)
                    {
                        pos = (int)(writeableBitmap.PixelHeight * (range.End - val) / range.Length);
                        if(!points.Contains(pos) &&
                            writeableBitmap.PaintHorizontalLine(linesDefinition.Color,
                            pos,
                            linesDefinition.Solid ? writeableBitmap.PixelWidth : 8,
                            linesDefinition.Solid ? 0 : 4))
                        {
                            result.Add(new PlotLine(false, pos, val, linesDefinition.Solid));
                            points.Add(pos);
                        }
                    }

                    val += linesDefinition.Interval <= 0 ? int.MaxValue : linesDefinition.Interval;
                }
            }

            return result;
        }

        internal static void PaintColor(this WriteableBitmap writeableBitmap, int color)
        {
            try
            {
                writeableBitmap.Lock();

                IntPtr pBackBuffer;

                unsafe
                {
                    pBackBuffer = writeableBitmap.BackBuffer;
                    int size = writeableBitmap.PixelWidth * writeableBitmap.PixelHeight;

                    for (int i = 0; i < size; i++)
                    {
                        *((int*)pBackBuffer) = color;
                        pBackBuffer += 4;
                    }
                    writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight));
                }
            }
            finally
            {
                writeableBitmap.Unlock();
            }
        }

        internal static bool PaintVerticalLine(this WriteableBitmap writeableBitmap, int color, int x,
            int solid = int.MaxValue, int gaps = 0)
        {
            if (x < 0 || x >= writeableBitmap.PixelWidth)
                return false;

            try
            {
                writeableBitmap.Lock();

                IntPtr pBackBuffer;

                unsafe
                {
                    pBackBuffer = writeableBitmap.BackBuffer + 4*x;
                    int height = writeableBitmap.PixelHeight;

                    int i = 0;
                    int iTo;

                    while (true)
                    {
                        iTo = Math.Min(i + solid, height);

                        for (; i < iTo; i++)
                        {
                            *((int*)pBackBuffer) = color;
                            pBackBuffer += writeableBitmap.BackBufferStride;
                        }

                        if (iTo < height && gaps > 0)
                        {
                            pBackBuffer += writeableBitmap.BackBufferStride * gaps;
                            i += gaps;
                        }

                        if (i >= height)
                            break;
                    }
                    writeableBitmap.AddDirtyRect(new Int32Rect(x, 0, 1, writeableBitmap.PixelHeight));
                }
            }
            finally
            {
                writeableBitmap.Unlock();
            }

            return true;
        }

        internal static bool PaintHorizontalLine(this WriteableBitmap writeableBitmap, int color, int y,
            int solid = int.MaxValue, int gaps = 0)
        {
            if (y < 0 || y >= writeableBitmap.PixelHeight)
                return false;

            try
            {
                writeableBitmap.Lock();

                IntPtr pBackBuffer;

                unsafe
                {
                    pBackBuffer = writeableBitmap.BackBuffer + writeableBitmap.BackBufferStride * y;
                    int width = writeableBitmap.PixelWidth;

                    int i = 0;
                    int iTo;

                    while (true)
                    {
                        iTo = Math.Min(i + solid, width);

                        for (; i < iTo; i++)
                        {
                            *((int*)pBackBuffer) = color;
                            pBackBuffer += 4;
                        }

                        if (iTo < width && gaps > 0)
                        {
                            pBackBuffer += 4 * gaps;
                            i += gaps;
                        }

                        if (i >= width)
                            break;
                    }

                    writeableBitmap.AddDirtyRect(new Int32Rect(0, y, writeableBitmap.PixelWidth, 1));
                }
            }
            finally
            {
                writeableBitmap.Unlock();
            }

            return true;
        }

        internal static bool PaintVerticalSegment(this WriteableBitmap writeableBitmap, int color, int fromX, int width)
        {
            int startX = fromX + (width < 0 ? width : 0);
            int endX = startX + Math.Abs(width);

            if (endX < 0 || startX >= writeableBitmap.PixelWidth)
                return false;

            if (startX < 0)
                startX = 0;
            if (endX > writeableBitmap.PixelWidth)
                endX = writeableBitmap.PixelWidth;

            int realWidth = endX - startX;

            if (realWidth <= 0)
                return false;

            try
            {
                writeableBitmap.Lock();

                IntPtr pBackBuffer;

                unsafe
                {
                    pBackBuffer = writeableBitmap.BackBuffer + 4 * startX;
                    int height = writeableBitmap.PixelHeight;

                    for(int j = 0; j < height; j++)
                    {
                        for(int i = 0; i < realWidth; i++)
                        {
                            *((int*)pBackBuffer) = color;
                            pBackBuffer += 4;
                        }

                        pBackBuffer += (writeableBitmap.PixelWidth- realWidth) *4;
                    }

                    writeableBitmap.AddDirtyRect(new Int32Rect(startX, 0, realWidth, writeableBitmap.PixelHeight));
                }
            }
            finally
            {
                writeableBitmap.Unlock();
            }

            return true;
        }
    }
}
