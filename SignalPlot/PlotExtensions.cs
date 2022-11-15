using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace SignalPlot
{
    public class PlotLine
    {
        public bool Vertical { get; private set; }
        public int Position { get; private set; }
        public float Value { get; private set; }
        public bool Solid { get; private set; }

        public PlotLine(bool vertical, int position, float value, bool solid)
        {
            Vertical = vertical;
            Position = position;
            Value = value;
            Solid = solid;
        }
    }

    public static class PlotExtensions
    {
        public static List<PlotLine> PlotSignal(this WriteableBitmap writeableBitmap,
            float[] values, 
            int indexFrom, int length,
            float xRangeFrom, float xRangeTo,
            float valueRangeFrom, float valueRangeTo,
            int backgroundcolor, int color, int selectedColor,
            List<LinesDefinition> verticalLines,
            List<LinesDefinition> horizontalLines,
            IntRange? selectedInterval)
        {
            List<PlotLine> result = new List<PlotLine>();

            writeableBitmap.PaintColor(backgroundcolor);

            if(selectedInterval != null && selectedInterval.Value.Length > 0)
            {
                int posStart = (int)(writeableBitmap.PixelWidth * (selectedInterval.Value.Start - indexFrom) / length);
                int posEnd = (int)(writeableBitmap.PixelWidth * (selectedInterval.Value.End- indexFrom)/ length);
                writeableBitmap.PaintVerticalSegment(selectedColor, posStart, posEnd - posStart);
            }

            if (writeableBitmap.Width < 2 || length <= 0)
                return result;

            HashSet<int> points = new();

            foreach(var line in verticalLines)
                result.AddRange(writeableBitmap.PaintVerticalLines(line, xRangeFrom, xRangeTo, points));

            foreach (var line in horizontalLines)
                result.AddRange(writeableBitmap.PaintHorizontalLines(line, valueRangeFrom, valueRangeTo, points));

            try
            {
                int height = (int)writeableBitmap.Height;
                double width = (int)writeableBitmap.Width;
                double sample = indexFrom;
                writeableBitmap.Lock();
                double deltaSample = (length - 1) / (writeableBitmap.Width - 1);

                IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                unsafe
                {
                    float yvalue;
                    int row;
                    float range = valueRangeTo - valueRangeFrom;
                    int sign;
                    yvalue = values[(int)sample];
                    int prevRow = (int)(height - height * (yvalue - valueRangeFrom) / range);
                    pBackBuffer += prevRow * writeableBitmap.BackBufferStride;

                    for (int i = 1; i < width; i++)
                    {
                        yvalue = values[(int)sample];
                        row = (int)(height - height * (yvalue - valueRangeFrom) / range);

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
                        else //if (row != prevRow)
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

        public static List<PlotLine> PaintVerticalLines(this WriteableBitmap writeableBitmap, 
            LinesDefinition linesDefinition, float rangeFrom, float rangeTo, HashSet<int> points)
        {
            List<PlotLine> result = new();

            var spacing = writeableBitmap.PixelWidth *
                (linesDefinition.Interval > 0 ? linesDefinition.Interval : rangeTo-rangeFrom)
                / (rangeTo - rangeFrom);

            if (rangeTo > rangeFrom && spacing >= linesDefinition.MinPointsSpacing)
            {
                float val = linesDefinition.Value + linesDefinition.Interval * (int)((linesDefinition.Value - rangeFrom) / linesDefinition.Interval);
                int pos;

                while (val < rangeTo)
                {
                    if (val > rangeFrom)
                    {
                        pos = (int)(writeableBitmap.PixelWidth * (val - rangeFrom) / (rangeTo - rangeFrom));

                        if (!points.Contains(pos) &&
                            writeableBitmap.PaintVerticalLine(linesDefinition.Color,
                                pos,
                                linesDefinition.Solid ? writeableBitmap.PixelHeight : 8,
                                linesDefinition.Solid ? 0 : 4))
                        {
                            result.Add(new PlotLine(false, pos, val, linesDefinition.Solid));
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

        public static List<PlotLine> PaintHorizontalLines(this WriteableBitmap writeableBitmap, 
            LinesDefinition linesDefinition, float rangeFrom, float rangeTo, HashSet<int> points)
        {
            List<PlotLine> result = new();

            var spacing = writeableBitmap.PixelHeight *
                (linesDefinition.Interval > 0 ? linesDefinition.Interval : rangeTo - rangeFrom)
                / (rangeTo - rangeFrom);

            if (rangeTo > rangeFrom && spacing >= linesDefinition.MinPointsSpacing)
            {
                float val = linesDefinition.Value - linesDefinition.Interval * (int)((linesDefinition.Value - rangeFrom) / linesDefinition.Interval);
                int pos;

                while (val < rangeTo)
                {
                    if (val > rangeFrom)
                    {
                        pos = (int)(writeableBitmap.PixelHeight * (rangeTo - val) / (rangeTo - rangeFrom));
                        if(!points.Contains(pos) &&
                            writeableBitmap.PaintHorizontalLine(linesDefinition.Color,
                            pos,
                            linesDefinition.Solid ? writeableBitmap.PixelWidth : 8,
                            linesDefinition.Solid ? 0 : 4))
                        {
                            result.Add(new PlotLine(true, pos, val, linesDefinition.Solid));
                            points.Add(pos);
                        }
                    }

                    val += linesDefinition.Interval <= 0 ? int.MaxValue : linesDefinition.Interval;
                }
            }

            return result;
        }

        public static void PaintColor(this WriteableBitmap writeableBitmap, int color)
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

        public static bool PaintVerticalLine(this WriteableBitmap writeableBitmap, int color, int x,
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

        public static bool PaintHorizontalLine(this WriteableBitmap writeableBitmap, int color, int y,
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

        public static bool PaintVerticalSegment(this WriteableBitmap writeableBitmap, int color, int fromX, int width)
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
