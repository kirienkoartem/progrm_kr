using System.Collections.Generic;
using Integral.Core;
using Xunit;

namespace Integral.Tests
{
    public class LexerTests
    {
        private static List<Token> All(string src)
        {
            var lx = new Lexer(src);
            var list = new List<Token>();
            Token t;
            do { t = lx.Next(); list.Add(t); } while (t.Type != TokenType.End);
            return list;
        }

        [Fact]
        public void Operators_AndParens()
        {
            var t = All("+-*/^()");
            Assert.Equal(new[] { TokenType.Plus, TokenType.Minus, TokenType.Multiply, TokenType.Divide,
                TokenType.Power, TokenType.LeftParen, TokenType.RightParen, TokenType.End },
                t.ConvertAll(x => x.Type).ToArray());
        }

        [Theory]
        [InlineData("12", 12.0)]
        [InlineData("1.5", 1.5)]
        [InlineData("1,5", 1.5)]
        [InlineData("2e-3", 0.002)]
        [InlineData("2,5E+2", 250.0)]
        public void Numbers(string src, double expected)
        {
            var t = All(src);
            Assert.Equal(TokenType.Number, t[0].Type);
            Assert.Equal(expected, t[0].Value, 12);
        }

        [Fact]
        public void CaseInsensitive_AndPositions()
        {
            var t = All("2,5e-3*X");
            Assert.Equal(TokenType.Number, t[0].Type);
            Assert.Equal(1, t[0].Position);
            Assert.Equal(TokenType.Multiply, t[1].Type);
            Assert.Equal(TokenType.Variable, t[2].Type);
            Assert.Equal(8, t[2].Position);
            Assert.Equal("sin", All("SIN")[0].Name);
        }

        [Fact]
        public void Words_Functions_Constants()
        {
            Assert.Equal(TokenType.Function, All("arcsin")[0].Type);
            var pi = All("pi")[0];
            Assert.Equal(TokenType.Constant, pi.Type);
            Assert.Equal(System.Math.PI, pi.Value);
            Assert.Equal(System.Math.E, All("e")[0].Value);
        }

        [Fact]
        public void ExponentWithoutDigits_IsSeparateConstant()
        {
            var t = All("2e");
            Assert.Equal(TokenType.Number, t[0].Type);
            Assert.Equal(TokenType.Constant, t[1].Type);
        }

        [Theory]
        [InlineData("@", ErrorCode.BadChar, 1)]
        [InlineData("1+#", ErrorCode.BadChar, 3)]
        [InlineData(".5", ErrorCode.BadNumber, 1)]
        [InlineData("1.", ErrorCode.BadNumber, 1)]
        [InlineData("sn(x)", ErrorCode.UnknownIdent, 1)]
        [InlineData("x+foo", ErrorCode.UnknownIdent, 3)]
        public void Errors(string src, ErrorCode code, int pos)
        {
            var ex = Assert.Throws<FormulaException>(() => All(src));
            Assert.Equal(code, ex.Code);
            Assert.Equal(pos, ex.Position);
        }

        [Fact]
        public void EmptyAndSpaces_GiveEnd()
        {
            Assert.Equal(TokenType.End, All("   ")[0].Type);
        }
    }
}
