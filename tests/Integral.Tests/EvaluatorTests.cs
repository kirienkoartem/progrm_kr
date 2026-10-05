using Integral.Core;
using Xunit;

namespace Integral.Tests
{
    public class EvaluatorTests
    {
        private static double Calc(string f, double x = 0) => Evaluator.Evaluate(Parser.Parse(f), x);

        [Theory]
        [InlineData("sin(pi/2)", 1)]
        [InlineData("cos(0)", 1)]
        [InlineData("tg(pi/4)", 1)]
        [InlineData("ctg(pi/4)", 1)]
        [InlineData("arcsin(1)", 1.5707963267948966)]
        [InlineData("arccos(1)", 0)]
        [InlineData("arctg(1)", 0.7853981633974483)]
        [InlineData("ln(e)", 1)]
        [InlineData("lg(100)", 2)]
        [InlineData("exp(0)", 1)]
        [InlineData("sqrt(9)", 3)]
        [InlineData("abs(-4)", 4)]
        [InlineData("2^10", 1024)]
        [InlineData("(-2)^3", -8)]
        public void Functions_Values(string f, double expected)
        {
            Assert.Equal(expected, Calc(f), 12);
        }

        [Theory]
        [InlineData("1/x", 0, ErrorCode.DivByZero)]
        [InlineData("ln(x)", -0.5, ErrorCode.LnDomain)]
        [InlineData("ln(x)", 0, ErrorCode.LnDomain)]
        [InlineData("lg(x)", -1, ErrorCode.LnDomain)]
        [InlineData("sqrt(x)", -1, ErrorCode.SqrtDomain)]
        [InlineData("arcsin(x)", 2, ErrorCode.AsinDomain)]
        [InlineData("arccos(x)", -1.5, ErrorCode.AsinDomain)]
        [InlineData("tg(x)", 1.5707963267948966, ErrorCode.TanPole)]
        [InlineData("ctg(x)", 0, ErrorCode.TanPole)]
        [InlineData("(-8)^(1/3)", 0, ErrorCode.PowDomain)]
        [InlineData("x^-1", 0, ErrorCode.DivByZero)]
        [InlineData("exp(x)", 1000, ErrorCode.NotFinite)]
        public void DomainErrors(string f, double x, ErrorCode code)
        {
            var ex = Assert.Throws<EvalException>(() => Calc(f, x));
            Assert.Equal(code, ex.Code);
            Assert.Equal(x, ex.X);
            Assert.False(string.IsNullOrWhiteSpace(ex.Message));
        }

        [Fact]
        public void Message_ContainsX()
        {
            var ex = Assert.Throws<EvalException>(() => Calc("ln(x)", -0.5));
            Assert.Contains("-0.5", ex.Message);
        }
    }
}
