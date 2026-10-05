using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Integral.Core
{
    /// <summary>Численное интегрирование: формулы трапеций и Симпсона, оценка точности по Рунге.</summary>
    public static class Integrator
    {
        /// <summary>Вычисляет интеграл функции f на [A; B]. Бросает <see cref="IntegralException"/>.</summary>
        public static IntegrationResult Integrate(Node f, IntegrationParams p, Method method)
        {
            Validate(p);
            var sw = Stopwatch.StartNew();
            long evals = 0;
            Func<double, double> fn = x => { evals++; return Evaluator.Evaluate(f, x); };

            int order = method == Method.Simpson ? 4 : 2;
            double divisor = Math.Pow(2, order) - 1;
            var res = new IntegrationResult { Method = method };

            int n = Math.Max(p.N, method == Method.Simpson ? 2 : 1);
            if (method == Method.Simpson && n % 2 == 1) { n++; res.NAdjusted = true; }
            if (p.Eps > 0 && n < 2) n = 2;

            if (p.A == p.B)
            {
                res.Value = 0; res.N = n; res.H = 0; res.ErrorEstimate = 0;
                res.ElapsedMs = sw.Elapsed.TotalMilliseconds;
                return res;
            }

            Func<int, double> compute = k => method == Method.Simpson ? Simpson(fn, p.A, p.B, k) : Trapezoid(fn, p.A, p.B, k);
            var steps = new List<ConvergenceStep>();
            double prev = compute(n);
            steps.Add(new ConvergenceStep(n, prev, double.NaN));
            res.Value = prev; res.N = n;

            if (p.Eps <= 0)
            {
                // Фиксированное n: оценка погрешности по Рунге, если удвоение не выходит за предел.
                if (n * 2 <= IntegrationParams.MaxN)
                    res.ErrorEstimate = Math.Abs(compute(n * 2) - prev) / divisor;
            }
            else
            {
                res.Converged = false;
                while (n * 2 <= IntegrationParams.MaxN)
                {
                    int n2 = n * 2;
                    double cur = compute(n2);
                    double r = Math.Abs(cur - prev) / divisor;
                    steps.Add(new ConvergenceStep(n2, cur, r));
                    res.Value = cur; res.N = n2; res.ErrorEstimate = r;
                    if (r <= p.Eps) { res.Converged = true; break; }
                    prev = cur; n = n2;
                }
            }

            res.H = (p.B - p.A) / res.N;
            res.Steps = steps;
            ObserveOrder(res, steps, order);
            res.Evaluations = evals;
            res.ElapsedMs = sw.Elapsed.TotalMilliseconds;
            return res;
        }

        /// <summary>Фактический порядок: log2((I(n)−I(n/2)) / (I(2n)−I(n))) по трём последним шагам.</summary>
        private static void ObserveOrder(IntegrationResult res, IList<ConvergenceStep> s, int order)
        {
            int k = s.Count;
            if (k < 3) return;
            double d1 = s[k - 2].Value - s[k - 3].Value;
            double d2 = s[k - 1].Value - s[k - 2].Value;
            if (d2 == 0 || d1 / d2 <= 0) return;
            res.ObservedOrder = Math.Log(d1 / d2, 2);
            res.OrderDegraded = res.ObservedOrder < 0.8 * order;
        }

        /// <summary>Составная формула трапеций: h·[(f0+fn)/2 + Σ fi].</summary>
        public static double Trapezoid(Func<double, double> f, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = (f(a) + f(b)) / 2;
            for (int i = 1; i < n; i++) sum += f(a + i * h);
            return h * sum;
        }

        /// <summary>Составная формула Симпсона (n чётное): h/3·[f0 + 4Σf_нечёт + 2Σf_чёт + fn].</summary>
        public static double Simpson(Func<double, double> f, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = f(a) + f(b);
            for (int i = 1; i < n; i++) sum += (i % 2 == 1 ? 4 : 2) * f(a + i * h);
            return h / 3 * sum;
        }

        private static void Validate(IntegrationParams p)
        {
            if (double.IsNaN(p.A) || double.IsInfinity(p.A) || double.IsNaN(p.B) || double.IsInfinity(p.B))
                throw new ParamsException(ErrorCode.BadLimits);
            if (p.N < 1 || p.N > IntegrationParams.MaxN)
                throw new ParamsException(ErrorCode.BadN, IntegrationParams.MaxN);
            if (double.IsNaN(p.Eps) || double.IsInfinity(p.Eps) || p.Eps < 0)
                throw new ParamsException(ErrorCode.BadEps);
        }
    }
}
