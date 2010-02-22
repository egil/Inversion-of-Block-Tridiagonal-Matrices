using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiledMatrixInversion.Math
{
    public struct Complex
    {
        float _real;
        float _imaginary;

        public Complex(float real, float imaginary)
        {
            _real = real;
            _imaginary = imaginary;
        }

        public float Real
        {
            get
            {
                return _real;
            }
            set
            {
                _real = value;
            }
        }

        public float Imaginary
        {
            get
            {
                return _imaginary;
            }
            set
            {
                _imaginary = value;
            }
        }

        public override string ToString()
        {
            return (String.Format("({0}, {1}i)", _real, _imaginary));
        }

        public static bool operator ==(Complex c1, Complex c2)
        {
            if ((c1._real == c2._real) && (c1._imaginary == c2._imaginary))
                return (true);
            return (false);
        }

        public static bool operator !=(Complex c1, Complex c2)
        {
            return (!(c1 == c2));
        }

        public override bool Equals(object o2)
        {
            Complex c2 = (Complex)o2;
            return (this == c2);
        }

        public override int GetHashCode()
        {
            return (_real.GetHashCode() ^ _imaginary.GetHashCode());
        }

        public static Complex operator +(Complex c1, Complex c2)
        {
            return (new Complex(c1._real + c2._real, c1._imaginary + c2._imaginary));
        }

        public static Complex operator -(Complex c1, Complex c2)
        {
            return (new Complex(c1._real - c2._real, c1._imaginary - c2._imaginary));
        }

        // product of two complex numbers
        public static Complex operator *(Complex c1, Complex c2)
        {
            return (new Complex(c1._real * c2._real - c1._imaginary * c2._imaginary,
                                c1._real * c2._imaginary + c2._real * c1._imaginary));
        }

        // quotient of two complex numbers
        public static Complex operator /(Complex c1, Complex c2)
        {
            if ((c2._real == 0.0f) && (c2._imaginary == 0.0f))
                throw new DivideByZeroException("Can't divide by zero Complex number");

            float newReal =
                (c1._real * c2._real + c1._imaginary * c2._imaginary) /
                (c2._real * c2._real + c2._imaginary * c2._imaginary);
            float newImaginary =
                (c2._real * c1._imaginary - c1._real * c2._imaginary) /
                (c2._real * c2._real + c2._imaginary * c2._imaginary);

            return (new Complex(newReal, newImaginary));
        }

        // non-operator versions for other languages
        public static Complex Add(Complex c1, Complex c2)
        {
            return (c1 + c2);
        }

        public static Complex Subtract(Complex c1, Complex c2)
        {
            return (c1 - c2);
        }

        public static Complex Multiply(Complex c1, Complex c2)
        {
            return (c1 * c2);
        }

        public static Complex Divide(Complex c1, Complex c2)
        {
            return (c1 / c2);
        }
    }
}