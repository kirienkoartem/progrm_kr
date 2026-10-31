using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Integral.Core;

namespace Integral.Cli
{
    /// <summary>Результат прогона одного тестового файла.</summary>
    public sealed class CaseOutcome
    {
        public string File = "";
        public string Note = "";
        public string Expected = "";
        public string Actual = "";
        public bool Passed;
    }

    /// <summary>Прогон набора тестовых файлов по манифесту cases.tsv: «ожидаемое ↔ фактическое».</summary>
    public static class CaseRunner
    {
        /// <summary>Выполняет все случаи из папки; каждая строка манифеста: файл, вид, ожидается, допуск, пояснение.</summary>
        public static List<CaseOutcome> RunAll(string dir)
        {
            var list = new List<CaseOutcome>();
            foreach (string raw in System.IO.File.ReadAllLines(Path.Combine(dir, "cases.tsv"), Encoding.UTF8))
            {
                if (raw.Trim().Length == 0 || raw.StartsWith("#")) continue;
                string[] c = raw.Split('\t');
                list.Add(RunOne(dir, c[0], c[1], c[2], c[3], c.Length > 4 ? c[4] : ""));
            }
            return list;
        }

        private static CaseOutcome RunOne(string dir, string file, string kind, string expected, string tolText, string note)
        {
            var o = new CaseOutcome { File = file, Note = note };
            var inv = CultureInfo.InvariantCulture;
            try
            {
                IntegrationTask t = TaskFile.Parse(System.IO.File.ReadAllText(Path.Combine(dir, file), Encoding.UTF8));
                Node f = Parser.Parse(t.Formula);
                var p = new IntegrationParams { A = t.A, B = t.B, N = t.N, Eps = t.Eps };
                double exp = kind == "ok" ? double.Parse(expected, inv) : 0;
                double tol = kind == "ok" ? double.Parse(tolText, inv) : 0;
                var parts = new List<string>();
                bool all = true;
                foreach (Method m in new[] { Method.Trapezoid, Method.Simpson })
                {
                    if ((m == Method.Trapezoid && t.Method == MethodChoice.Simpson) ||
                        (m == Method.Simpson && t.Method == MethodChoice.Trapezoid)) continue;
                    double v = Integrator.Integrate(f, p, m).Value;
                    all &= Math.Abs(v - exp) <= tol;
                    parts.Add((m == Method.Simpson ? "Симпсон " : "трапеции ") + v.ToString("G10", inv));
                }
                o.Expected = kind == "ok" ? exp.ToString("G10", inv) + " (допуск " + tolText + ")" : "ошибка " + expected;
                o.Actual = string.Join("; ", parts);
                o.Passed = kind == "ok" && all;
            }
            catch (IntegralException ex)
            {
                o.Expected = kind == "ok" ? expected : "ошибка " + expected;
                o.Actual = "ошибка " + ex.Code + ": " + ex.Message;
                o.Passed = kind == "error" && ex.Code.ToString() == expected;
            }
            return o;
        }

        /// <summary>Протокол в виде таблицы Markdown.</summary>
        public static string Protocol(List<CaseOutcome> list)
        {
            var sb = new StringBuilder("| № | Файл | Что проверяется | Ожидается | Получено | Итог |\n|---|---|---|---|---|---|\n");
            int i = 1;
            foreach (CaseOutcome o in list)
                sb.Append("| ").Append(i++).Append(" | ").Append(o.File).Append(" | ").Append(o.Note).Append(" | ")
                  .Append(o.Expected).Append(" | ").Append(o.Actual.Replace("|", "/")).Append(" | ").Append(o.Passed ? "верно" : "НЕВЕРНО").Append(" |\n");
            return sb.ToString();
        }
    }
}
