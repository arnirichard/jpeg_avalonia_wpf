using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class BitReader
    {
        internal ArraySegment<byte> ScanData;

        int lastBit = -1;
        byte currentByte;
        int counterReadBit, counterReadInt;

        public BitReader(ArraySegment<byte> data)
        {
            ScanData = data;
        }

        public int ScanBit()
        {
            counterReadBit++;
            int bit = ++lastBit;

            if (bit % 8 == 0)
            {
                if (bit / 8 >= ScanData.Count)
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
            counterReadInt++;
            int result = 0;
            int b;

            for (int i = 0; i < length; i++)
            {
                b = ScanBit();
                if (b == -1)
                    return -1;
                result <<= 1;
                result += b;
            }

            return result;
        }
    }
}
