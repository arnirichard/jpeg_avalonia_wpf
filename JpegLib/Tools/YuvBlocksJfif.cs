using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    internal static class YuvBlocksJfif
    {
        internal static Jfif YCbCrBlocksToJfif(int[][][] yuvBlocks, int width, int height)
        {
            int[][][] dctBlocks = new int[yuvBlocks.Length][][];

            for (int i = 0; i < yuvBlocks.Length; i++)
            {
                dctBlocks[i] = new int[yuvBlocks[i].Length][];

                for (int j = 0; j < yuvBlocks[i].Length; j++)
                {
                    if (yuvBlocks[i][j] != null)
                        dctBlocks[i][j] = DCT.ForwardFast(yuvBlocks[i][j]);
                }
            }

            HufCodecMaker yHF = new HufCodecMaker();
            HufCodecMaker crHF = new HufCodecMaker();

            for (int i = 0; i < dctBlocks.Length; i++)
            {
                dctBlocks[i][0] = yHF.Sample(Zigzag.Zigzagize(Quant.Quantize(dctBlocks[i][0], Quant.QuantLuminance)), 0);
                if(dctBlocks[i][1] != null)
                    dctBlocks[i][1] = crHF.Sample(Zigzag.Zigzagize(Quant.Quantize(dctBlocks[i][1], Quant.QuantChrominance)), 1);
                if(dctBlocks[i][1] != null)
                    dctBlocks[i][2] = crHF.Sample(Zigzag.Zigzagize(Quant.Quantize(dctBlocks[i][2], Quant.QuantChrominance)), 2);
            }

            HufCodec yDcHf = yHF.CreateDcCodec(0);
            HufCodec yAcHf = yHF.CreateAcCodec(0);
            HufCodec crDcHf = crHF.CreateDcCodec(1);
            HufCodec crAcHf = crHF.CreateAcCodec(1);

            int[] previousDc = new int[3];
            int coeff, coeffLength;
            byte numZeroes;
            HufCode code;
            byte[] data;

            using (var ms = new MemoryStream())
            {
                using(var bitWriter = new BitWriter(ms))
                {
                    int[] block;
                    HufCodec dcHf, acHf;

                    for (int i = 0; i < dctBlocks.Length; i++)
                    {
                        dcHf = yDcHf;
                        acHf = yAcHf;
                        for (int c = 0; c < 3; c++)
                        {
                            block = dctBlocks[i][c];
                            coeff = block[0] - previousDc[c];
                            coeffLength = coeff.BitLength();
                            code = dcHf.Encode(coeffLength);
                            previousDc[c] = block[0];

                            if (coeff < 0)
                            {
                                coeff += (1 << coeffLength) - 1;
                            }

                            bitWriter.WriteBits(code.Value, code.Length);
                            bitWriter.WriteBits(coeff, coeffLength);

                            for (int j = 1; j < 64; j++)
                            {
                                numZeroes = 0;

                                while (j < 64 && block[j] == 0)
                                {
                                    numZeroes += 1;
                                    j += 1;
                                }

                                if (j == 64)
                                {
                                    code = acHf.Encode(0);
                                    bitWriter.WriteBits(code.Value, code.Length);
                                    break;
                                }

                                while (numZeroes >= 16)
                                {
                                    code = acHf.Encode(0);
                                    bitWriter.WriteBits(code.Value, code.Length);
                                    numZeroes -= 16;
                                }

                                coeff = block[j];
                                coeffLength = coeff.BitLength();

                                if (coeff < 0)
                                {
                                    coeff += (1 << coeffLength) - 1;
                                }

                                code = acHf.Encode(numZeroes << 4 | coeffLength);
                                bitWriter.WriteBits(code.Value, code.Length);
                                bitWriter.WriteBits(coeff, coeffLength);
                            }

                            dcHf = crDcHf;
                            acHf = crAcHf;
                        }
                    }

                    data = bitWriter.GetData();
                }
            }

            return new Jfif(width, height,
                new int[][] { Quant.QuantLuminance, Quant.QuantChrominance},
                new HufCodec[] { yAcHf, crAcHf },
                new HufCodec[] { yDcHf, crDcHf },
                3, 
                new ColorComponent[] 
                { 
                    new ColorComponent(1, 1, 1, 0, yAcHf.Id, yDcHf.Id),
                    new ColorComponent(2, 1, 1, 1, crAcHf.Id, crDcHf.Id),
                    new ColorComponent(3, 1, 1, 1, crAcHf.Id, crDcHf.Id),
                },
                data);
        }

        internal static int[][][] JfifToYuvBlocks(Jfif jfif)
        {
            int size, code, zzIndex;
            HufCodec hcac, hcdc;
            ColorComponent component;
            int[] prevDC = new int[4];
            int[] quantizationTable;
            int[] block;

            int[][][] result = new int[jfif.NumBlocksWithPadding][][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new int[3][];

            for (int y = 0; y < jfif.BlocksHeight; y += jfif.VerticalSamplingFactor)
            {
                for (int x = 0; x < jfif.BlocksWidth; x += jfif.HorizontalSamplingFactor)
                {
                    for (int c = 0; c < jfif.NumComponents; c++)
                    {
                        component = jfif.Components[c + jfif.ComponentsFirstIndex];
                        hcac = jfif.HuffmanTablesAc[component.HuffmanTableIdAc];
                        hcdc = jfif.HuffmanTablesDc[component.HuffmanTableIdDc];
                        quantizationTable = jfif.QuantizationTables[component.QuantizationTableIndex];

                        // When component Id is 1, then Y block is read
                        for (int v = 0; v < component.SampFactorV; v++)
                        {
                            for (int h = 0; h < component.SampFactorH; h++)
                            {
                                size = hcdc.Decode(jfif);
                                code = size > 0 ? DecodeNegative(jfif.ScanInt(size), size) : 0;

                                block = new int[64];
                                prevDC[c] += code; // dc is always relative to last dc
                                block[0] = Quant.Dequantize(prevDC[c], 0, quantizationTable);

                                for (int j = 1; j < 64; j++)
                                {
                                    code = hcac.Decode(jfif);

                                    if (code <= 0)
                                        break; // all remaining is zero
                                               // length of coefficient 1-10
                                               // can be zero if 16 zeros 
                                    size = (code >> 0) & 0xf;
                                    // The upper 4 bits of a symbol tell how many zeros are preceding the current coefficient
                                    j += ((code >> 4) & 0xf);
                                    code = DecodeNegative(jfif.ScanInt(size), size);
                                    if (j < 64) // is this check necessary?
                                    {
                                        zzIndex = Zigzag.ZIGZAG[j];
                                        block[zzIndex] = Quant.Dequantize(code, zzIndex, quantizationTable);
                                    }
                                }
                                result[(y + v) * jfif.BlocksWidthWithPadding + (x + h)][c] = DCT.InverseFast(block);
                            }
                        }
                    }
                }
            }

            return result;
        }

        // for code of size S the numbers below 2^(S-1) are negative
        static int DecodeNegative(int code, int size)
        {
            return code >= (1 << (size - 1))
                ? code
                : code - (1 << size) + 1;
        }
    }
}

