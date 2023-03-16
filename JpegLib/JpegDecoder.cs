using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class JpegDecoder
    {
        public static async Task Decode(string jpegFileName, string bmpFileName)
        {
            JpegSegments jpegSegments = await JpegSegments.ReadJpeg(jpegFileName);

            Jfif jfif = Jfif.FromSegments(jpegSegments);
            
            int[][][] yuvBlocks = YuvBlocksJfif.JfifToYuvBlocks(jfif);

            var jfif2  = YuvBlocksJfif.YCbCrBlocksToJfif(yuvBlocks, jfif.Width, jfif.Height);

            int[][] rgbBlocks = YCbCrRgbBlocks.YuvToRgb(yuvBlocks, jfif);

            BMP.WriteBitmap(bmpFileName, new BmpData(jfif.Width, jfif.Height, rgbBlocks));
        }
    }

}
