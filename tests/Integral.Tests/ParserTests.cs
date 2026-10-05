using Integral.Core;
using Xunit;

namespace Integral.Tests
{
    public class ParserTests
    {
        private static double Calc(string f, double x = 0) => Evaluator.Evaluate(Parser.Parse(f), x);

        [Theory]
        [InlineData("2+3*4", 14)]
        [InlineData("(1+2)*3", 9)]
        [InlineData("2^3^2", 512)]
        [InlineData("-2^2", -4)]
        [InlineData("2^-1", 0.5)]
        [InlineData("10-4-3", 3)]
        [InlineData("100/10/5", 2)]
        [InlineData("--3", 3)]
        [InlineData("+5", 5)]
        [InlineData("1,5*2", 3)]
        [InlineData("2 * ( 3 + 4 )", 14)]
        public void Precedence_AndAssociativity(string f, double expected)
        {
            Assert.Equal(expected, Calc(f), 12);
        }

        [Fact]
        public void Variable_AndConstants()
        {
            Assert.Equal(7, Calc("x+4", 3), 12);
            Assert.Equal(System.Math.PI, Calc("pi"), 12);
            Assert.Equal(System.Math.E, Calc("E"), 12);
        }

        [Fact]
        public void Function_FromTaskNote()
        {
            Assert.Equal(20 * System.Math.Sin(System.Math.Sqrt(4) * 3), Calc("20*sin(sqrt(x)*3)", 4), 12);
        }

        [Theory]
        [InlineData("sin(x", ErrorCode.ExpectedRParen, 6)]
        [InlineData("(1+2", ErrorCode.ExpectedRParen, 5)]
        [InlineData("sqrt(x)sin(x)", ErrorCode.UnexpectedToken, 8)]
        [InlineData("2x", ErrorCode.UnexpectedToken, 2)]
        [InlineData("1+2)", ErrorCode.UnexpectedToken, 4)]
        [InlineData("2+", ErrorCode.ExpectedOperand, 3)]
        [InlineData("()", ErrorCode.ExpectedOperand, 2)]
        [InlineData("*3", ErrorCode.ExpectedOperand, 1)]
        [InlineData("sin x", ErrorCode.ExpectedLParen, 5)]
        [InlineData("sn(x)", ErrorCode.UnknownIdent, 1)]
        [InlineData("", ErrorCode.EmptyExpression, 1)]
        [InlineData("   ", ErrorCode.EmptyExpression, 1)]
        public void SyntaxErrors(string f, ErrorCode code, int pos)
        {
            var ex = Assert.Throws<FormulaException>(() => Parser.Parse(f));
            Assert.Equal(code, ex.Code);
            Assert.Equal(pos, ex.Position);
            Assert.False(string.IsNullOrWhiteSpace(ex.Message));
        }

        [Fact]
        public void TooDeepNesting_IsError_NotStackOverflow()
        {
            string f = new string('(', 5000) + "x" + new string(')', 5000);
            var ex = Assert.Throws<FormulaException>(() => Parser.Parse(f));
            Assert.Equal(ErrorCode.TooDeep, ex.Code);
        }

        [Fact]
        public void UsesX()
        {
            Assert.True(Parser.UsesX(Parser.Parse("sin(x)+1")));
            Assert.False(Parser.UsesX(Parser.Parse("2*pi")));
        }
    }
}
