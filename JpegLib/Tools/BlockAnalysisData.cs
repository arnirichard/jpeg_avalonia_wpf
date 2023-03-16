using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class BlockAnalysisData
    {
        public int[]? Rgb { get; set; }
        public int[]? Yuv { get; set; }
        public int[]? YDct { get; set; }
        public int[]? UDct { get; set; }
        public int[]? VDct { get; set; }
        public int[]? YDctQuantized { get; set; }
        public int[]? UDctQuantized { get; set; }
        public int[]? VDctQuantized { get; set; }
        public int[]? YDctQZigZagged { get; set; }
        public int[]? UDctQZigZagged { get; set; }
        public int[]? VDctQZigZagged { get; set; }
        public int[]? YIDct { get; set; }
        public int[]? UIDct { get; set; }
        public int[]? VIDct { get; set; }
        public int[]? JpegRgb { get; set; }
        public int[]? JpegYuv { get; set; }

        public static BlockAnalysisData CreateFrom(int[] rgb)
        {
            // Adding 128 so the values fit into 0-255
            int[][] yuv = YCbCrRgbColor.RgbToYCrCb(rgb, 0);
            int[] originalYuv = new int[64];
            for (int i = 0; i < originalYuv.Length; i++)
            {
                originalYuv[i] = ((yuv[0][i] + 128) << 16) + ((yuv[1][i] + 128) << 8) + yuv[2][i];
            }
            
            int[][] dct = new int[3][];
           
            for (int i = 0; i < yuv.Length; i++)
            {
                dct[i] = DCT.ForwardFast(yuv[i]);
            }

            int[] quantLuminance = Quant.QuantLuminance;
            int[] quantChrominance = Quant.QuantChrominance;
            int[] lum_quant_dct = Quant.Quantize(dct[0], quantLuminance);
            int[] u_quant_dct = Quant.Quantize(dct[1], quantChrominance);
            int[] v_quant_dct = Quant.Quantize(dct[2], quantChrominance);
            int[] y_q_dct_zz = Zigzag.Zigzagize(lum_quant_dct);
            int[] u_q_dct_zz = Zigzag.Zigzagize(u_quant_dct);
            int[] v_q_dct_zz = Zigzag.Zigzagize(v_quant_dct);
            int[] idct_y = DCT.InverseFast(Quant.Dequantize(lum_quant_dct, quantLuminance));
            int[] idct_u = DCT.InverseFast(Quant.Dequantize(u_quant_dct, quantChrominance));
            int[] idct_v = DCT.InverseFast(Quant.Dequantize(v_quant_dct, quantChrominance));

            int[] jpegYuv = new int[64];

            for (int i = 0; i < jpegYuv.Length; i++)
            {
                jpegYuv[i] = (Math.Min(255, Math.Max(0, idct_y[i]+128)) << 16) + (Math.Min(255, Math.Max(0, idct_u[i] + 128)) << 8) + Math.Min(255, Math.Max(0, idct_v[i] + 128));
            }

            int[] jpegRgb = YCbCrRgbColor.YuvToRgb(jpegYuv, 128);

            return new BlockAnalysisData()
            {
                Rgb = rgb,
                Yuv = originalYuv,
                YDct = dct[0],
                UDct = dct[1],
                VDct = dct[2],
                YDctQuantized = lum_quant_dct,
                UDctQuantized = u_quant_dct,
                VDctQuantized = v_quant_dct,
                YDctQZigZagged = y_q_dct_zz,
                UDctQZigZagged = u_q_dct_zz,
                VDctQZigZagged = v_q_dct_zz,
                YIDct = idct_y,
                UIDct = idct_u,
                VIDct = idct_v,
                JpegRgb = jpegRgb,
                JpegYuv = jpegYuv,
            };
        }
    }
}
