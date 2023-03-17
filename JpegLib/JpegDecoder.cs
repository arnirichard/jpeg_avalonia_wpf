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
            List<JpegSegment> jpegSegments = await JpegSegments.ReadJpeg(jpegFileName);

            Jfif jfif = Jfif.FromSegments(jpegSegments);
            int counter = 0;

            foreach (var segment in jfif.Segments)
            {
                if (segment is StartOfScan s)
                {
                    counter++;
                    YuvBlocksJfif.JfifToYuvBlocks(jfif, s);
                }
                else if (segment is HufCodec c)
                {
                    jfif.SetHufCodec(c);
                }
            }            

            if(jfif.Header.IsProgessive)
            {
                for(int i = 0; i < jfif.YCbCrBlocks.Length; i++)
                {
                    for (int c = 0; c < jfif.Header.NumberOfComponents; c++)
                    {
                        if (jfif.YCbCrBlocks[i][c] != null)
                            jfif.YCbCrBlocks[i][c] = Quant.Dequantize(jfif.YCbCrBlocks[i][c], jfif.QuantizationTables[jfif.Header.Components[c].QuantizationTableIndex].Table);
                    }
                }

                for (int i = 0; i < jfif.YCbCrBlocks.Length; i++)
                {
                    for (int c = 0; c < jfif.Header.NumberOfComponents; c++)
                    {
                        if (jfif.YCbCrBlocks[i][c] != null)
                            jfif.YCbCrBlocks[i][c] = DCT.InverseFast(jfif.YCbCrBlocks[i][c]);
                    }
                }
            }

            int[][] rgbBlocks = YCbCrRgbBlocks.YCbCrToRgb(jfif.YCbCrBlocks, jfif.Header);

            BMP.WriteBitmap(bmpFileName, new BmpData(jfif.Header.Width, jfif.Header.Height, rgbBlocks));
        }
    }

}
