using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public static class YCbCrRgbBlocks
    {
        public static int[][] YuvToRgb(int[][][] yuvBlocks, Jfif jfif)
        {
            int[] yBlock, uBlock, vBlock, rgbBlock;
            int[][] rgbBlocks = new int[jfif.NumBlocks][];
            int cbcrPixelRow, cbcrPixelColumn, cbcrPixel, rgbBlocksIndex;

            for (int y = 0; y < jfif.BlocksHeight; y += jfif.VerticalSamplingFactor)
            {
                for (int x = 0; x < jfif.BlocksWidth; x += jfif.HorizontalSamplingFactor)
                {
                    uBlock = yuvBlocks[y * jfif.BlocksWidthWithPadding + x][1];
                    vBlock = yuvBlocks[y * jfif.BlocksWidthWithPadding + x][2];

                    for (int v = 0; v < jfif.VerticalSamplingFactor; v++)
                    {
                        for (int h = 0; h < jfif.HorizontalSamplingFactor; h++)
                        {
                            yBlock = yuvBlocks[(y + v) * jfif.BlocksWidthWithPadding + (x + h)][0];
                            rgbBlocksIndex = (y + v) * jfif.BlocksWidthWithPadding + (x + h);
                            if (rgbBlocksIndex < rgbBlocks.Length)
                            {
                                rgbBlocks[rgbBlocksIndex] = rgbBlock = new int[64];

                                for (int j = 0; j < rgbBlock.Length; j++)
                                {
                                    cbcrPixelRow = j / 8 / jfif.VerticalSamplingFactor + v * 4;
                                    cbcrPixelColumn = j % 8 / jfif.HorizontalSamplingFactor + h * 4;
                                    cbcrPixel = cbcrPixelRow * 8 + cbcrPixelColumn;
                                    rgbBlock[j] = YCbCrRgbColor.YCbCrToRgb(yBlock[j], uBlock[cbcrPixel], vBlock[cbcrPixel]);
                                }
                            }
                        }
                    }
                }
            }

            return rgbBlocks;
        }

        internal static int[][][] RgbBlocksToYuvBlocks(int[][] rgbBlocks, int width, int height)
        {
            int[][][] result = new int[rgbBlocks.Length][][];
            int blocksHeight = (height + 7) / 8;
            int blocksWidth = (width + 7) / 8;

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new int[3][];
            }

            int[] yBlock, uBlock, vBlock, rgbBlock, yuv;
            int rgbBlocksIndex;

            for (int y = 0; y < blocksHeight; y += 1)
            {
                rgbBlocksIndex = y * blocksWidth;

                for (int x = 0; x < blocksWidth; x += 1)
                {
                    rgbBlock = rgbBlocks[rgbBlocksIndex];

                    result[rgbBlocksIndex][0] = yBlock = new int[rgbBlock.Length];
                    result[rgbBlocksIndex][1] = uBlock = new int[rgbBlock.Length];
                    result[rgbBlocksIndex][2] = vBlock = new int[rgbBlock.Length];

                    for (int i = 0; i < rgbBlock.Length; i++)
                    {
                        yuv = YCbCrRgbColor.RgbToYuv(rgbBlock[i]);
                        yBlock[i] = yuv[0];
                        uBlock[i] = yuv[1];
                        vBlock[i] = yuv[2];
                    }

                    rgbBlocksIndex++;
                }
            }

            return result;
        }
    }
}
