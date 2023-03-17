using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    // Class that creates HufCodec instances from blocks, both DC and AC
    public class HufCodecMaker
    {
        Dictionary<int, int> dcFreq = new();
        int[] acFreq = new int[256];
        int[] previousDc = new int[3]; 

        public int[] Sample(int[] ints, int c)
        {
            int val = (ints[0] - previousDc[c]).BitLength();

            if (dcFreq.ContainsKey(val))
                dcFreq[val]++;
            else
                dcFreq[val] = 1;

            previousDc[c] = ints[0];
            byte numZeroes;
            int ac, acLength;

            for (int j = 1; j < ints.Length; j++)
            {
                numZeroes = 0;

                while (j < 64 && ints[j] == 0)
                {
                    numZeroes += 1;
                    j += 1;
                }

                if (j == 64)
                {
                    acFreq[0]++;
                    break;
                }

                while (numZeroes >= 16)
                {
                    acFreq[0]++;
                    numZeroes -= 16;
                }

                ac = ints[j];
                acLength = ac.BitLength();

                acFreq[(byte)(numZeroes << 4 | acLength)]++;
            }

            return ints;
        }

        public HufCodec CreateDcCodec(int id)
        {
            return CreateCodec(dcFreq, id, false);
        }

        public HufCodec CreateAcCodec(int id)
        {
            return CreateCodec(
                Enumerable.Range(0, acFreq.Length)
                    .Where(i => acFreq[i] != 0)
                    .Select(i => new KeyValuePair<int, int>(i, acFreq[i])),
                id, 
                true);
        }

        HufCodec CreateCodec(IEnumerable<KeyValuePair<int, int>> codeFreq, int id, bool isAc)
        {
            List<HufNode> allNodes = codeFreq.OrderBy(kvp => kvp.Value).Select(kvp => new HufNode(kvp.Value, symbol: kvp.Key)).ToList();
            LinkedList<HufNode> nodes = new LinkedList<HufNode>(allNodes);
            HufNode n1, n2, newNode;

            while (nodes.Count > 1)
            {
                n1 = nodes.First();
                nodes.RemoveFirst();
                n2 = nodes.First();
                nodes.RemoveFirst();
                newNode = new HufNode(n1.Frequency + n2.Frequency, n1, n2);
               
                var insertBefore = nodes.FirstOrDefault(n => n.Frequency >= newNode.Frequency);

                if (insertBefore != null)
                    nodes.AddBefore(nodes.Find(insertBefore)!, newNode);
                else
                    nodes.AddLast(newNode);
            }

            if (nodes.Count == 1)
            {
                var node = nodes.First();
                node.SetCode(node.Symbol == null ? 0 : 1);
            }

            byte[] huftb = new byte[HufCodec.MAX_HUFFMAN_CODE_LEN + allNodes.Count];
            int index = HufCodec.MAX_HUFFMAN_CODE_LEN;

            allNodes = allNodes.OrderBy(n => n.CodeLength).ToList();

            foreach (var n in allNodes)
            {
                if (n.Symbol != null && n.CodeLength > 0)
                {
                    huftb[n.CodeLength-1]++;
                    huftb[index++] = (byte)n.Symbol;
                }
            }

            return new HufCodec(id, isAc, huftb);
        }

        class HufNode
        {
            public int Frequency;
            public int? Symbol;
            public HufNode? Left;
            public HufNode? Right;
            public int CodeLength;

            public HufNode(int frequency, HufNode? n1 = null, HufNode? n2 = null, int? symbol = null)
            {
                Symbol = symbol;
                Frequency = frequency;
                Left = n1;
                Right = n2;
            }

            public void SetCode(int length)
            {
                CodeLength = length;
                Left?.SetCode(length + 1);
                Right?.SetCode(length + 1);
            }

            public override string ToString()
            {
                return string.Format("Symbol {0}, Freq {1}",
                    Symbol?.ToString() ?? "-",
                    Frequency);
            }
        }
    }
}
