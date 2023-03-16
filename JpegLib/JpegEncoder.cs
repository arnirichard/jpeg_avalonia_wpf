using JpegLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class JpegEncoder
    {
        public static void Encode(string bmpFileName, string jpgFileName)
        {
            BmpData bmpData = BMP.ReadBitmap(bmpFileName);

            int[][][] yuvBlocks = YCbCrRgbBlocks.RgbBlocksToYuvBlocks(bmpData.RgbBlocks, bmpData.Width, bmpData.Height);

            Jfif jfif = YuvBlocksJfif.YCbCrBlocksToJfif(yuvBlocks, bmpData.Width, bmpData.Height);

            JpegSegments jpegSegments = jfif.ToSegments();

            jpegSegments.WriteJpeg(jpgFileName);
        }
    }
}
