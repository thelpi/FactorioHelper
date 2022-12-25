using System;
using System.Collections.Generic;

namespace FactorioHelper
{
    public struct Fraction
    {
        private const bool ApplyCtorReduce = true;

        private int _num;
        private int _den;

        public Fraction(int numerator, int denominator)
        {
            if (denominator == 0)
            {
                throw new ArgumentException("Denominator cannot be zero.", nameof(denominator));
            }
            _num = numerator;
            _den = denominator;
            if (ApplyCtorReduce)
                Reduce();
        }

        public Fraction(int value)
            : this(value, 1)
        { }

        public Fraction(decimal fraction)
        {
            var den = 1;
            while (Math.Floor(fraction) != fraction)
            {
                fraction *= 10;
                den *= 10;
            }
            _num = (int)Math.Floor(fraction);
            _den = den;
            if (ApplyCtorReduce)
                Reduce();
        }

        public decimal Decimal
            => _num / (decimal)_den;

        public static Fraction operator +(Fraction a)
            => a;
        public static Fraction operator -(Fraction a)
            => new Fraction(-a._num, a._den);

        public static Fraction operator +(Fraction a, Fraction b)
            => new Fraction(a._num * b._den + b._num * a._den, a._den * b._den);
        public static Fraction operator -(Fraction a, Fraction b)
            => a + (-b);
        public static Fraction operator *(Fraction a, Fraction b)
            => new Fraction(a._num * b._num, a._den * b._den);
        public static Fraction operator /(Fraction a, Fraction b)
            => b._num == 0 ? throw new DivideByZeroException() : new Fraction(a._num * b._den, a._den * b._num);

        public static Fraction operator +(Fraction a, int b)
            => new Fraction(a._num * 1 + b * a._den, a._den * 1);
        public static Fraction operator *(Fraction a, int b)
            => new Fraction(a._num * b, a._den * 1);
        public static Fraction operator -(Fraction a, int b)
            => a - new Fraction(b);
        public static Fraction operator -(int a, Fraction b)
            => new Fraction(a) - b;

        public static bool operator ==(Fraction a, int value)
            => a.Decimal == value;
        public static bool operator !=(Fraction a, int value)
            => a.Decimal != value;
        public static bool operator >=(Fraction a, int value)
            => a.Decimal >= value;
        public static bool operator <=(Fraction a, int value)
            => a.Decimal <= value;
        public static bool operator >(Fraction a, int value)
            => a.Decimal > value;
        public static bool operator <(Fraction a, int value)
            => a.Decimal < value;

        public static bool operator ==(Fraction a, decimal value)
            => a.Decimal == value;
        public static bool operator !=(Fraction a, decimal value)
            => a.Decimal != value;
        public static bool operator >=(Fraction a, decimal value)
            => a.Decimal >= value;
        public static bool operator <=(Fraction a, decimal value)
            => a.Decimal <= value;
        public static bool operator >(Fraction a, decimal value)
            => a.Decimal > value;
        public static bool operator <(Fraction a, decimal value)
            => a.Decimal < value;

        public override string ToString()
            => Math.Round(Decimal, 3).ToString();

        public static bool operator ==(Fraction a, Fraction b)
            => a.Equals(b);
        public static bool operator !=(Fraction a, Fraction b)
            => !(a == b);

        public Fraction Reduce()
        {
            var denMiddle = _den / 2;
            for (var dm = denMiddle; dm > 1; dm--)
            {
                if (_num % dm == 0 && _den % dm == 0)
                {
                    _num /= dm;
                    _den /= dm;
                }
            }
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType().IsAssignableFrom(typeof(decimal)))
            {
                return this == (decimal)obj;
            }
            else if (obj.GetType().IsAssignableFrom(typeof(int)))
            {
                return this == (int)obj;
            }
            else if(obj.GetType() != typeof(Fraction))
            {
                return false;
            }

            var reduced = ((Fraction)obj).Reduce();
            var meReduced = Reduce();
            return reduced._den == meReduced._den
                && reduced._num == meReduced._num;
        }

        public override int GetHashCode()
        {
            return _num ^ _den;
        }

        public static implicit operator Fraction(int b) => new Fraction(b);

        public static implicit operator Fraction(decimal b) => new Fraction(b);
    }

    public static class LinqExtensions
    {
        public static Fraction FractionSum<T>(this IEnumerable<T> collection, Func<T, Fraction> projectionFunc)
        {
            var value = new Fraction(0);
            foreach (var item in collection)
                value += projectionFunc(item);
            return value;
        }
    }
}
