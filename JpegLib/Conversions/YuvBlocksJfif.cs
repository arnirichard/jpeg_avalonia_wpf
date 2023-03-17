using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

            var components = new ColorComponent[]
                {
                    new ColorComponent(1, 1, 1, 0),
                    new ColorComponent(2, 1, 1, 1),
                    new ColorComponent(3, 1, 1, 1),
                };
            var header = new JfifHeader(JpegMarker.StartOfFrame0, width, height, components);
            
            List<QuantTable> quantTables = new List<QuantTable>()
            {
                new QuantTable(0, Quant.QuantLuminance),
                new QuantTable(1, Quant.QuantChrominance),
            };

            StartOfScan startOfScan = new StartOfScan(
                new ArraySegment<byte>(data),
                new byte[] { 1, 2, 3 },
                new byte[] {0, 1, 1},
                new byte[] { 0, 1, 1 },
                0, 63,
                0, 0
            );

            List<IJpegSegment> segments = new List<IJpegSegment>()
            {
                yAcHf, crAcHf, yDcHf, crDcHf, startOfScan
            };

            return new Jfif(header, quantTables, segments);
        }

        internal static int[][][] JfifToYuvBlocks(Jfif jfif, StartOfScan scanSegment)
        {
            int componentId;
            HufCodec? hcac, hcdc;
            ColorComponent component;
            int[] block;
            int[][][] result = jfif.YCbCrBlocks;
            int resultIndex;

            for (int y = 0; y < jfif.Header.BlocksHeight; y += jfif.Header.VerticalSamplingFactor)
            {
                for (int x = 0; x < jfif.Header.BlocksWidth; x += jfif.Header.HorizontalSamplingFactor)
                {
                    for (int c = 0; c < scanSegment.NumberOfComponents; c++)
                    {
                        hcac = jfif.HufCodecsAc[scanSegment.HufTableIdAc[c]];
                        hcdc = jfif.HufCodecsDc[scanSegment.HufTableIdDc[c]];
                        componentId = scanSegment.ComponentIds[c];
                        component = jfif.Header.GetComponent(componentId);

                        

                        for (int v = 0; v < component.SampFactorV; v++)
                        {
                            for (int h = 0; h < component.SampFactorH; h++)
                            {
                                resultIndex = (y + v) * jfif.Header.BlocksWidthWithPadding + (x + h);

                                if (resultIndex == 41)
                                {

                                }

                                block = result[resultIndex][componentId - jfif.Header.FirstComponentId];

                                if (block == null)
                                    block = result[resultIndex][componentId - jfif.Header.FirstComponentId] = new int[64];

                                if (jfif.Header.IsBaseLine)
                                {
                                    if (hcac == null || hcdc == null)
                                        throw new Exception("Missing Hufman codec");
                                    ReadBaselineBlock(block, jfif, scanSegment, hcac, hcdc, component);
                                    Quant.Dequantize(block, jfif.QuantizationTables[component.QuantizationTableIndex].Table);
                                    result[resultIndex][componentId - jfif.Header.FirstComponentId] = DCT.InverseFast(block);
                                }
                                else if(scanSegment.StartOfSelection == 0)
                                {
                                    if (hcdc == null)
                                        throw new Exception("Missing DC Hufman codec");
                                    ReadProgressiveBlockDc(block, jfif, scanSegment, hcdc, component);
                                }
                                else if(scanSegment.SuccessiveApproximationHigh == 0)
                                {
                                    if (hcac == null)
                                        throw new Exception("Missing AC Hufman codec");
                                    ReadProgressiveBlockAcInitial(block, jfif, scanSegment, hcac);
                                }
                                else
                                {
                                    if (hcac == null)
                                        throw new Exception("Missing AC Hufman codec");
                                    ReadProgressiveBlockAcRefinement(block, jfif, scanSegment, hcac);
                                }
                            }
                        }
                    }
                }
            }
            
            return result;
        }

        static void ReadProgressiveBlockAcRefinement(int[] block, Jfif jfif, StartOfScan scanSegment,
            HufCodec hcac)
        {
            BitReader bitReader = scanSegment.BitReader;
            int positive = 1 << scanSegment.SuccessiveApproximationLow;
            int negative = -1 << scanSegment.SuccessiveApproximationLow;
            int i = scanSegment.StartOfSelection;
            int size, code, numZeroes;

            if (scanSegment.Skips == 0)
            {
                for (; i <= scanSegment.EndOfSelection; ++i)
                {
                    code = hcac.Decode(bitReader);

                    if (code == -1)
                    {
                        throw new Exception("Error - Invalid AC value");
                    }

                    numZeroes = ((code >> 4) & 0xf);
                    size = (code >> 0) & 0xf;
                    int coeff = 0;

                    if (size != 0)
                    {
                        if (size != 1)
                        {
                            throw new Exception("Error - Invalid AC value");
                        }
                        switch (bitReader.ScanBit())
                        {
                            case 1:
                                coeff = positive;
                                break;
                            case 0:
                                coeff = negative;
                                break;
                            default: // -1, data stream is empty
                                throw new Exception("Error - Invalid AC value");
                        }
                    }
                    else
                    {
                        if (numZeroes != 15)
                        {
                            scanSegment.Skips = 1 << numZeroes;
                            int extraSkips = bitReader.ScanInt(numZeroes);
                            if (extraSkips == -1)
                            {
                                throw new Exception("Error - Invalid AC value");
                            }
                            scanSegment.Skips += extraSkips;
                            break;
                        }
                    }

                    do
                    {
                        if (block[Zigzag.ZIGZAG[i]] != 0)
                        {
                            switch (bitReader.ScanBit())
                            {
                                case 1:
                                    if ((block[Zigzag.ZIGZAG[i]] & positive) == 0)
                                    {
                                        if (block[Zigzag.ZIGZAG[i]] >= 0)
                                        {
                                            block[Zigzag.ZIGZAG[i]] += positive;
                                        }
                                        else
                                        {
                                            block[Zigzag.ZIGZAG[i]] += negative;
                                        }
                                    }
                                    break;
                                case 0:
                                    // do nothing
                                    break;
                                default: // -1, data stream is empty
                                    throw new Exception("Error - Invalid AC value");
                            }
                        }
                        else
                        {
                            if (numZeroes == 0)
                            {
                                break;
                            }
                            numZeroes -= 1;
                        }

                        i += 1;
                    } while (i <= scanSegment.EndOfSelection);

                    if (coeff != 0 && i <= scanSegment.EndOfSelection)
                    {
                        block[Zigzag.ZIGZAG[i]] = coeff;
                    }
                }
            }

            if (scanSegment.Skips > 0)
            {
                for (; i <= scanSegment.EndOfSelection; ++i)
                {
                    if (block[Zigzag.ZIGZAG[i]] != 0)
                    {
                        switch (bitReader.ScanBit())
                        {
                            case 1:
                                if ((block[Zigzag.ZIGZAG[i]] & positive) == 0)
                                {
                                    if (block[Zigzag.ZIGZAG[i]] >= 0)
                                    {
                                        block[Zigzag.ZIGZAG[i]] += positive;
                                    }
                                    else
                                    {
                                        block[Zigzag.ZIGZAG[i]] += negative;
                                    }
                                }
                                break;
                            case 0:
                                // do nothing
                                break;
                            default: // -1, data stream is empty
                                throw new Exception("Error - Invalid AC value");
                        }
                     }
                }
                scanSegment.Skips -= 1;
            }
        }


        static void ReadBaselineBlock(int[] block, Jfif jfif, StartOfScan scanSegment,
            HufCodec hcac, HufCodec hcdc, ColorComponent component)
        {
            BitReader bitReader = scanSegment.BitReader;
            int size, code, zzIndex, numZeroes;

            size = hcdc.Decode(bitReader);
            code = DecodeNegative(bitReader.ScanInt(size), size);
            jfif.PreviousDc[component.Id] += code; // dc is always relative to last dc
            block[0] = jfif.PreviousDc[component.Id];

            for (int j = 1; j < 64; j++)
            {
                code = hcac.Decode(bitReader);

                if (code <= 0)
                {
                    break; // all remaining is zero
                }

                // length of coefficient 1-10
                // can be zero if 16 zeros 
                // The upper 4 bits of a symbol tell how many zeros are preceding the current coefficient
                numZeroes = ((code >> 4) & 0xf);
                size = (code >> 0) & 0xf;

                j += numZeroes;
                code = DecodeNegative(bitReader.ScanInt(size), size);
                if (j < 64) // is this check necessary?
                {
                    zzIndex = Zigzag.ZIGZAG[j];
                    block[zzIndex] = code << scanSegment.SuccessiveApproximationLow;
                }
            }
        }

        static void ReadProgressiveBlockDc(int[] block, Jfif jfif, StartOfScan scanSegment,
            HufCodec hcdc, ColorComponent component)
        {
            BitReader bitReader = scanSegment.BitReader;
            int size, code, bit;

            if (scanSegment.SuccessiveApproximationHigh == 0)
            {
                // DC first visit
                size = hcdc.Decode(bitReader);
                code = DecodeNegative(bitReader.ScanInt(size), size);
                jfif.PreviousDc[component.Id] += code; // dc is always relative to last dc
                block[0] = jfif.PreviousDc[component.Id] << scanSegment.SuccessiveApproximationLow;
            }
            else
            {
                // DC refinement
                bit = bitReader.ScanBit();
                block[0] |= bit << scanSegment.SuccessiveApproximationLow;
            }
        }

        static void ReadProgressiveBlockAcInitial(int[] block, Jfif jfif, StartOfScan scanSegment,
            HufCodec hcac)
        {
            BitReader bitReader = scanSegment.BitReader;
            int size, code, numZeroes;

            if (scanSegment.Skips > 0)
            {
                scanSegment.Skips--;
                return;
            }

            for (int j = scanSegment.StartOfSelection; j <= scanSegment.EndOfSelection; j++)
            {
                code = hcac.Decode(bitReader);

                // length of coefficient 1-10
                // can be zero if 16 zeros 
                // The upper 4 bits of a symbol tell how many zeros are preceding the current coefficient
                numZeroes = ((code >> 4) & 0xf);
                size = (code >> 0) & 0xf;

                if (size > 0)
                {
                    for (int i = 0; i < numZeroes; ++i)
                    {
                        block[Zigzag.ZIGZAG[j++]] = 0;
                    }
                    code = DecodeNegative(bitReader.ScanInt(size), size);
                    block[Zigzag.ZIGZAG[j]] = code << scanSegment.SuccessiveApproximationLow;
                }
                else if (numZeroes == 15)
                {
                    for (int i = 0; i < numZeroes; ++i)
                    {
                        block[Zigzag.ZIGZAG[j++]] = 0;
                    }
                }
                else
                {
                    scanSegment.Skips = (1 << numZeroes) - 1;
                    int extraSkips = bitReader.ScanInt(numZeroes);
                    if (extraSkips == -1)
                        throw new Exception("Invalid AC value");
                    scanSegment.Skips += extraSkips;
                    break;
                }
            }
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

