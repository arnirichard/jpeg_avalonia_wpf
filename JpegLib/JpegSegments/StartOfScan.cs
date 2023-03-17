using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class StartOfScan : IJpegSegment
    {
        public readonly BitReader BitReader;
        public readonly byte[] ComponentIds, HufTableIdAc, HufTableIdDc;
        public readonly int StartOfSelection, EndOfSelection;
        public int Skips;
        // How many bits to shift, left is High = 0, right if High > 0
        // High is the value of Low in previous scan for same pixels
        public readonly int SuccessiveApproximationHigh,SuccessiveApproximationLow;

        public int NumberOfComponents => ComponentIds.Length;

        public StartOfScan(ArraySegment<byte> data,
            byte[] componentIds, byte[] hufTableIdAc, byte[] hufTableIdDc, 
            byte startOfSelection, byte endOfSelection,
            byte successiveApproximationHigh, byte successiveApproximationLow) 
        { 
            BitReader = new BitReader(data);
            ComponentIds = componentIds;
            HufTableIdAc = hufTableIdAc;
            HufTableIdDc = hufTableIdDc;
            StartOfSelection = startOfSelection;
            EndOfSelection = endOfSelection;
            SuccessiveApproximationHigh = successiveApproximationHigh;
            SuccessiveApproximationLow = successiveApproximationLow;
        }

        public override string ToString()
        {
            return string.Format("HufTabIdDc [{0}], HufTabIdAc [{1}], Selection {2}-{3}, SuccessiveApprox {4}-{5}, Length {6}",
                string.Join(",", HufTableIdDc),
                string.Join(",", HufTableIdAc),
                StartOfSelection, EndOfSelection,
                SuccessiveApproximationLow, SuccessiveApproximationHigh,
                BitReader.ScanData.Count);
        }


        public static StartOfScan FromArraySegment(ArraySegment<byte> startOfScan)
        {
            int index = 3;
            int numComponents = startOfScan[2];
            byte[] componentIds = new byte[numComponents];
            byte[] hufTableIdAc = new byte[numComponents];
            byte[] hufTableIdDc = new byte[numComponents];
            for (int i = 0; i < numComponents; i++)
            {
                componentIds[i] = startOfScan[index++];
                hufTableIdAc[i] = (byte)(startOfScan[index] & 0x0f);
                hufTableIdDc[i] = (byte)((startOfScan[index++] >> 4) & 0x0f);
            }
            byte startOfSelection = startOfScan[index++];
            byte endOfSelection = startOfScan[index++];
            byte successiveApproximation = startOfScan[index++];
            byte successiveApproximationHigh = (byte)(successiveApproximation >> 4);
            byte successiveApproximationLow = (byte)(successiveApproximation & 0x0f); ;

            ArraySegment<byte> data = new ArraySegment<byte>(startOfScan.Array!, startOfScan.Offset + index, startOfScan.Count - index);

            return new StartOfScan(data, componentIds, hufTableIdAc, hufTableIdDc,
                startOfSelection, endOfSelection,
                successiveApproximationHigh, successiveApproximationLow);
        }

        public JpegSegment ToJpegSegment()
        {
            int headerLength = 6 + NumberOfComponents * 2;
            byte[] bytes = new byte[headerLength + BitReader.ScanData.Count];
            bytes[0] = (byte)(headerLength >> 8);
            bytes[1] = (byte)headerLength;
            bytes[2] = (byte)NumberOfComponents;
            int index = 3;
            byte id = 1;
            for(int i = 0; i < NumberOfComponents; i++)
            {
                bytes[index++] = id++;
                bytes[index++] = (byte)((HufTableIdDc[i] << 4) | HufTableIdAc[i]);
            }
            bytes[index++] = 0;
            bytes[index++] = 63;
            bytes[index++] = 0;
            Array.Copy(BitReader.ScanData.Array!, BitReader.ScanData.Offset, bytes, index, BitReader.ScanData.Count);
            return new JpegSegment(JpegMarker.StartOfScan, new ArraySegment<byte>(bytes));
        }
    }
}
