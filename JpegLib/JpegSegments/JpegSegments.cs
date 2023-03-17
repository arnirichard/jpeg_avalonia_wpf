using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public interface IJpegSegment
    {
        JpegSegment ToJpegSegment();
    }

    public class JpegSegment
    {
        public readonly JpegMarker Marker;
        public ArraySegment<byte> Segment;

        public JpegSegment(JpegMarker marker, ArraySegment<byte> segment)
        {
            Marker = marker;
            Segment = segment;
        }

        public override string ToString()
        {
            return string.Format("{0}:Length {1}",
                Marker,
                Segment.Count);
        }
    }

    internal static class JpegSegments
    {
        public static async Task<List<JpegSegment>> ReadJpeg(string fileName)
        {
            byte[] data = await File.ReadAllBytesAsync(fileName);

            if (data.Length < 4)
                throw new Exception("File is too short");

            if (data[0] != 0xff || data[1] != 0xd8)
                throw new Exception("SOI not found");

            byte b;
            List<JpegSegment> segments = new();
            JpegMarker? segmentType;
            int i = 2;
            int length;

            // if 0xff is followed by 0xff , then ignore the first 0xff
            while (i < data.Length)
            {
                if (data[i] != 0xff)
                    throw new Exception("Expecting 0xff in " + i);

                i++;
                b = data[i++];

                if (b == 0xff)
                    break;

                if (!Enum.IsDefined(typeof(JpegMarker), b))
                    throw new Exception("Segment type not valid: " + b);

                segmentType = (JpegMarker)b;

                if (segmentType == JpegMarker.EndOfImage)
                    break;

                length = (data[i] << 8) + data[i + 1];
                i += length;

                if (segmentType == JpegMarker.StartOfScan)
                {
                    // Must include the length in this data
                    while (i < data.Length)
                    {
                        if (data[i] == 0xff && data[i + 1] != 0x00 && data[i + 1] != 0xff)
                            break;
                        length++;
                        i++;
                    }
                }

                segments.Add(new JpegSegment(segmentType.Value, new ArraySegment<byte>(data, i - length, length)));
            }

            return segments;
        }

        internal static void WriteJpeg(this List<JpegSegment> segments, string fileName)
        {
            using (FileStream sw = File.OpenWrite(fileName))
            {
                foreach (var segment in segments)
                {
                    sw.WriteByte(0xff);
                    sw.WriteByte((byte)segment.Marker);
                    sw.Write(segment.Segment);
                }
            }
        }
    }
}
