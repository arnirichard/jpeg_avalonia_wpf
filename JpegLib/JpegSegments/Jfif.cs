using JpegLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    internal class Jfif
    {
        public readonly JfifHeader Header;
        public readonly QuantTable[] QuantizationTables;
        public List<IJpegSegment> Segments;

        public int[] PreviousDc = new int[4];
        public HufCodec?[] HufCodecsAc = new HufCodec?[2];
        public HufCodec?[] HufCodecsDc = new HufCodec?[2];

        public Jfif(JfifHeader header,
            List<QuantTable> quantTables,
            List<IJpegSegment> segments)
        {
            Header = header;
            QuantizationTables = quantTables.ToArray();
            Segments = segments;
        }

        internal void SetHufCodec(HufCodec c)
        {
            (c.IsAc ? HufCodecsAc : HufCodecsDc)[c.Id] = c;
        }

        internal static Jfif FromSegments(List<JpegSegment> segments)
        {
            JfifHeader? header = null;
            List<QuantTable> quantTables = new();
            List<IJpegSegment> jpegSegments = new();

            foreach (var segment in segments)
            {
                switch(segment.Marker)
                {
                    case JpegMarker.StartOfFrame0:
                    case JpegMarker.StartOfFrame2:
                        header = JfifHeader.FromArraySegment(segment);
                        break;
                    case JpegMarker.DefineQuantizationTable:
                        quantTables.Add(QuantTable.FromArraySegment(segment.Segment));
                        break;
                    case JpegMarker.StartOfScan:
                        jpegSegments.Add(StartOfScan.FromArraySegment(segment.Segment));
                        break;
                    case JpegMarker.DefineHuffmanTable:
                        jpegSegments.Add(HufCodec.FromArraySegment(segment.Segment));
                        break;
                }
            }

            if (header == null)
                throw new Exception("StartOfFrame marker missing or not supported");


            return new Jfif(header, quantTables, jpegSegments);
        }

        internal List<JpegSegment> ToSegments()
        {
            List<JpegSegment> segments = new();

            segments.Add(new JpegSegment(JpegMarker.StartOfImage, new ArraySegment<byte>()));
            segments.AddRange(QuantizationTables.Select(t => t.ToJpegSegment()));
            segments.Add(Header.ToJpegSegment());
            segments.AddRange(Segments.Select(t => t.ToJpegSegment()));
            segments.Add(new JpegSegment(JpegMarker.EndOfImage, new ArraySegment<byte>()));

            return segments;
        }
    }
}
