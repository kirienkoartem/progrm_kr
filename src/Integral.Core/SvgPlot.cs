using System.Collections.Generic;
using System.Text;

namespace Integral.Core
{
    /// <summary>Рисует график по <see cref="PlotModel"/> в формате SVG (для HTML-отчёта).</summary>
    public static class SvgPlot
    {
        private const int Left = 64, Right = 20, Top = 16, Bottom = 40;
        private const string Ink = "#1f2937", Accent = "#2563eb", Grid = "#e5e7eb", Axis = "#6b7280";

        /// <summary>Возвращает элемент &lt;svg&gt; заданного размера; при методе трапеций узлы соединяются ломаной.</summary>
        public static string Render(PlotModel m, int width, int height, Method method)
        {
            double pw = width - Left - Right, ph = height - Top - Bottom;
            var sb = new StringBuilder();
            sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 ").Append(width).Append(' ').Append(height)
              .Append("\" width=\"").Append(width).Append("\" height=\"").Append(height)
              .Append("\" role=\"img\" aria-label=\"График подынтегральной функции\" font-family=\"Segoe UI, Tahoma, Arial, sans-serif\" font-size=\"12\">");
            sb.Append("<defs><clipPath id=\"plot\"><rect x=\"").Append(Left).Append("\" y=\"").Append(Top)
              .Append("\" width=\"").Append(HtmlFormat.Coord(pw)).Append("\" height=\"").Append(HtmlFormat.Coord(ph)).Append("\"/></clipPath></defs>");
            sb.Append("<rect width=\"100%\" height=\"100%\" fill=\"#ffffff\"/>");

            foreach (double v in m.XTicks)
            {
                string x = HtmlFormat.Coord(m.MapX(v, Left, pw));
                Line(sb, x, Top.ToString(), x, HtmlFormat.Coord(Top + ph), v == 0 ? Axis : Grid, 1);
                sb.Append("<text x=\"").Append(x).Append("\" y=\"").Append(HtmlFormat.Coord(Top + ph + 18))
                  .Append("\" text-anchor=\"middle\" fill=\"").Append(Ink).Append("\">").Append(HtmlFormat.Num(v, 5)).Append("</text>");
            }
            foreach (double v in m.YTicks)
            {
                string y = HtmlFormat.Coord(m.MapY(v, Top, ph));
                Line(sb, Left.ToString(), y, HtmlFormat.Coord(Left + pw), y, v == 0 ? Axis : Grid, 1);
                sb.Append("<text x=\"").Append(Left - 6).Append("\" y=\"").Append(y)
                  .Append("\" text-anchor=\"end\" dominant-baseline=\"middle\" fill=\"").Append(Ink).Append("\">").Append(HtmlFormat.Num(v, 5)).Append("</text>");
            }

            sb.Append("<g clip-path=\"url(#plot)\">");
            Area(sb, m, pw, ph);
            Nodes(sb, m, pw, ph, method);
            foreach (string seg in Segments(m.Curve, m, pw, ph))
                sb.Append("<polyline fill=\"none\" stroke=\"").Append(Ink).Append("\" stroke-width=\"2\" stroke-linejoin=\"round\" points=\"").Append(seg).Append("\"/>");
            foreach (double x in new[] { m.A, m.B })
            {
                string px = HtmlFormat.Coord(m.MapX(x, Left, pw));
                sb.Append("<line x1=\"").Append(px).Append("\" y1=\"").Append(Top).Append("\" x2=\"").Append(px).Append("\" y2=\"")
                  .Append(HtmlFormat.Coord(Top + ph)).Append("\" stroke=\"").Append(Accent).Append("\" stroke-width=\"1.5\" stroke-dasharray=\"6 4\"/>");
            }
            sb.Append("</g>");
            sb.Append("<rect x=\"").Append(Left).Append("\" y=\"").Append(Top).Append("\" width=\"").Append(HtmlFormat.Coord(pw))
              .Append("\" height=\"").Append(HtmlFormat.Coord(ph)).Append("\" fill=\"none\" stroke=\"").Append(Ink).Append("\"/>");
            sb.Append("</svg>");
            return sb.ToString();
        }

