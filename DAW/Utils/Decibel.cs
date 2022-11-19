using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    public class Decibel
    {
        // 20 / ln( 10 )
        private const double LOG_2_DB = 8.6858896380650365530225783783321;

        // ln( 10 ) / 20
        private const double DB_2_LOG = 0.11512925464970228420089957273422;

        const double refSpl = 0.00002;
        /// Base-10 logarithm of [e].
        const double log10e = 0.4342944819032518;

        /// <summary>
        /// linear to dB conversion
        /// </summary>
        /// <param name="lin">linear value</param>
        /// <returns>decibel value</returns>
        public static double LinearToDecibels(double lin)
        {
            return Math.Log(lin) * LOG_2_DB;
        }

        /// <summary>
        /// dB to linear conversion
        /// </summary>
        /// <param name="dB">decibel value</param>
        /// <returns>linear value</returns>
        public static double DecibelsToLinear(double dB)
        {
            return Math.Exp(dB * DB_2_LOG);
        }

        public static double AvgPowerToSQL(double power)
        {
            return Math.Max(0, (20 * log10e * Math.Log(power/ refSpl)));
        }

    }
}
