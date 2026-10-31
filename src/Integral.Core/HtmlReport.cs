using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Integral.Core
{
    /// <summary>Исходные данные и результаты для HTML-отчёта.</summary>
    public sealed class ReportInput
    {
        public string Formula { get; set; } = "";
        public double A { get; set; }
        public double B { get; set; }
        public int N { get; set; }
        public double Eps { get; set; }
        public MethodChoice Method { get; set; }
        public Node? Function { get; set; }
        public IList<IntegrationResult> Results { get; set; } = new List<IntegrationResult>();
        public DateTime Created { get; set; } = DateTime.Now;
    }

    /// <summary>Формирование отчёта о расчёте в виде самодостаточного HTML-файла.</summary>
    public static class HtmlReport
    {
        /// <summary>Число точек в таблице значений функции.</summary>
        public const int ValuePoints = 21;

        private static string Name(Method m) => m == Method.Simpson ? "Метод Симпсона" : "Метод трапеций";
        private static string Short(Method m) => m == Method.Simpson ? "Симпсона" : "трапеций";
        private static string Title(Method m) => m == Method.Simpson ? "Симпсона" : "Трапеций";

        /// <summary>Возвращает текст HTML-документа.</summary>
        public static string Build(ReportInput r)
        {
            if (r.Function == null || r.Results.Count == 0) throw new ArgumentException("Нет результатов расчёта для отчёта.", nameof(r));
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>\n<html lang=\"ru\">\n<head>\n<meta charset=\"utf-8\">\n")
              .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n")
              .Append("<title>Отчёт: интеграл f(x) = ").Append(HtmlFormat.Esc(r.Formula)).Append("</title>\n")
              .Append("<style>").Append(HtmlReportStyle.Css).Append("</style>\n</head>\n<body>\n<main class=\"page\">\n");
            Header(sb, r);
            Cards(sb, r);
            Warnings(sb, r);
            Inputs(sb, r);
            ResultsTable(sb, r);
            Plot(sb, r);
            foreach (IntegrationResult res in r.Results)
                if (res.Steps.Count > 1) Convergence(sb, res);
            Values(sb, r);
            sb.Append("<footer>Сформировано программой «Интегрирование функций» ")
              .Append(r.Created.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture))
              .Append(". Курсовая работа по дисциплине «Основы программирования», ДонГТУ.</footer>\n");
            sb.Append("</main>\n</body>\n</html>\n");
            return sb.ToString();
        }

        private static void Header(StringBuilder sb, ReportInput r)
        {
            sb.Append("<header>\n<p class=\"eyebrow\">Интегрирование функций · отчёт о расчёте</p>\n")
              .Append("<h1>Вычисление определённого интеграла</h1>\n")
              .Append("<p class=\"meta\">Сформирован ").Append(r.Created.ToString("dd.MM.yyyy в HH:mm", CultureInfo.InvariantCulture)).Append("</p>\n")
              .Append("<div class=\"formula\"><span class=\"int\">∫</span><span class=\"lim\"><span>").Append(HtmlFormat.Num(r.B, 8))
              .Append("</span><span>").Append(HtmlFormat.Num(r.A, 8)).Append("</span></span>f(x)&nbsp;dx, &nbsp; f(x) = <code>")
              .Append(HtmlFormat.Esc(r.Formula)).Append("</code></div>\n</header>\n");
        }

        private static void Cards(StringBuilder sb, ReportInput r)
        {
            sb.Append("<section class=\"cards\">\n");
            foreach (IntegrationResult res in r.Results)
            {
                sb.Append("<div class=\"card\"><div class=\"label\">").Append(Name(res.Method)).Append("</div><div class=\"value\">")
                  .Append(HtmlFormat.Num(res.Value, 12)).Append("</div><div class=\"sub\">n = ").Append(HtmlFormat.Int(res.N));
                if (!double.IsNaN(res.ErrorEstimate)) sb.Append(" · оценка погрешности ").Append(HtmlFormat.Sci(res.ErrorEstimate));
                sb.Append("</div></div>\n");
            }
            sb.Append("</section>\n");
        }

        private static void Warnings(StringBuilder sb, ReportInput r)
        {
            var list = new List<string>();
            foreach (IntegrationResult res in r.Results)
            {
                if (res.NAdjusted) list.Add(Name(res.Method) + ": число разбиений увеличено до чётного.");
                if (!res.Converged) list.Add(Name(res.Method) + ": заданная точность не достигнута (достигнут предел n).");
                if (res.OrderDegraded)
                    list.Add(Name(res.Method) + ": фактический порядок сходимости " + HtmlFormat.Num(res.ObservedOrder, 3) +
                             " ниже теоретического — функция негладкая, реальная погрешность может быть больше оценки.");
            }
            if (list.Count == 0) return;
            sb.Append("<section class=\"warn\">\n");
            foreach (string w in list) sb.Append("<p>").Append(HtmlFormat.Esc(w)).Append("</p>\n");
            sb.Append("</section>\n");
        }

        private static void Inputs(StringBuilder sb, ReportInput r)
        {
            string method = r.Method == MethodChoice.Both ? "оба метода (трапеций и Симпсона)" : r.Method == MethodChoice.Trapezoid ? "метод трапеций" : "метод Симпсона";
            sb.Append("<section>\n<h2>Исходные данные</h2>\n<table class=\"kv\">\n");
            Row(sb, "Подынтегральная функция f(x)", "<code>" + HtmlFormat.Esc(r.Formula) + "</code>");
            Row(sb, "Нижний предел a", HtmlFormat.Num(r.A, 12));
            Row(sb, "Верхний предел b", HtmlFormat.Num(r.B, 12));
            Row(sb, "Метод", method);
            Row(sb, r.Eps > 0 ? "Начальное число разбиений n" : "Число разбиений n", HtmlFormat.Int(r.N));
            Row(sb, "Требуемая точность ε", r.Eps > 0 ? HtmlFormat.Sci(r.Eps) : "не задана (расчёт при заданном n)");
            sb.Append("</table>\n</section>\n");
        }

        private static void Row(StringBuilder sb, string key, string valueHtml)
        {
            sb.Append("<tr><th>").Append(HtmlFormat.Esc(key)).Append("</th><td>").Append(valueHtml).Append("</td></tr>\n");
        }

        private static void ResultsTable(StringBuilder sb, ReportInput r)
        {
            sb.Append("<section>\n<h2>Результаты</h2>\n<div class=\"scroll\"><table class=\"wide\">\n<tr><th>Метод</th><th class=\"n\">Интеграл</th><th class=\"n\">n</th>")
              .Append("<th class=\"n\">Шаг h</th><th class=\"n\">Оценка погрешности</th><th class=\"n\">Порядок</th>")
              .Append("<th class=\"n\">Вычислений f(x)</th><th class=\"n\">Время, мс</th></tr>\n");
            foreach (IntegrationResult res in r.Results)
                sb.Append("<tr><td>").Append(Title(res.Method)).Append("</td><td class=\"n\">").Append(HtmlFormat.Num(res.Value, 12))
                  .Append("</td><td class=\"n\">").Append(HtmlFormat.Int(res.N)).Append("</td><td class=\"n\">").Append(HtmlFormat.Num(res.H, 6))
                  .Append("</td><td class=\"n\">").Append(HtmlFormat.Sci(res.ErrorEstimate)).Append("</td><td class=\"n\">").Append(HtmlFormat.Num(res.ObservedOrder, 3))
                  .Append("</td><td class=\"n\">").Append(HtmlFormat.Int(res.Evaluations)).Append("</td><td class=\"n\">").Append(HtmlFormat.Num(res.ElapsedMs, 3))
                  .Append("</td></tr>\n");
            sb.Append("</table></div>\n</section>\n");
        }

        private static void Plot(StringBuilder sb, ReportInput r)
        {
            IntegrationResult last = r.Results[r.Results.Count - 1];
            PlotModel m = PlotModel.Build(r.Function!, r.A, r.B, last.N, 600, 8, 6);
            sb.Append("<section>\n<h2>График</h2>\n<figure>").Append(SvgPlot.Render(m, 820, 380, last.Method))
              .Append("<figcaption>График f(x) = ").Append(HtmlFormat.Esc(r.Formula)).Append("; закрашена площадь на отрезке [")
              .Append(HtmlFormat.Num(Math.Min(r.A, r.B), 8)).Append("; ").Append(HtmlFormat.Num(Math.Max(r.A, r.B), 8)).Append("]")
              .Append(m.Nodes.Count > 0 ? ", показаны узлы разбиения" : "").Append(".</figcaption></figure>\n</section>\n");
        }

        private static void Convergence(StringBuilder sb, IntegrationResult res)
        {
            sb.Append("<section>\n<h2>Таблица сходимости — метод ").Append(Short(res.Method)).Append("</h2>\n<table>\n")
              .Append("<tr><th class=\"n\">n</th><th class=\"n\">Значение интеграла</th><th class=\"n\">Оценка погрешности по Рунге</th></tr>\n");
            foreach (ConvergenceStep s in res.Steps)
                sb.Append("<tr><td class=\"n\">").Append(HtmlFormat.Int(s.N)).Append("</td><td class=\"n\">").Append(HtmlFormat.Num(s.Value, 12))
                  .Append("</td><td class=\"n\">").Append(HtmlFormat.Sci(s.ErrorEstimate)).Append("</td></tr>\n");
            sb.Append("</table>\n</section>\n");
        }

        /// <summary>Значения функции в <see cref="ValuePoints"/> равноотстоящих точках отрезка (две пары колонок).</summary>
        private static void Values(StringBuilder sb, ReportInput r)
        {
            double a = Math.Min(r.A, r.B), b = Math.Max(r.A, r.B);
            int count = a == b ? 1 : ValuePoints, half = (count + 1) / 2;
            sb.Append("<section>\n<h2>Значения функции</h2>\n<table>\n")
              .Append("<tr><th class=\"n\">x</th><th class=\"n\">f(x)</th><th class=\"n\">x</th><th class=\"n\">f(x)</th></tr>\n");
            for (int row = 0; row < half; row++)
            {
                sb.Append("<tr>");
                ValueCells(sb, r, a, b, count, row, "");
                ValueCells(sb, r, a, b, count, row + half, " sep");
                sb.Append("</tr>\n");
            }
            sb.Append("</table>\n</section>\n");
        }

        private static void ValueCells(StringBuilder sb, ReportInput r, double a, double b, int count, int i, string cls)
        {
            if (i >= count) { sb.Append("<td class=\"n").Append(cls).Append("\"></td><td></td>"); return; }
            double x = count == 1 ? a : a + (b - a) * i / (count - 1);
            string fx;
            try { fx = HtmlFormat.Num(Evaluator.Evaluate(r.Function!, x), 10); }
            catch (IntegralException) { fx = "не определена"; }
            sb.Append("<td class=\"n").Append(cls).Append("\">").Append(HtmlFormat.Num(x, 8)).Append("</td><td class=\"n\">").Append(fx).Append("</td>");
        }
    }
}
