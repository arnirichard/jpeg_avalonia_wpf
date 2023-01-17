using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DAW.FilterDesign
{
    class Term
    {
        public Complex Coeff;
        public int Power;

        public Term(Complex coeff, int power)
        {
            Coeff = coeff;
            Power = power;
        }

        public Term Mult(Term t)
        {
            return new Term(t.Coeff * Coeff, t.Power + Power);
        }

        public override string ToString()
        {
            return (Coeff == 1 && Power > 0 ? "" : Coeff) + (Power > 0 ? "z^" + Power : "");
        }
    }

    class Polynomial
    {
        public List<Term> Terms;

        public Polynomial()
        {
            Terms = new List<Term>() { new Term(1, 0) };
        }

        public int GetOrder()
        {
            return Terms[0].Power;
        }

        public Polynomial(List<Term> terms)
        {
            Terms = terms.OrderByDescending(t => t.Power).ToList();
        }

        public static Polynomial FromRoot(Complex root)
        {
            return new Polynomial(new List<Term>() { new Term(1, 1), new Term(-root, 0) });
        }

        public Polynomial Mult(Polynomial polynomial)
        {
            List<Term> terms = new List<Term>();

            foreach (var t1 in Terms)
            {
                foreach (var t2 in polynomial.Terms)
                {
                    terms.Add(t1.Mult(t2));
                }
            }

            Dictionary<int, Term> powers = new Dictionary<int, Term>();
            Term? term;
            foreach (var t in terms)
            {
                if (!powers.TryGetValue(t.Power, out term))
                    powers[t.Power] = t;
                else
                {
                    powers[t.Power] = new Term(term.Coeff + t.Coeff, term.Power);
                }
            }

            return new Polynomial(powers.Values.Where(t => t.Coeff != 0).ToList());
        }

        public override string ToString()
        {
            List<string> chunks = new List<string>();

            foreach (var t in Terms)
            {
                if (chunks.Count > 0 && t.Coeff.Norm() > 0)
                    chunks.Add("+");
                chunks.Add(t.ToString());
            }

            return string.Join("", chunks);
        }
    }
}
