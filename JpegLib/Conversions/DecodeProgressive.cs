using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class ProgressiveStep
    {
        public StartOfScan Scan { get; set; }
        public BmpData Bmp { get; set; }

        public ProgressiveStep(StartOfScan scan, BmpData bmp)
        {
            Scan = scan;
            Bmp = bmp;
        }
    }

    public static class DecodeProgressive
    {
        public static async Task<List<ProgressiveStep>> GetProgresiveSteps(string jpegFileName)
        {
            List<ProgressiveStep> result = new();

            List<JpegSegment> jpegSegments = await JpegSegments.ReadJpeg(jpegFileName);

            Jfif jfif = Jfif.FromSegments(jpegSegments);

            int[][][] yCbCrBlocks = new int[jfif.Header.NumBlocksWithPadding][][];
            for (int i = 0; i < yCbCrBlocks.Length; i++)
                yCbCrBlocks[i] = new int[jfif.Header.NumberOfComponents][];

            int[][][] yCbCrBlocksStep;

            foreach (var segment in jfif.Segments)
            {
                if (segment is StartOfScan s)
                {
                    YCbCrBlocksJfif.JfifToYCbCrBlocks(jfif, s, yCbCrBlocks);

                    if (jfif.Header.IsProgessive)
                    {
                        yCbCrBlocksStep = new int[jfif.Header.NumBlocksWithPadding][][];
                        for (int i = 0; i < yCbCrBlocksStep.Length; i++)
                            yCbCrBlocksStep[i] = new int[jfif.Header.NumberOfComponents][];

                        for (int i = 0; i < yCbCrBlocks.Length; i++)
                        {
                            for (int c = 0; c < jfif.Header.NumberOfComponents; c++)
                            {
                                if (yCbCrBlocks[i][c] != null)
                                    yCbCrBlocksStep[i][c] = DCT.InverseFast(Quant.Dequantize(
                                        yCbCrBlocks[i][c], 
                                        jfif.QuantizationTables[jfif.Header.Components[c].QuantizationTableIndex].Table));
                            }
                        }
                    }
                    else
                    {
                        yCbCrBlocksStep = yCbCrBlocks;
                    }

                    int[][] rgbBlocks = YCbCrRgbBlocks.YCbCrToRgb(yCbCrBlocksStep, jfif.Header);

                     result.Add(new ProgressiveStep(s, new BmpData(jfif.Header.Width, jfif.Header.Height, rgbBlocks)));
                }
                else if (segment is HufCodec c)
                {
                    jfif.SetHufCodec(c);
                }
            }

            return result;
        }
    }
}
