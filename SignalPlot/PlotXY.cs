using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SignalPlot
{
    public static class PlotXY
    {
        public static List<PlotLine> Plot(this WriteableBitmap writeableBitmap,
            float[] x, float[] y,
            FloatRange xRange,
            FloatRange yRange,
            int backgroundcolor, int color, int selectedColor,
            List<LinesDefinition> verticalLines,
            List<LinesDefinition> horizontalLines,
            FloatRange? selectedXRange)
        {
            List<PlotLine> result = new List<PlotLine>();

            try
            {
                writeableBitmap.Lock();

                writeableBitmap.PaintColor(backgroundcolor);

                if (writeableBitmap.Width < 2 || xRange.Length <= 0 || yRange.Length <= 0)
                    return result;

                int height = writeableBitmap.PixelHeight;
                double width = writeableBitmap.PixelWidth;

                // Paint selected interval
                if (selectedXRange != null && selectedXRange.Value.Length > 0)
                {
                    int posStart = (int)(width * (selectedXRange.Value.Start - xRange.Start) / xRange.Length);
                    int posEnd = (int)(width * (selectedXRange.Value.End - xRange.Start) / xRange.Length);
                    writeableBitmap.PaintVerticalSegment(selectedColor, posStart, posEnd - posStart);
                }

                HashSet<int> points = new(); // Keeps track of which points already have been painted

                foreach (var line in verticalLines)
                    result.AddRange(writeableBitmap.PaintVerticalLines(line, xRange, points));

                points.Clear();

                foreach (var line in horizontalLines)
                    result.AddRange(writeableBitmap.PaintHorizontalLines(line, yRange, points));

                // Paint data
                if (x.Length == y.Length)
                {
                    IntPtr pBackBuffer = writeableBitmap.BackBuffer;
                    int xpos, ypos;

                    unsafe
                    {
                        for (int i = 0; i < x.Length; i++)
                        {
                            xpos = (int)(width * (x[i] - xRange.Start) / xRange.Length);
                            ypos = (int)(height * (y[i] - yRange.Start) / yRange.Length);
                            if(xpos >= 0 && xpos < width && ypos >= 0 && ypos < height)
                            {
                                pBackBuffer = writeableBitmap.BackBuffer + 4 * xpos +
                                    writeableBitmap.BackBufferStride * ypos;
                                *((int*)pBackBuffer) = color;
                            }
                        }
                    }
                }
            }
            finally
            {
                writeableBitmap.Unlock();
            }

            return result;
        }
    }
}
