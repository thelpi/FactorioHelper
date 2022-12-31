using System;
using System.Collections.Generic;

namespace FactorioHelper
{
    // each method assumes quality parameters
    public static class SystemExtensions
    {
        public static Fraction FractionSum<T>(this IEnumerable<T> collection, Func<T, Fraction> projectionFunc)
        {
            var value = new Fraction(0);
            foreach (var item in collection)
                value += projectionFunc(item);
            return value;
        }

        public static int Max(params int[] values)
        {
            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                max = Math.Max(max, values[i]);
            return max;
        }

        public static int Min(params int[] values)
        {
            var max = values[0];
            for (var i = 1; i < values.Length; i++)
                max = Math.Min(max, values[i]);
            return max;
        }
    }
}
