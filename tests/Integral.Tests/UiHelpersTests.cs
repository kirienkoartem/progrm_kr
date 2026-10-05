using Integral.Core;
using Xunit;

namespace Integral.Tests
{
    public class UiHelpersTests
    {
        [Theory]
        [InlineData("1.5", 1.5)]
        [InlineData(" 1,5 ", 1.5)]
        [InlineData("-2e3", -2000)]
        [InlineData("0", 0)]
        public void NumberParser_Valid(string s, double expected)
        {
            Assert.True(NumberParser.TryParse(s, out double v));
            Assert.Equal(expected, v);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("abc")]
        [InlineData("1.2.3")]
        [InlineData("1e999")]
        [InlineData("NaN")]
        public void NumberParser_Invalid(string? s)
        {
            Assert.False(NumberParser.TryParse(s, out _));
        }

        [Fact]
        public void NumberParser_Int()
        {
            Assert.True(NumberParser.TryParseInt(" 42 ", out int n));
            Assert.Equal(42, n);
            Assert.False(NumberParser.TryParseInt("4.2", out _));
        }

        [Theory]
        [InlineData(0, 10, 5, 2)]
        [InlineData(0, 1, 5, 0.2)]
        [InlineData(-3.1, 3.1, 6, 1)]
        [InlineData(0, 1000, 5, 200)]
        public void NiceStep(double min, double max, int target, double expected)
        {
            Assert.Equal(expected, AxisScale.NiceStep(min, max, target), 9);
        }

        [Fact]
        public void Ticks_AreInsideRange_AndIncludeZero()
        {
            var t = AxisScale.Ticks(-3.1, 3.1, 6);
            Assert.All(t, v => Assert.InRange(v, -3.1, 3.1));
            Assert.Contains(0.0, t);
            Assert.Equal(7, t.Count);
        }

        [Fact]
        public void Ticks_DegenerateRange_DoesNotHang()
        {
            Assert.NotNull(AxisScale.Ticks(5, 5, 5));
        }
    }
}
