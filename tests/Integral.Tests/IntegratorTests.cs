using System;
using Integral.Core;
using Xunit;

namespace Integral.Tests
{
    public class IntegratorTests
    {
        private static IntegrationResult Run(string f, double a, double b, Method m, int n = 100, double eps = 0)
        {
            return Integrator.Integrate(Parser.Parse(f), new IntegrationParams { A = a, B = b, N = n, Eps = eps }, m);
        }

        [Theory]
        [InlineData("x^2", 0, 1, 1.0 / 3)]
        [InlineData("x^3 - 2*x + 1", -1, 2, 3.75)]
        [InlineData("sin(x)", 0, 3.141592653589793, 2)]
        [InlineData("ln(x)", 1, 2.718281828459045, 1)]
        [InlineData("4/(1+x^2)", 0, 1, 3.141592653589793)]
        [InlineData("exp(-x^2)", 0, 2, 0.882081390762422)]
        public void BothMethods_ReachEps_OnReferenceIntegrals(string f, double a, double b, double exact)
        {
            foreach (Method m in new[] { Method.Trapezoid, Method.Simpson })
            {
                var r = Run(f, a, b, m, 4, 1e-8);
                Assert.True(r.Converged, $"{m} {f}");
                Assert.True(Math.Abs(r.Value - exact) <= 2e-8, $"{m} {f}: {r.Value}");
            }
        }

        [Fact]
        public void Simpson_IsExactForCubics()
        {
            Assert.Equal(1.0 / 3, Run("x^2", 0, 1, Method.Simpson, 2).Value, 14);
            Assert.Equal(3.75, Run("x^3 - 2*x + 1", -1, 2, Method.Simpson, 2).Value, 12);
        }

        [Fact]
        public void TaskNoteFunction_MatchesAnalyticAntiderivative()
        {
            // Замена x = t^2: ∫ 20 sin(3√x) dx = 40·(sin 3t / 9 − t cos 3t / 3), t = √x.
            double t = Math.Sqrt(10);
            double exact = 40 * (Math.Sin(3 * t) / 9 - t * Math.Cos(3 * t) / 3);
            var r = Run("20*sin(sqrt(x)*3)", 0, 10, Method.Simpson, 100, 1e-6);
            Assert.True(r.Converged);
            Assert.Equal(exact, r.Value, 4);
        }

        [Fact]
        public void ObservedOrder_SmoothFunction_MatchesTheory()
        {
            var s = Run("exp(-x^2)", 0, 2, Method.Simpson, 4, 1e-10);
            Assert.InRange(s.ObservedOrder, 3.6, 4.4);
            Assert.False(s.OrderDegraded);
            var t = Run("exp(-x^2)", 0, 2, Method.Trapezoid, 4, 1e-8);
            Assert.InRange(t.ObservedOrder, 1.8, 2.2);
            Assert.False(t.OrderDegraded);
        }

        [Fact]
        public void ObservedOrder_NonSmoothFunction_IsDegraded_AndWarns()
        {
            // √x у нуля: порядок сходимости около 1,5, оценка Рунге занижена.
            var r = Run("20*sin(sqrt(x)*3)", 0, 10, Method.Simpson, 100, 1e-6);
            Assert.InRange(r.ObservedOrder, 1.2, 1.8);
            Assert.True(r.OrderDegraded);
            Assert.True(Run("20*sin(sqrt(x)*3)", 0, 10, Method.Trapezoid, 100, 1e-5).OrderDegraded);
        }

        [Fact]
        public void ObservedOrder_NotAvailable_ForFixedN()
        {
            var r = Run("x^2", 0, 1, Method.Simpson, 10);
            Assert.True(double.IsNaN(r.ObservedOrder));
            Assert.False(r.OrderDegraded);
        }

        [Fact]
        public void ConvergenceOrders_AreTwoAndFour()
        {
            double exact = 0.882081390762422;
            double e1 = Math.Abs(Run("exp(-x^2)", 0, 2, Method.Trapezoid, 32).Value - exact);
            double e2 = Math.Abs(Run("exp(-x^2)", 0, 2, Method.Trapezoid, 64).Value - exact);
            Assert.InRange(e1 / e2, 3.6, 4.4);

            double s1 = Math.Abs(Run("exp(-x^2)", 0, 2, Method.Simpson, 16).Value - exact);
            double s2 = Math.Abs(Run("exp(-x^2)", 0, 2, Method.Simpson, 32).Value - exact);
            Assert.InRange(s1 / s2, 14.4, 17.6);
        }

        [Fact]
        public void FixedN_GivesRungeEstimate_AndNoSteps()
        {
            var r = Run("sin(x)", 0, Math.PI, Method.Trapezoid, 50);
            Assert.False(double.IsNaN(r.ErrorEstimate));
            Assert.InRange(r.ErrorEstimate, 0, 1e-2);
            Assert.Equal(50, r.N);
            Assert.True(r.Evaluations > 0);
        }

        [Fact]
        public void Simpson_OddN_IsAdjusted()
        {
            var r = Run("x", 0, 1, Method.Simpson, 7);
            Assert.True(r.NAdjusted);
            Assert.Equal(8, r.N);
            Assert.Equal(0.5, r.Value, 12);
        }

        [Fact]
        public void LimitsEqual_AndReversed()
        {
            Assert.Equal(0, Run("x", 5, 5, Method.Simpson).Value);
            Assert.Equal(-0.5, Run("x", 1, 0, Method.Trapezoid).Value, 12);
        }

        [Fact]
        public void Steps_AreRecorded_WithEps()
        {
            var r = Run("x^2", 0, 1, Method.Trapezoid, 2, 1e-6);
            Assert.True(r.Steps.Count >= 2);
            Assert.True(double.IsNaN(r.Steps[0].ErrorEstimate));
            Assert.Equal(r.N, r.Steps[r.Steps.Count - 1].N);
        }

        [Fact]
        public void NotConverged_WhenEpsUnreachable()
        {
            var r = Run("sqrt(x)", 0, 1, Method.Trapezoid, 2, 1e-15);
            Assert.False(r.Converged);
            Assert.Equal(IntegrationParams.MaxN, r.N);
        }

        [Fact]
        public void EvalError_PropagatesWithX()
        {
            var ex = Assert.Throws<EvalException>(() => Run("1/x", -1, 1, Method.Simpson, 100));
            Assert.Equal(ErrorCode.DivByZero, ex.Code);
            Assert.Equal(0, ex.X);
        }

        [Theory]
        [InlineData(0, 0.0, ErrorCode.BadN)]
        [InlineData(IntegrationParams.MaxN + 1, 0.0, ErrorCode.BadN)]
        [InlineData(10, -1.0, ErrorCode.BadEps)]
        [InlineData(10, double.NaN, ErrorCode.BadEps)]
        public void InvalidParams(int n, double eps, ErrorCode code)
        {
            var ex = Assert.Throws<ParamsException>(() => Run("x", 0, 1, Method.Simpson, n, eps));
            Assert.Equal(code, ex.Code);
        }

        [Fact]
        public void InfiniteLimit_IsError()
        {
            var ex = Assert.Throws<ParamsException>(() => Run("x", 0, double.PositiveInfinity, Method.Simpson));
            Assert.Equal(ErrorCode.BadLimits, ex.Code);
        }
    }
}
