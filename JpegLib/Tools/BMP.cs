using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace JpegLib
{
    public class BmpData
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int[][] RgbBlocks;
        public BmpData(int width, int height, int[][] rgbBlocks)
        {
            Width = width;
            Height = height;
            RgbBlocks = rgbBlocks;
        }
    }

    public static class BMP
    {
        public static void WriteBitmap(string fileName, BmpData bmpData) 
        {
            int[][] rgbBlocks = bmpData.RgbBlocks;
            int width = bmpData.Width;
            int height = bmpData.Height;
            int paddingSize = width % 4;
            int size = 14 + 12 + height * width * 3 + paddingSize * height;
            int blocksWidth = (width+7) / 8;

            using (FileStream sw = File.OpenWrite(fileName))
            {
                sw.WriteByte((byte)'B');
                sw.WriteByte((byte)'M');
                sw.Write(BitConverter.GetBytes(size));
                sw.Write(BitConverter.GetBytes(0));
                sw.Write(BitConverter.GetBytes(26)); // data offset
                sw.Write(BitConverter.GetBytes(12)); // header size
                sw.Write(BitConverter.GetBytes((short)width));
                sw.Write(BitConverter.GetBytes((short)height));
                sw.Write(BitConverter.GetBytes((short)1)); // number of planes
                sw.Write(BitConverter.GetBytes((short)24)); // bits per pixel

                for (int y = height - 1; y > -1; --y)
                {
                    int blockRow = y / 8;
                    int pixelRow = y % 8;

                    for (int x = 0; x < width; ++x)
                    {
                        int blockColumn = x / 8;
                        int pixelColumn = x % 8;
                        int blockIndex = blockRow * blocksWidth + blockColumn;
                        int pixelIndex = pixelRow * 8 + pixelColumn;

                        sw.WriteByte((byte)(rgbBlocks[blockIndex][pixelIndex] & 0xff));
                        sw.WriteByte((byte)((rgbBlocks[blockIndex][pixelIndex] & 0xff00) >> 8));
                        sw.WriteByte((byte)((rgbBlocks[blockIndex][pixelIndex] & 0xff0000) >> 16));
                    }
                    for (uint i = 0; i < paddingSize; ++i)
                    {
                        sw.WriteByte(0);
                    }
                }
            }
        }

        public static BmpData ReadBitmap(string fileName)
        {
            int width, height;
            int[][] rgbBlocks;
            using (var fs = File.OpenRead(fileName))
            {
                // This code does not work on all formats
                fs.Position = 10;
                int dataOffset = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                int headerSize = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                width = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                height = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                short planes = (short)(fs.ReadByte() | fs.ReadByte() << 8);
                short bitsPerPixel = (short)(fs.ReadByte() | fs.ReadByte() << 8);
                int compression = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                int imageSize = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                int xpixelsPerM = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                int ypixelsPerM = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                int colorsUsed = fs.ReadByte() | fs.ReadByte() << 8 | fs.ReadByte() << 16 | fs.ReadByte() << 24;
                fs.Position = dataOffset;
                int blocksHeight = (height + 7) / 8;
                int blocksWidth = (width+ 7) / 8;
                rgbBlocks = new int[blocksHeight*blocksWidth][];
                int paddingSize = width % 4;

                for (int i = 0; i < rgbBlocks.Length; i++)
                    rgbBlocks[i] = new int[64];

                for (int y = height - 1; y > -1; --y)
                {
                    int blockRow = y / 8;
                    int pixelRow = y % 8;

                    for (int x = 0; x < width; ++x)
                    {
                        int blockColumn = x / 8;
                        int pixelColumn = x % 8;
                        int blockIndex = blockRow * blocksWidth + blockColumn;
                        int pixelIndex = pixelRow * 8 + pixelColumn;

                        rgbBlocks[blockIndex][pixelIndex] = fs.ReadByte() << 16 | fs.ReadByte() << 8 | fs.ReadByte();
                    }
                    for (uint i = 0; i < paddingSize; ++i)
                    {
                        fs.Position++;
                    }
                }
            }

            return new BmpData(width, height, rgbBlocks);
        }
    }
}
