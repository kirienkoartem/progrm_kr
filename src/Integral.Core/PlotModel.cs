using System;
using System.Collections.Generic;

namespace Integral.Core
{
    /// <summary>Точка графика; Y = null, если функция в точке не определена.</summary>
    public readonly struct PlotPoint
    {
        public double X { get; }
        public double? Y { get; }
        public PlotPoint(double x, double? y) { X = x; Y = y; }
    }

    /// <summary>
    /// Данные для построения графика: выборка точек кривой, диапазоны осей, деления, узлы разбиения.
    /// Используется и окном «График», и HTML-отчётом, поэтому оба графика совпадают.
    /// </summary>
    public sealed class PlotModel
    {
        /// <summary>Наибольшее n, при котором показываются узлы разбиения.</summary>
        public const int MaxNodes = 64;

        public double A { get; private set; }
        public double B { get; private set; }
        public double XMin { get; private set; }
        public double XMax { get; private set; }
        public double YMin { get; private set; }
        public double YMax { get; private set; }
        public IReadOnlyList<PlotPoint> Curve { get; private set; } = new List<PlotPoint>();
        public IReadOnlyList<PlotPoint> Nodes { get; private set; } = new List<PlotPoint>();
        public IReadOnlyList<double> XTicks { get; private set; } = new List<double>();
        public IReadOnlyList<double> YTicks { get; private set; } = new List<double>();

        /// <summary>
        /// Строит модель: x — от a до b с запасом 8 % по краям, <paramref name="samples"/> точек кривой,
        /// узлы разбиения при n ≤ <see cref="MaxNodes"/>, около xTicks и yTicks делений осей.
        /// </summary>
        public static PlotModel Build(Node f, double a, double b, int n, int samples, int xTicks, int yTicks)
        {
            var m = new PlotModel { A = Math.Min(a, b), B = Math.Max(a, b) };
            double pad = (m.B - m.A) * 0.08;
            if (pad <= 0) pad = 1;
            m.XMin = m.A - pad;
            m.XMax = m.B + pad;

            samples = Math.Max(10, samples);
            var curve = new List<PlotPoint>(samples + 1);
            var values = new List<double>();
            for (int i = 0; i <= samples; i++)
            {
                double x = m.XMin + (m.XMax - m.XMin) * i / samples;
                double? y = TryEval(f, x);
                curve.Add(new PlotPoint(x, y));
                if (y.HasValue) values.Add(y.Value);
            }
            m.Curve = curve;
            m.SetYRange(values);

            var nodes = new List<PlotPoint>();
            if (n >= 1 && n <= MaxNodes)
                for (int i = 0; i <= n; i++)
                {
                    double x = m.A + (m.B - m.A) * i / n;
                    nodes.Add(new PlotPoint(x, TryEval(f, x)));
                }
            m.Nodes = nodes;
            m.XTicks = new List<double>(AxisScale.Ticks(m.XMin, m.XMax, Math.Max(2, xTicks)));
            m.YTicks = new List<double>(AxisScale.Ticks(m.YMin, m.YMax, Math.Max(2, yTicks)));
            return m;
        }

        /// <summary>
        /// Диапазон y: включает ноль (ось x и площадь видны). Если отдельные значения «улетают»
        /// (полюс функции), берутся 2-й и 98-й процентили, чтобы не сжать график в линию.
        /// </summary>
        private void SetYRange(List<double> values)
        {
            double lo = 0, hi = 0;
            if (values.Count > 0)
            {
                values.Sort();
                double min = values[0], max = values[values.Count - 1];
                double q2 = values[(int)(0.02 * (values.Count - 1))], q98 = values[(int)(0.98 * (values.Count - 1))];
                bool outliers = (max - min) > 10 * Math.Max(q98 - q2, 1e-12);
                lo = Math.Min(0, outliers ? q2 : min);
                hi = Math.Max(0, outliers ? q98 : max);
            }
            double pad = (hi - lo) * 0.08;
            if (pad <= 0) pad = 1;
            YMin = lo - pad;
            YMax = hi + pad;
        }

        private static double? TryEval(Node f, double x)
        {
            try { return Evaluator.Evaluate(f, x); }
            catch (IntegralException) { return null; }
        }

        /// <summary>Перевод x в экранную координату области шириной width, начинающейся с left.</summary>
        public double MapX(double x, double left, double width) => left + (x - XMin) / (XMax - XMin) * width;

        /// <summary>Перевод y в экранную координату (ось y направлена вниз).</summary>
        public double MapY(double y, double top, double height) => top + height - (y - YMin) / (YMax - YMin) * height;
    }
}