        private static void Line(StringBuilder sb, string x1, string y1, string x2, string y2, string color, double w)
        {
            sb.Append("<line x1=\"").Append(x1).Append("\" y1=\"").Append(y1).Append("\" x2=\"").Append(x2).Append("\" y2=\"").Append(y2)
              .Append("\" stroke=\"").Append(color).Append("\" stroke-width=\"").Append(w.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("\"/>");
        }

        /// <summary>Закрашенная площадь между кривой и осью x на [a; b].</summary>
        private static void Area(StringBuilder sb, PlotModel m, double pw, double ph)
        {
            var pts = new StringBuilder();
            string zero = HtmlFormat.Coord(m.MapY(0, Top, ph));
            pts.Append(HtmlFormat.Coord(m.MapX(m.A, Left, pw))).Append(',').Append(zero).Append(' ');
            foreach (PlotPoint p in m.Curve)
                if (p.X >= m.A && p.X <= m.B && p.Y.HasValue)
                    pts.Append(HtmlFormat.Coord(m.MapX(p.X, Left, pw))).Append(',').Append(HtmlFormat.Coord(Clamp(m.MapY(p.Y.Value, Top, ph)))).Append(' ');
            pts.Append(HtmlFormat.Coord(m.MapX(m.B, Left, pw))).Append(',').Append(zero);
            sb.Append("<polygon fill=\"").Append(Accent).Append("\" fill-opacity=\"0.18\" points=\"").Append(pts).Append("\"/>");
        }

        /// <summary>Узлы разбиения (вертикальные отрезки) и, для метода трапеций, ломаная через узлы.</summary>
        private static void Nodes(StringBuilder sb, PlotModel m, double pw, double ph, Method method)
        {
            if (m.Nodes.Count == 0) return;
            string zero = HtmlFormat.Coord(m.MapY(0, Top, ph));
            var chain = new StringBuilder();
            foreach (PlotPoint p in m.Nodes)
            {
                if (!p.Y.HasValue) continue;
                string x = HtmlFormat.Coord(m.MapX(p.X, Left, pw)), y = HtmlFormat.Coord(Clamp(m.MapY(p.Y.Value, Top, ph)));
                sb.Append("<line x1=\"").Append(x).Append("\" y1=\"").Append(zero).Append("\" x2=\"").Append(x).Append("\" y2=\"").Append(y)
                  .Append("\" stroke=\"").Append(Accent).Append("\" stroke-opacity=\"0.5\"/>");
                chain.Append(x).Append(',').Append(y).Append(' ');
            }
            if (method == Method.Trapezoid && chain.Length > 0)
                sb.Append("<polyline fill=\"none\" stroke=\"").Append(Accent).Append("\" stroke-width=\"1.5\" points=\"").Append(chain.ToString().Trim()).Append("\"/>");
        }

        /// <summary>Делит кривую на непрерывные участки (в точках, где функция не определена, — разрыв).</summary>
        private static IEnumerable<string> Segments(IReadOnlyList<PlotPoint> curve, PlotModel m, double pw, double ph)
        {
            var seg = new StringBuilder();
            int count = 0;
            foreach (PlotPoint p in curve)
            {
                if (p.Y.HasValue)
                {
                    seg.Append(HtmlFormat.Coord(m.MapX(p.X, Left, pw))).Append(',').Append(HtmlFormat.Coord(Clamp(m.MapY(p.Y.Value, Top, ph)))).Append(' ');
                    count++;
                    continue;
                }
                if (count > 1) yield return seg.ToString().Trim();
                seg.Clear(); count = 0;
            }
            if (count > 1) yield return seg.ToString().Trim();
        }

        /// <summary>Ограничивает координату, чтобы огромные значения у полюса не ломали SVG.</summary>
        private static double Clamp(double v) => v < -10000 ? -10000 : v > 10000 ? 10000 : v;
    }
}
