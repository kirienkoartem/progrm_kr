using Integral.Core;
using Xunit;

namespace Integral.Tests
{
    public class TaskFileTests
    {
        private const string Good = "# комментарий\nfunction = 20*sin(sqrt(x)*3)\na = 0\nb = 10   # верхний предел\nmethod = both\nn = 50\neps = 1e-6\n";

        [Fact]
        public void Parse_Good()
        {
            var t = TaskFile.Parse(Good);
            Assert.Equal("20*sin(sqrt(x)*3)", t.Formula);
            Assert.Equal(0, t.A);
            Assert.Equal(10, t.B);
            Assert.Equal(MethodChoice.Both, t.Method);
            Assert.Equal(50, t.N);
            Assert.Equal(1e-6, t.Eps);
        }

        [Fact]
        public void Defaults_Bom_Crlf_CaseAndComma()
        {
            var t = TaskFile.Parse("﻿FUNCTION = x^2\r\nA = 0,5\r\nB = 1\r\n");
            Assert.Equal(0.5, t.A);
            Assert.Equal(MethodChoice.Simpson, t.Method);
            Assert.Equal(100, t.N);
            Assert.Equal(0, t.Eps);
        }

        [Fact]
        public void RussianMethodNames()
        {
            Assert.Equal(MethodChoice.Trapezoid, TaskFile.Parse("function=x\na=0\nb=1\nmethod=трапеции").Method);
        }

        [Fact]
        public void RoundTrip()
        {
            var t = TaskFile.Parse(Good);
            var t2 = TaskFile.Parse(TaskFile.ToText(t));
            Assert.Equal(t.Formula, t2.Formula);
            Assert.Equal(t.A, t2.A);
            Assert.Equal(t.B, t2.B);
            Assert.Equal(t.Method, t2.Method);
            Assert.Equal(t.N, t2.N);
            Assert.Equal(t.Eps, t2.Eps);
        }

        [Theory]
        [InlineData("", ErrorCode.FileEmpty, 0)]
        [InlineData("# только комментарий\n\n", ErrorCode.FileEmpty, 0)]
        [InlineData("function x^2\na=0\nb=1", ErrorCode.FileSyntax, 1)]
        [InlineData("function=x\nmehtod=simpson\na=0\nb=1", ErrorCode.FileUnknownKey, 2)]
        [InlineData("function=x\na=0\na=1\nb=1", ErrorCode.FileDuplicateKey, 3)]
        [InlineData("function=x\na=0", ErrorCode.FileMissingKey, 0)]
        [InlineData("a=0\nb=1", ErrorCode.FileMissingKey, 0)]
        [InlineData("function=x\na=abc\nb=1", ErrorCode.FileBadValue, 2)]
        [InlineData("function=x\na=0\nb=1\nn=0", ErrorCode.FileBadValue, 4)]
        [InlineData("function=x\na=0\nb=1\nn=2,5", ErrorCode.FileBadValue, 4)]
        [InlineData("function=x\na=0\nb=1\neps=-1", ErrorCode.FileBadValue, 4)]
        [InlineData("function=x\na=0\nb=1\nmethod=euler", ErrorCode.FileBadValue, 4)]
        [InlineData("function=x\na=0\nb=1e999", ErrorCode.FileBadValue, 3)]
        [InlineData("function=sin(x\na=0\nb=1", ErrorCode.FileBadFormula, 1)]
        public void Errors(string text, ErrorCode code, int line)
        {
            var ex = Assert.Throws<TaskFileException>(() => TaskFile.Parse(text));
            Assert.Equal(code, ex.Code);
            Assert.Equal(line, ex.Line);
            Assert.False(string.IsNullOrWhiteSpace(ex.Message));
        }

        [Fact]
        public void BadFormula_MessageHasLineAndPosition()
        {
            var ex = Assert.Throws<TaskFileException>(() => TaskFile.Parse("function=sin(x\na=0\nb=1"));
            Assert.Contains("Строка 1", ex.Message);
            Assert.Contains("6", ex.Message);
        }
    }
}
