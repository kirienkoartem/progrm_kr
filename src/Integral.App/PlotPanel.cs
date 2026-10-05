using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    /// <summary>Панель с графиком f(x): оси с подписями, сетка, площадь под кривой на [a; b], узлы разбиения.</summary>
    public sealed class PlotPanel : Panel
    {
        private const int Left0 = 64, Right0 = 20, Top0 = 16, Bottom0 = 36, MaxNodesShown = 64;
        private readonly Node _f;
        private readonly double _a, _b;
        private readonly int _n;
        private readonly Method _method;

        public PlotPanel(Node f, double a, double b, int n, Method method)
        {
            _f = f; _a = Math.Min(a, b); _b = Math.Max(a, b); _n = n; _method = method;
            DoubleBuffered = true;
            BackColor = Color.White;
            ResizeRedraw = true;
            Dock = DockStyle.Fill;
        }

        private static double? Eval(Node f, double x)
        {
            try { return Evaluator.Evaluate(f, x); }
            catch (IntegralException) { return null; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var plot = new Rectangle(Left0, Top0, Math.Max(10, Width - Left0 - Right0), Math.Max(10, Height - Top0 - Bottom0));

            double pad = (_b - _a) * 0.08; if (pad <= 0) pad = 1;
            double x0 = _a - pad, x1 = _b + pad;
            var pts = new List<PointF>(); var xs = new List<double>(); var ys = new List<double?>();
            int samples = Math.Max(50, plot.Width);
            double ymin = 0, ymax = 0;
            for (int i = 0; i <= samples; i++)
            {
                double x = x0 + (x1 - x0) * i / samples;
                double? y = Eval(_f, x);
                xs.Add(x); ys.Add(y);
                if (y.HasValue) { ymin = Math.Min(ymin, y.Value); ymax = Math.Max(ymax, y.Value); }
            }
            double ypad = (ymax - ymin) * 0.08; if (ypad <= 0) ypad = 1;
            ymin -= ypad; ymax += ypad;
            Func<double, float> px = x => (float)(plot.Left + (x - x0) / (x1 - x0) * plot.Width);
            Func<double, float> py = y => (float)(plot.Bottom - (y - ymin) / (ymax - ymin) * plot.Height);

            DrawGrid(g, plot, x0, x1, ymin, ymax, px, py);
            g.SetClip(plot);

            // площадь под кривой на [a; b]
            var area = new List<PointF> { new PointF(px(_a), py(0)) };
            for (int i = 0; i < xs.Count; i++)
                if (xs[i] >= _a && xs[i] <= _b && ys[i].HasValue) area.Add(new PointF(px(xs[i]), py(ys[i].Value)));
            area.Add(new PointF(px(_b), py(0)));
            if (area.Count > 2)
                using (var br = new SolidBrush(Color.FromArgb(70, Ui.Accent))) g.FillPolygon(br, area.ToArray());

            DrawNodes(g, px, py);

            // кривая (с разрывами в точках, где функция не определена)
            using (var pen = new Pen(Ui.Ink, 2f) { LineJoin = LineJoin.Round })
            {
                var seg = new List<PointF>();
                for (int i = 0; i < xs.Count; i++)
                {
                    if (ys[i].HasValue && Math.Abs(py(ys[i].Value)) < 1e6) seg.Add(new PointF(px(xs[i]), py(ys[i].Value)));
                    else Flush(g, pen, seg);
                }
                Flush(g, pen, seg);
            }
            using (var pen = new Pen(Ui.Accent, 1.5f) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(pen, px(_a), plot.Top, px(_a), plot.Bottom);
                g.DrawLine(pen, px(_b), plot.Top, px(_b), plot.Bottom);
            }
            g.ResetClip();
            using (var pen = new Pen(Ui.Ink)) g.DrawRectangle(pen, plot);
        }

        private static void Flush(Graphics g, Pen pen, List<PointF> seg)
        {
            if (seg.Count > 1) g.DrawLines(pen, seg.ToArray());
            seg.Clear();
        }

        private void DrawNodes(Graphics g, Func<double, float> px, Func<double, float> py)
        {
            if (_n > MaxNodesShown || _n < 1) return;
            using (var pen = new Pen(Color.FromArgb(120, Ui.Accent), 1f))
            {
                var tops = new List<PointF>();
                for (int i = 0; i <= _n; i++)
                {
                    double x = _a + (_b - _a) * i / _n;
                    double? y = Eval(_f, x);
                    if (!y.HasValue) continue;
                    g.DrawLine(pen, px(x), py(0), px(x), py(y.Value));
                    tops.Add(new PointF(px(x), py(y.Value)));
                }
                if (_method == Method.Trapezoid && tops.Count > 1)
                    using (var tp = new Pen(Ui.Accent, 1.5f)) g.DrawLines(tp, tops.ToArray());
            }
        }

        private void DrawGrid(Graphics g, Rectangle plot, double x0, double x1, double y0, double y1, Func<double, float> px, Func<double, float> py)
        {
            using (var grid = new Pen(Color.FromArgb(225, 228, 232)))
            using (var axis = new Pen(Color.FromArgb(120, 125, 135), 1.2f))
            using (var font = new Font("Tahoma", 8.25f))
            using (var brush = new SolidBrush(Ui.Ink))
            {
                foreach (double v in AxisScale.Ticks(x0, x1, Math.Max(3, plot.Width / 90)))
                {
                    float x = px(v);
                    g.DrawLine(v == 0 ? axis : grid, x, plot.Top, x, plot.Bottom);
                    string s = v.ToString("G5", CultureInfo.CurrentCulture);
                    SizeF sz = g.MeasureString(s, font);
                    g.DrawString(s, font, brush, x - sz.Width / 2, plot.Bottom + 4);
                }
                foreach (double v in AxisScale.Ticks(y0, y1, Math.Max(3, plot.Height / 50)))
                {
                    float y = py(v);
                    g.DrawLine(v == 0 ? axis : grid, plot.Left, y, plot.Right, y);
                    string s = v.ToString("G5", CultureInfo.CurrentCulture);
                    SizeF sz = g.MeasureString(s, font);
                    g.DrawString(s, font, brush, plot.Left - sz.Width - 4, y - sz.Height / 2);
                }
            }
        }
    }
}
