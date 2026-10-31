using System.IO;
using Integral.Cli;
using Xunit;

namespace Integral.Tests
{
    /// <summary>Тестовый пример курсовой: каждый файл из data/tests даёт ожидаемое значение или ожидаемую ошибку.</summary>
    public class CasesTests
    {
        private static string Dir()
        {
            string d = Directory.GetCurrentDirectory();
            while (!Directory.Exists(Path.Combine(d, "data", "tests"))) d = Directory.GetParent(d)!.FullName;
            return Path.Combine(d, "data", "tests");
        }

        [Fact]
        public void AllCases_MatchExpected()
        {
            var all = CaseRunner.RunAll(Dir());
            Assert.True(all.Count >= 20);
            foreach (CaseOutcome o in all)
                Assert.True(o.Passed, o.File + ": ожидалось " + o.Expected + ", получено " + o.Actual);
        }
    }
}
