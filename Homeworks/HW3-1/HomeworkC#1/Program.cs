using System;

class Polynomial
{
    private int degree;
    private double[] coeffs;

    public Polynomial()
    {
        degree = 0;
        coeffs = new double[1] { 0.0 };
    }

    public Polynomial(double[] new_coeffs)
    {
        degree = new_coeffs.Length - 1;
        coeffs = (double[])new_coeffs.Clone();
    }

    public int Degree
    {
        get { return degree; }
    }

    public double[] Coeffs
    {
        get { return (double[])coeffs.Clone(); }
    }

    public override string ToString()
    {
        string result = "";
        bool firstTerm = true;

        for (int i = 0; i <= degree; i++)
        {
            double coeff = coeffs[i];
            if (coeff == 0)
                continue;


            if (!firstTerm)
            {
                if (coeff > 0)
                    result += " + ";
                else
                {
                    result += " - ";
                    coeff = Math.Abs(coeff);
                }
            }
            else
            {

                if (coeff < 0)
                {
                    result += "-";
                    coeff = Math.Abs(coeff);
                }
                firstTerm = false;
            }


            if (i == 0)
            {
                result += coeff.ToString();
            }
            else if (i == 1)
            {
                if (coeff == 1)
                    result += "x";
                else
                    result += coeff.ToString() + "x";
            }
            else
            {
                if (coeff == 1)
                    result += "x^" + i;
                else
                    result += coeff.ToString() + "x^" + i;
            }
        }


        if (result == "")
            result = "0";

        return result;
    }
   public static Polynomial operator +(Polynomial p1, Polynomial p2)
    {
        int maxD = Math.Max(p1.degree, p2.degree);
        double[] newCoefs = new double[maxD + 1];

        for (int i = 0; i <= maxD; i++)
        {
            double coef1 = i <= p1.degree ? p1.coeffs[i] : 0.0;
            double coef2 = i <= p2.degree ? p2.coeffs[i] : 0.0;
            newCoefs[i] = coef1 + coef2;
        }

        return new Polynomial(newCoefs);
    }

    class Programm
    {
        static void Main(string[] args)
        {
            double[] coeffs = { 1.0, 0.0, 2.0 };
            Polynomial p = new Polynomial(coeffs);

            Console.WriteLine(p);
        }
    }
}


