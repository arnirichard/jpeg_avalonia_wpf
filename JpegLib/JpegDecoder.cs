using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
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

            int[][][] yCbCrBlocks = new int[jfif.Header.NumBlocksWithPadding][][];
            for (int i = 0; i < yCbCrBlocks.Length; i++)
                yCbCrBlocks[i] = new int[jfif.Header.NumberOfComponents][];

            foreach (var segment in jfif.Segments)
            {
                if (segment is StartOfScan s)
                {
                    YCbCrBlocksJfif.JfifToYCbCrBlocks(jfif, s, yCbCrBlocks);
                }
                else if (segment is HufCodec c)
                {
                    jfif.SetHufCodec(c);
                }
            }            

            if(jfif.Header.IsProgessive)
            {
                for(int i = 0; i < yCbCrBlocks.Length; i++)
                {
                    for (int c = 0; c < jfif.Header.NumberOfComponents; c++)
                    {
                        if (yCbCrBlocks[i][c] != null)
                            yCbCrBlocks[i][c] = DCT.InverseFast(Quant.Dequantize(
                                yCbCrBlocks[i][c], 
                                jfif.QuantizationTables[jfif.Header.Components[c].QuantizationTableIndex].Table));
                    }
                }
            }

            int[][] rgbBlocks = YCbCrRgbBlocks.YCbCrToRgb(yCbCrBlocks, jfif.Header);

            BMP.WriteBitmap(bmpFileName, new BmpData(jfif.Header.Width, jfif.Header.Height, rgbBlocks));
        }
    }

}
