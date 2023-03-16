using JpegLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class Jfif
    {
        public readonly int Width, Height;
        public readonly int BlocksWidth, BlocksHeight;
        public readonly int BlocksWidthWithPadding, BlocksHeightWithPadding;
        public readonly int[][] QuantizationTables;
        public readonly HufCodec[] HuffmanTablesAc;
        public readonly HufCodec[] HuffmanTablesDc;
        public readonly int NumComponents;
        public readonly int ComponentsFirstIndex;
        public readonly ColorComponent[] Components;
        public readonly byte[] ScanData;

        // This class also servers as bit reader for ScanData
        // which should be encapsulated
        int lastBit = -1;
        byte currentByte;
        public int NumBlocks => BlocksHeight * BlocksWidth;
        public int NumBlocksWithPadding => BlocksHeightWithPadding * BlocksWidthWithPadding;
        public int VerticalSamplingFactor;
        public int HorizontalSamplingFactor;

        public Jfif(int width, int height, int[][] quantz,
            HufCodec[] hac, HufCodec[] hdc,
            int numComponents, ColorComponent[] components,
            byte[] data)
        {
            Width = width;
            Height = height;
            QuantizationTables = quantz;
            HuffmanTablesAc = hac;
            HuffmanTablesDc = hdc;
            NumComponents = numComponents;
            ComponentsFirstIndex = components[0] == null ? 1 : 0;
            Components = components;
            ScanData = data;
            BlocksWidth = (width + 7) / 8;
            VerticalSamplingFactor = components.Where(c => c != null).Max(v => v.SampFactorV);
            HorizontalSamplingFactor = components.Where(c => c != null).Max(v => v.SampFactorH);
            BlocksWidthWithPadding = BlocksWidth + (BlocksWidth % HorizontalSamplingFactor);
            BlocksHeight = (height + 7) / 8;
            BlocksHeightWithPadding = BlocksHeight + (BlocksHeight % VerticalSamplingFactor);
        }

        public int ScanBit()
        {
            int bit = ++lastBit;

            if(bit % 8 == 0)
            {
                if (bit / 8 >= ScanData.Length)
                {
                    return -1;
                }
                currentByte = ScanData[bit / 8];
                }

            int result = (currentByte & (1 << (8 - bit % 8 - 1))) > 0 ? 1 : 0;

            // 0xFF is encoded in the bitstream as 0xFF00
            if (currentByte == 255 && bit % 8 == 7)
            {
                lastBit += 8;
            }

            return result;
        }

        public int ScanInt(int length)
        {
            int result = 0;
            int b;

            for (int i = 0; i < length; i++)
            {
                b = ScanBit();
                result <<= 1;
                result += b;
            }

            return result;
        }

        internal static Jfif FromSegments(JpegSegments segments)
        {
            // JpegMarker.StartOfFrameBaseline
            ArraySegment<byte> frameBaseline = segments.Segments[JpegMarker.StartOfFrame0][0];
            // byte 2 (precision) must be 8
            int height = (frameBaseline[3] << 8) + frameBaseline[4];
            int width = (frameBaseline[5] << 8) + frameBaseline[6];
            // number of components is 1 or 3, 1 for grayscale  (only luminance)
            // 4 components is CMYK (another color mode)
            int numComponents = frameBaseline[7] < 4 ? frameBaseline[7] : 4;
            ColorComponent[] components = new ColorComponent[numComponents+1];
            int index = 8;

            for (int i = 0; i < numComponents; i++)
            {
                components[frameBaseline[index]] = new ColorComponent(
                    frameBaseline[index], // is 0-3
                    Math.Max(1, frameBaseline[index + 1] & 0x0f),
                    Math.Max(1, (frameBaseline[index + 1] >> 4) & 0x0f),
                    frameBaseline[index + 2] & 0x0f
                );
                index += 3;
            }

            //JpegMarker.StartOfScan
            ArraySegment<byte> startOfScan = segments.Segments[JpegMarker.StartOfScan]![0];
            index = 3;
            numComponents = startOfScan[2];
            ColorComponent component;
            for (int i = 0; i < numComponents; i++)
            {
                component = components[startOfScan[index++]];
                component.HuffmanTableIdAc = startOfScan[index] & 0x0f;
                component.HuffmanTableIdDc = (startOfScan[index++] >> 4) & 0x0f;
            }

            index = (startOfScan[0] << 8) + startOfScan[1];
            byte[] data = new byte[startOfScan.Count - index];
            Array.Copy(startOfScan.Array!, startOfScan.Offset + index, data, 0, data.Length);

            // SegmentType.QuantizationTable
            List<int[]> quants = new();
            foreach (var arr in segments.Segments[JpegMarker.DefineQuantizationTable])
            {
                int idx = arr[2] & 0x0f; // table Id, 0-3
                // in practise, only Id 0,1 are used
                int f16 = arr[2] & 0xf0; // f16 = 1 means the values are 16 bits, 8 bits otherwise
                // in practise, f16 is 0
                // Now read 64 8/16 bit numbers
                int[] quant = new int[64];
                index = 3;
                for (int i = 0; i < quant.Length; i++)
                {
                    quant[Zigzag.ZIGZAG[i]] = f16 > 0
                        ? ((arr[index + i * 2] << 8) | (arr[index + 1 + i * 2] << 0))
                        : arr[index + i];
                }
                quants.Add(quant);
            }

            // SegmentType.HuffmanTable
            List<HufCodec> hac = new List<HufCodec>();
            List<HufCodec> hdc = new List<HufCodec>();
            foreach (ArraySegment<byte> arr in segments.Segments[JpegMarker.DefineHuffmanTable])
            {
                HufCodec hufCodec = HufCodec.FromArraySegment(arr);
                (hufCodec.IsAc ? hac : hdc).Add(hufCodec);
            }

            return new Jfif(width, height, 
                quants.ToArray(),
                hac.ToArray(), hdc.ToArray(),
                numComponents, components,
                data);
        }

        internal JpegSegments ToSegments()
        {
            Dictionary<JpegMarker, List<ArraySegment<byte>>> segmentsDict = new()
            {
                { JpegMarker.StartOfFrame0, new List<ArraySegment<byte>>() { GetStartOfFrameBaselineSegment() } },
                { JpegMarker.StartOfScan, new List<ArraySegment<byte>>() { GetStartOfScanSegment() } },
                { JpegMarker.DefineQuantizationTable, GetQuantizationTableSegments() },
                { JpegMarker.DefineHuffmanTable, GetHuffmanTableSegments() }
            };

            return new JpegSegments(segmentsDict);
        }

        List<ArraySegment<byte>> GetQuantizationTableSegments()
        {
            List<ArraySegment<byte>> result = new()
            {
                QuantTableToArraySegment(0, Quant.QuantLuminance),
                QuantTableToArraySegment(1, Quant.QuantChrominance)
            };

            return result;
        }

        static ArraySegment<byte> QuantTableToArraySegment(int id, int[] table)
        {
            byte[] bytes = new byte[3+table.Length];
            bytes[0] = (byte)(bytes.Length>>8);
            bytes[1] = (byte)bytes.Length;
            bytes[2] = (byte)id;

            for (int i = 0; i < table.Length; i++)
            {
                bytes[i+3] = (byte)table[i];
            }

            return new ArraySegment<byte>(bytes);
        }

        List<ArraySegment<byte>> GetHuffmanTableSegments()
        {
            List<ArraySegment<byte>> result = new();

            result.AddRange(HuffmanTablesDc.Select(t => t.ToArraySegment()));
            result.AddRange(HuffmanTablesAc.Select(t => t.ToArraySegment()));

            return result;
        }

        ArraySegment<byte> GetStartOfFrameBaselineSegment()
        {
            byte[] bytes = new byte[17];
            bytes[0] = (byte)(bytes.Length >> 8);
            bytes[1] = (byte)bytes.Length;
            bytes[2] = 8; // Precision
            bytes[3] = (byte)(Height >> 8);
            bytes[4] = (byte)Height;
            bytes[5] = (byte)(Width >> 8);
            bytes[6] = (byte)Width;
            bytes[7] = (byte)NumComponents;
            int index = 8;
            byte id = 1;
            foreach(var component in Components)
            {
                bytes[index++] = id++;
                bytes[index++] = (1 << 4) | 1;
                bytes[index++] = (byte)component.QuantizationTableIndex;
            }
            return new ArraySegment<byte>(bytes);
        }

        ArraySegment<byte> GetStartOfScanSegment()
        {
            int headerLength = 6 + NumComponents * 2;
            byte[] bytes = new byte[headerLength+ScanData.Length];
            bytes[0] = (byte)(headerLength >> 8);
            bytes[1] = (byte)headerLength;
            bytes[2] = (byte)Components.Length;
            int index = 3;
            byte id = 1;
            foreach (var component in Components)
            {
                bytes[index++] = id++;
                bytes[index++] = (byte)((component.HuffmanTableIdDc << 4) | component.HuffmanTableIdAc);
            }
            bytes[index++] = 0;
            bytes[index++] = 63;
            bytes[index++] = 0;
            Array.Copy(ScanData, 0, bytes, index, ScanData.Length);
            return new ArraySegment<byte>(bytes);
        }
    }
}
