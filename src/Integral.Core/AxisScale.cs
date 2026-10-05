using System;
using System.Collections.Generic;

namespace Integral.Core
{
    /// <summary>Подбор «круглых» делений координатных осей для графика.</summary>
    public static class AxisScale
    {
        /// <summary>Шаг из ряда 1, 2, 5 × 10^k, при котором на [min; max] около <paramref name="target"/> делений.</summary>
        public static double NiceStep(double min, double max, int target)
        {
            double span = max - min;
            if (!(span > 0) || target < 1) return 1;
            double raw = span / target;
            double pow = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double f = raw / pow;
            double nice = f < 1.5 ? 1 : f < 3.5 ? 2 : f < 7.5 ? 5 : 10;
            return nice * pow;
        }

        /// <summary>Значения делений, лежащие внутри [min; max].</summary>
        public static IList<double> Ticks(double min, double max, int target)
        {
            var list = new List<double>();
            double step = NiceStep(min, max, target);
            double first = Math.Ceiling(min / step - 1e-9) * step;
            for (int i = 0; i < 1000; i++)
            {
                double v = first + i * step;
                if (v > max + step * 1e-9) break;
                list.Add(Math.Abs(v) < step * 1e-9 ? 0 : v);
            }
            return list;
        }
    }
}
