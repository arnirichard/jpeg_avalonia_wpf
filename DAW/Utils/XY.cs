using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Utils
{
    public struct XY
    {
        /// <summary>
        /// Real Part
        /// </summary>
        public float X;
        /// <summary>
        /// Imaginary Part
        /// </summary>
        public float Y;

        public float Power => X * X + Y * Y;
        public float Abs => (float)Math.Sqrt(Power);
        public double Phase => Math.Atan2(-Y, X);

        public XY Inverse => new XY() { X = X, Y = -Y };

        public override string ToString()
        {
            return string.Format("{0}{1}{2}i",
                X,
                Y < 0 ? "" : "+",
                Y);
        }

        public XY Multiply(XY complex)
        {
            return new XY()
            {
                X = X * complex.X - Y * complex.Y,
                Y = X * complex.Y + Y * complex.X
            };
        }
    }
}
