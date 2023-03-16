using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpegLib
{
    public class ColorComponent
    {
        // Id = 1 for luminance channel
        public int Id;
        // Sampling factor is 1 or 2, the latter only for the luminance channel
        public int SampFactorV; // Vertical
        public int SampFactorH; // Horizontal
        // Quantization table id
        public int QuantizationTableIndex;
        public int HuffmanTableIdAc;
        public int HuffmanTableIdDc;

        public ColorComponent(int id, int sampFactorV, int sampFactorH, int qtabIdx,
            int hfTableIdAc = 0, int hftableIdDc = 0)
        {
            Id = id;
            SampFactorV = sampFactorV;
            SampFactorH = sampFactorH;
            QuantizationTableIndex = qtabIdx;
            HuffmanTableIdAc = hfTableIdAc;
            HuffmanTableIdDc= hftableIdDc;
        }
    }
}
