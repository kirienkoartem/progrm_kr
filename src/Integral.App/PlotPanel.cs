using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    /// <summary>
    /// Панель с графиком f(x) в окне «График». Данные готовит <see cref="PlotModel"/> (та же модель,
    /// что и для HTML-отчёта); панель только рисует их средствами GDI+.
    /// </summary>
    public sealed class PlotPanel : Panel
    {
        private const int Left0 = 64, Right0 = 20, Top0 = 16, Bottom0 = 36;
        private readonly Node _f;
        private readonly double _a, _b;
        private readonly int _n;
        private readonly Method _method;

        public PlotPanel(Node f, double a, double b, int n, Method method)
        {
            _f = f; _a = a; _b = b; _n = n; _method = method;
            DoubleBuffered = true;
            BackColor = Color.White;
            ResizeRedraw = true;
            Dock = DockStyle.Fill;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var plot = new Rectangle(Left0, Top0, Math.Max(10, Width - Left0 - Right0), Math.Max(10, Height - Top0 - Bottom0));
            PlotModel m = PlotModel.Build(_f, _a, _b, _n, Math.Max(50, plot.Width), Math.Max(3, plot.Width / 90), Math.Max(3, plot.Height / 50));
            Func<double, float> px = x => (float)m.MapX(x, plot.Left, plot.Width);
            Func<double, float> py = y => Clamp(m.MapY(y, plot.Top, plot.Height));

            DrawGrid(g, m, plot, px, py);
            g.SetClip(plot);
            var area = new List<PointF> { new PointF(px(m.A), py(0)) };
            foreach (PlotPoint p in m.Curve)
                if (p.X >= m.A && p.X <= m.B && p.Y.HasValue) area.Add(new PointF(px(p.X), py(p.Y.Value)));
            area.Add(new PointF(px(m.B), py(0)));
            if (area.Count > 2)
                using (var br = new SolidBrush(Color.FromArgb(46, Ui.Accent))) g.FillPolygon(br, area.ToArray());
            DrawNodes(g, m, px, py);
            using (var pen = new Pen(Ui.Ink, 2f) { LineJoin = LineJoin.Round })
            {
                var seg = new List<PointF>();
                foreach (PlotPoint p in m.Curve)
                {
                    if (p.Y.HasValue) { seg.Add(new PointF(px(p.X), py(p.Y.Value))); continue; }
                    Flush(g, pen, seg);
                }
                Flush(g, pen, seg);
            }
            using (var pen = new Pen(Ui.Accent, 1.5f) { DashStyle = DashStyle.Dash })
            {
                g.DrawLine(pen, px(m.A), plot.Top, px(m.A), plot.Bottom);
                g.DrawLine(pen, px(m.B), plot.Top, px(m.B), plot.Bottom);
            }
            g.ResetClip();
            using (var pen = new Pen(Ui.Ink)) g.DrawRectangle(pen, plot);
        }

        /// <summary>Ограничивает координату, чтобы значения у полюса не ломали отрисовку.</summary>
        private static float Clamp(double v) { return (float)(v < -10000 ? -10000 : v > 10000 ? 10000 : v); }

        private static void Flush(Graphics g, Pen pen, List<PointF> seg)
        {
            if (seg.Count > 1) g.DrawLines(pen, seg.ToArray());
            seg.Clear();
        }

        /// <summary>Узлы разбиения; для метода трапеций — ломаная через узлы.</summary>
        private void DrawNodes(Graphics g, PlotModel m, Func<double, float> px, Func<double, float> py)
        {
            var tops = new List<PointF>();
            using (var pen = new Pen(Color.FromArgb(120, Ui.Accent), 1f))
                foreach (PlotPoint p in m.Nodes)
                {
                    if (!p.Y.HasValue) continue;
                    g.DrawLine(pen, px(p.X), py(0), px(p.X), py(p.Y.Value));
                    tops.Add(new PointF(px(p.X), py(p.Y.Value)));
                }
            if (_method == Method.Trapezoid && tops.Count > 1)
                using (var tp = new Pen(Ui.Accent, 1.5f)) g.DrawLines(tp, tops.ToArray());
        }

        private static void DrawGrid(Graphics g, PlotModel m, Rectangle plot, Func<double, float> px, Func<double, float> py)
        {
            using (var grid = new Pen(Color.FromArgb(229, 231, 235)))
            using (var axis = new Pen(Color.FromArgb(107, 114, 128), 1.2f))
            using (var font = new Font("Tahoma", 8.25f))
            using (var brush = new SolidBrush(Ui.Ink))
            {
                foreach (double v in m.XTicks)
                {
                    float x = px(v);
                    g.DrawLine(v == 0 ? axis : grid, x, plot.Top, x, plot.Bottom);
                    string s = v.ToString("G5", CultureInfo.CurrentCulture);
                    SizeF sz = g.MeasureString(s, font);
                    g.DrawString(s, font, brush, x - sz.Width / 2, plot.Bottom + 4);
                }
                foreach (double v in m.YTicks)
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
