using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JpegLib
{
    public struct HufCode
    {
        public int Value;
        public int Length;

        public HufCode(int code, int length)
        {
            this.Value = code;
            this.Length = length;
        }

        public override string ToString()
        {
            return string.Format("Value {0}, Length {1}",
                Value,
                Length);
        }
    }

    public class HufCodec : IJpegSegment
    {
        // Maximum code length is 16 bits
        public const int MAX_HUFFMAN_CODE_LEN = 16;

        public readonly int Id;
        public readonly bool IsAc;
        // first 16 bytes are numbers of codes of length 1-16 bits
        // the remaining bytes are the symbols
        internal readonly byte[] HufTab;
        int[] First = new int[MAX_HUFFMAN_CODE_LEN];
        // Starting index of each code
        int[] Index = new int[MAX_HUFFMAN_CODE_LEN];
        HufCode[] Codes;
        byte[] Symbols;

        public HufCodec(int id, bool isAc, byte[] huftab)
        {
            Id = id;
            IsAc = isAc;
            HufTab = huftab;

            Codes = new HufCode[huftab.Length - MAX_HUFFMAN_CODE_LEN];
            Symbols = new byte[huftab.Length - MAX_HUFFMAN_CODE_LEN];
            Array.Copy(huftab, MAX_HUFFMAN_CODE_LEN, Symbols, 0, Symbols.Length);
            int num, code = 0, counter = 0;

            for(int i = 0;i < MAX_HUFFMAN_CODE_LEN; i++)
            {
                num = huftab[i];
                for(int j = 0; j < num; j++)
                {
                    Codes[counter++] = new HufCode(code++, i+1);
                }
                code <<= 1;
            }

            First[0] = 0; // there is never code of length 0
            Index[0] = MAX_HUFFMAN_CODE_LEN;

            for (int i = 1; i < MAX_HUFFMAN_CODE_LEN; i++)
            {
                First[i] = (First[i - 1] + HufTab[i - 1]) << 1;
                Index[i] = Index[i - 1] + HufTab[i - 1];
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}, {2} codes",
                Id,
                IsAc ? "AC" : "DC",
                Codes.Length);
        }

        public int Decode(BitReader bitReader)
        {
            int result = -1;
            int b;
            int code = 0, length = 0;

            while (true)
            {
                b = bitReader.ScanBit();
                if (b == -1)
                    break;
                code <<= 1;
                code |= b;
                if (code - First[length] < HufTab[length])
                    break;
                if (++length == MAX_HUFFMAN_CODE_LEN)
                    return result;
            }

            int idx = Index[length] + (code - First[length]);
            return idx < MAX_HUFFMAN_CODE_LEN + 256 ? HufTab[idx] : -1;
        }

        internal HufCode Encode(int symbol)
        {
            for(int i = 0; i < Symbols.Length; i++)
            {
                if (Symbols[i] == symbol)
                    return Codes[i];
            }
            return new HufCode(0, 0);
        }

        public JpegSegment ToJpegSegment()
        {
            byte[] result = new byte[3 + HufTab.Length];
            result[0] = (byte)(result.Length >> 8);
            result[1] = (byte)result.Length;
            result[2] = (byte)(((IsAc ? 1 : 0) << 4) | Id);
            Array.Copy(HufTab, 0, result, 3, HufTab.Length);
            return new JpegSegment(JpegMarker.DefineHuffmanTable, result);
        }

        internal static HufCodec FromArraySegment(ArraySegment<byte> arr)
        {
            int idx = arr[2] & 0x0f; // table Id 0-3
            int fac = arr[2] & 0xf0; // 0 (DC) or 1 (AC)
            int len = 0;
            for (int i = 3; i < 19; i++)
            {
                len += arr[i]; // how many codes have length i
            }
            // len is the total number of codes
            // the remaining bytes are the symbols, usually 12 for DC table, 162 for AC table
            byte[] huftab = new byte[MAX_HUFFMAN_CODE_LEN + len];
            Array.Copy(arr.Array!, arr.Offset + 3, huftab, 0, 16 + len);

            return new HufCodec(idx, fac > 0, huftab);
        }
    }
}
