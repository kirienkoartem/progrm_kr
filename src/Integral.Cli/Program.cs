using System;
using System.Globalization;
using System.IO;
using System.Text;
using Integral.Core;

namespace Integral.Cli
{
    /// <summary>Консольная обвязка ядра: расчёт по файлу или по аргументам командной строки.</summary>
    public static class Program
    {
        private const string Usage =
            "Использование:\n" +
            "  integ_cli <файл.txt> [--html отчёт.html]\n" +
            "  integ_cli \"<формула>\" <a> <b> [--method trapezoid|simpson|both] [--n N] [--eps E] [--html отчёт.html]\n";

        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            try
            {
                string? html = null;
                int hi = Array.IndexOf(args, "--html");
                if (hi >= 0 && hi + 1 < args.Length)
                {
                    html = args[hi + 1];
                    var rest = new System.Collections.Generic.List<string>(args);
                    rest.RemoveRange(hi, 2);
                    args = rest.ToArray();
                }
                IntegrationTask task = args.Length == 1 ? TaskFile.Parse(File.ReadAllText(args[0], Encoding.UTF8))
                                     : args.Length >= 3 ? FromArgs(args)
                                     : null!;
                if (task == null) { Console.WriteLine(Usage); return 2; }
                Run(task, html);
                return 0;
            }
            catch (IntegralException ex)
            {
                Console.Error.WriteLine("Ошибка: " + ex.Message);
                return 1;
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine("Ошибка файла: " + ex.Message);
                return 1;
            }
        }

        private static IntegrationTask FromArgs(string[] a)
        {
            var t = new IntegrationTask { Formula = a[0] };
            t.A = Number(a[1]);
            t.B = Number(a[2]);
            for (int i = 3; i + 1 < a.Length; i += 2)
            {
                switch (a[i])
                {
                    case "--method": t.Method = (MethodChoice)Enum.Parse(typeof(MethodChoice), a[i + 1], true); break;
                    case "--n": t.N = int.Parse(a[i + 1], CultureInfo.InvariantCulture); break;
                    case "--eps": t.Eps = Number(a[i + 1]); break;
                }
            }
            return t;
        }

        private static double Number(string s) => double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);

        private static void Run(IntegrationTask t, string? html)
        {
            Node f = Parser.Parse(t.Formula);
            var p = new IntegrationParams { A = t.A, B = t.B, N = t.N, Eps = t.Eps };
            Console.WriteLine("f(x) = " + t.Formula);
            Console.WriteLine("Пределы: [" + t.A.ToString(CultureInfo.InvariantCulture) + "; " + t.B.ToString(CultureInfo.InvariantCulture) + "]");
            var results = new System.Collections.Generic.List<IntegrationResult>();
            if (t.Method != MethodChoice.Simpson) results.Add(Integrator.Integrate(f, p, Method.Trapezoid));
            if (t.Method != MethodChoice.Trapezoid) results.Add(Integrator.Integrate(f, p, Method.Simpson));
            foreach (IntegrationResult r in results) Print(r);
            if (html == null) return;
            var input = new ReportInput { Formula = t.Formula, A = t.A, B = t.B, N = t.N, Eps = t.Eps, Method = t.Method, Function = f, Results = results };
            File.WriteAllText(html, HtmlReport.Build(input), new UTF8Encoding(true));
            Console.WriteLine();
            Console.WriteLine("Отчёт сохранён: " + html);
        }

        private static void Print(IntegrationResult r)
        {
            var c = CultureInfo.InvariantCulture;
            Console.WriteLine();
            Console.WriteLine(r.Method == Method.Simpson ? "Метод Симпсона" : "Метод трапеций");
            Console.WriteLine("  Интеграл            = " + r.Value.ToString("G15", c));
            Console.WriteLine("  n                   = " + r.N + (r.NAdjusted ? " (увеличено до чётного)" : ""));
            Console.WriteLine("  h                   = " + r.H.ToString("G6", c));
            Console.WriteLine("  Оценка погрешности  = " + (double.IsNaN(r.ErrorEstimate) ? "—" : r.ErrorEstimate.ToString("E2", c)));
            Console.WriteLine("  Вычислений f(x)     = " + r.Evaluations);
            Console.WriteLine("  Время               = " + r.ElapsedMs.ToString("F2", c) + " мс");
            if (!double.IsNaN(r.ObservedOrder)) Console.WriteLine("  Фактический порядок = " + r.ObservedOrder.ToString("F2", c));
            if (r.OrderDegraded) Console.WriteLine("  ВНИМАНИЕ: функция негладкая, порядок сходимости ниже теоретического — реальная погрешность больше оценки.");
            if (!r.Converged) Console.WriteLine("  ВНИМАНИЕ: заданная точность не достигнута (достигнут предел n).");
            if (r.Steps.Count > 1)
            {
                Console.WriteLine("  Таблица сходимости:");
                foreach (ConvergenceStep s in r.Steps)
                    Console.WriteLine("    n = " + s.N.ToString().PadLeft(8) + "   I = " + s.Value.ToString("G12", c).PadRight(18)
                        + " R = " + (double.IsNaN(s.ErrorEstimate) ? "—" : s.ErrorEstimate.ToString("E2", c)));
            }
        }
    }
}
