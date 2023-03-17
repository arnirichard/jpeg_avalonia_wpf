using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class QuantTable : IJpegSegment
    {
        public readonly int Id;
        public readonly int[] Table;

        public QuantTable(int id, int[] table)
        {
            Id = id;
            Table = table;
        }

        public static QuantTable FromArraySegment(ArraySegment<byte> bytes)
        {
            int idx = bytes[2] & 0x0f; // table Id, 0-3
                                             // in practise, only Id 0,1 are used
            int f16 = bytes[2] & 0xf0; // f16 = 1 means the values are 16 bits, 8 bits otherwise
                                             // in practise, f16 is 0
                                             // Now read 64 8/16 bit numbers
            int[] table = new int[64];
            int index = 3;
            for (int i = 0; i < table.Length; i++)
            {
                table[Zigzag.ZIGZAG[i]] = f16 > 0
                    ? ((bytes[index + i * 2] << 8) | (bytes[index + 1 + i * 2] << 0))
                    : bytes[index + i];
            }

            return new QuantTable(idx, table);
        }

        public JpegSegment ToJpegSegment()
        {
            byte[] bytes = new byte[3 + Table.Length];
            bytes[0] = (byte)(bytes.Length >> 8);
            bytes[1] = (byte)bytes.Length;
            bytes[2] = (byte)Id;

            for (int i = 0; i < Table.Length; i++)
            {
                bytes[i + 3] = (byte)Table[i];
            }

            return new JpegSegment(JpegMarker.DefineQuantizationTable, new ArraySegment<byte>(bytes));
        }
    }
}
