using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class JfifHeader : IJpegSegment
    {
        public JpegMarker SOF;
        public readonly int Width, Height;
        public readonly ColorComponent[] Components;
        public readonly int FirstComponentId;

        public readonly int VerticalSamplingFactor, HorizontalSamplingFactor;
        public readonly int BlocksWidth, BlocksHeight;
        public readonly int BlocksWidthWithPadding, BlocksHeightWithPadding;
        public int NumBlocks => BlocksHeight * BlocksWidth;
        public int NumBlocksWithPadding => BlocksHeightWithPadding * BlocksWidthWithPadding;
       
        public int NumberOfComponents => Components.Length;
        public bool IsBaseLine => SOF == JpegMarker.StartOfFrame0;
        public bool IsProgessive => SOF == JpegMarker.StartOfFrame2;

        public JfifHeader(JpegMarker sof, int width, int height, ColorComponent[] components)
        {
            SOF = sof;
            Width = width;
            Height = height;
            Components = components;
            FirstComponentId = components[0].Id;

            VerticalSamplingFactor = components.Where(c => c != null).Max(v => v.SampFactorV);
            HorizontalSamplingFactor = components.Where(c => c != null).Max(v => v.SampFactorH);
            BlocksWidth = (width + 7) / 8;
            BlocksWidthWithPadding = BlocksWidth + (BlocksWidth % HorizontalSamplingFactor);
            BlocksHeight = (height + 7) / 8;
            BlocksHeightWithPadding = BlocksHeight + (BlocksHeight % VerticalSamplingFactor);
        }

        public ColorComponent GetComponent(int id)
        {
            return Components[id-FirstComponentId];
        }

        public static JfifHeader FromArraySegment(JpegSegment jpegSegment)
        {
            ArraySegment<byte> segment = jpegSegment.Segment;

            int height = (segment[3] << 8) + segment[4];
            int width = (segment[5] << 8) + segment[6];
            // number of components is 1 or 3, 1 for grayscale  (only luminance)
            // 4 components is CMYK (another color mode)
            int numComponents = segment[7] < 4 ? segment[7] : 4;
            ColorComponent[] components = new ColorComponent[numComponents];
            int index = 8;
            for (int i = 0; i < numComponents; i++)
            {
                components[i] = new ColorComponent(
                    segment[index], // is 0-3
                    Math.Max(1, segment[index + 1] & 0x0f),
                    Math.Max(1, (segment[index + 1] >> 4) & 0x0f),
                    segment[index + 2] & 0x0f
                );
                index += 3;
            }

            return new JfifHeader(jpegSegment.Marker, width, height, components);
        }

        public JpegSegment ToJpegSegment()
        {
            byte[] bytes = new byte[17];
            bytes[0] = (byte)(bytes.Length >> 8);
            bytes[1] = (byte)bytes.Length;
            bytes[2] = 8; // Precision
            bytes[3] = (byte)(Height >> 8);
            bytes[4] = (byte)Height;
            bytes[5] = (byte)(Width >> 8);
            bytes[6] = (byte)Width;
            bytes[7] = (byte)Components.Length;
            int index = 8;
            byte id = 1;
            foreach (var component in Components)
            {
                bytes[index++] = id++;
                bytes[index++] = (1 << 4) | 1;
                bytes[index++] = (byte)component.QuantizationTableIndex;
            }
            return new JpegSegment(SOF, new ArraySegment<byte>(bytes));
        }
    }
}
