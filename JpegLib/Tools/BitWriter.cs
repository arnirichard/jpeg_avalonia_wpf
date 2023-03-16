using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    internal class BitWriter : IDisposable
    {
        MemoryStream ms;
        int currentByte;
        int currentBits = 7;

        public BitWriter(MemoryStream ms)
        {
            this.ms = ms;
        }

        public void WriteBits(int value, int bits)
        {
            for(int i = bits-1; i > -1; i--)
            {
                currentByte |= ((value & (1 << i)) >> i) << currentBits;
                currentBits--;
                if(currentBits == 0)
                {
                    ms.WriteByte((byte)currentByte);
                    currentBits = 7;
                }
            }
        }

        public void Dispose()
        {
            if (currentBits % 8 != 0)
                ms.WriteByte((byte)currentByte);
            ms.Dispose();
        }

        internal byte[] GetData()
        {
            if (currentBits % 8 != 0)
                ms.WriteByte((byte)currentByte);
            return ms.ToArray();
        }
    }
}
