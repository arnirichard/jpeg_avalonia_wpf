using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib   
{
    public enum JpegMarker : byte
    {
        // Padding
        Padding = 0xFF,
        // Start of image
        StartOfImage = 0xD8,
        // Reserved for application segments
        App0 = 0xE0,
        App1 = 0xE1,
        App2 = 0xE2,
        App3 = 0xE3,
        App4 = 0xE4,
        App5 = 0xE5,
        App6 = 0xE6,
        App7 = 0xE7,
        App8 = 0xE8,
        App9 = 0xE9,
        App10 = 0xEA,
        App11 = 0xEB,
        App12 = 0xEC,
        App13 = 0xED,
        App14 = 0xEE,
        App15 = 0xEF,
        // Start of Frame marker, non-differential, Huffman coding, Baseline DCT
        StartOfFrame0 = 0xC0,
        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Extended sequential DCT
        /// </summary>
        StartOfFrame1 = 0xC1,
        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Progressive DCT
        /// </summary>
        StartOfFrame2 = 0xC2,
        /// <summary>
        /// Start of Frame marker, non-differential, Huffman coding, Lossless (sequential)
        /// </summary>
        StartOfFrame3 = 0xC3,
        /// <summary>
        /// Start of Frame marker, differential, Huffman coding, Differential sequential DCT
        /// </summary>
        StartOfFrame5 = 0xC5,
        /// <summary>
        /// Start of Frame marker, differential, Huffman coding, Differential progressive DCT
        /// </summary>
        StartOfFrame6 = 0xC6,
        /// <summary>
        /// Start of Frame marker, differential, Huffman coding, Differential lossless (sequential)
        /// </summary>
        StartOfFrame7 = 0xC7,
        /// <summary>
        /// Start of Frame marker, non-differential, arithmetic coding, Extended sequential DCT
        /// </summary>
        StartOfFrame9 = 0xC9,
        /// <summary>
        /// Start of Frame marker, non-differential, arithmetic coding, Progressive DCT
        /// </summary>
        StartOfFrame10 = 0xCA,
        /// <summary>
        /// Start of Frame marker, non-differential, arithmetic coding, Lossless (sequential)
        /// </summary>
        StartOfFrame11 = 0xCB,
        /// <summary>
        /// Start of Frame marker, differential, arithmetic coding, Differential sequential DCT
        /// </summary>
        StartOfFrame13 = 0xCD,
        /// <summary>
        /// Start of Frame marker, differential, arithmetic coding, Differential progressive DCT
        /// </summary>
        StartOfFrame14 = 0xCE,
        /// <summary>
        /// Start of Frame marker, differential, arithmetic coding, Differential lossless (sequential)
        /// </summary>
        StartOfFrame15 = 0xCF,
        /// <summary>
        ///  Define Huffman table(s)
        /// </summary>
        DefineHuffmanTable = 0xC4,
        /// <summary>
        /// Define arithmetic coding conditioning(s)
        /// </summary>
        DefineArithmeticCodingConditioning = 0xCC,
        /// <summary>
        /// Define quantization table(s)
        /// </summary>
        DefineQuantizationTable = 0xDB,
        /// <summary>
        /// Define number of lines
        /// </summary>
        DefineNumberOfLines = 0xDC,
        /// <summary>
        /// Define restart interval
        /// Length (2 bytes)
        /// Restart interval (2 bytes)
        /// </summary>
        DefineRestartInterval = 0xDD,
        /// <summary>
        /// Start of scan
        /// </summary>
        StartOfScan = 0xDA,
        /// <summary>
        ///  Restart with modulo 8 count 0
        /// </summary>
        DefineRestart0 = 0xD0,
        /// <summary>
        ///  Restart with modulo 8 count 1
        /// </summary>
        DefineRestart1 = 0xD1,
        /// <summary>
        ///  Restart with modulo 8 count 2
        /// </summary>
        DefineRestart2 = 0xD2,
        /// <summary>
        ///  Restart with modulo 8 count 3
        /// </summary>
        DefineRestart3 = 0xD3,
        /// <summary>
        ///  Restart with modulo 8 count 4
        /// </summary>
        DefineRestart4 = 0xD4,
        /// <summary>
        ///  Restart with modulo 8 count 5
        /// </summary>
        DefineRestart5 = 0xD5,
        /// <summary>
        ///  Restart with modulo 8 count 6
        /// </summary>
        DefineRestart6 = 0xD6,
        /// <summary>
        ///  Restart with modulo 8 count 7
        /// </summary>
        DefineRestart7 = 0xD7,
        /// <summary>
        /// Comment
        /// </summary>
        Comment = 0xFE,
        /// <summary>
        /// End of image
        /// </summary>
        EndOfImage = 0xD9,
    }
}
