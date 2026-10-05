using System.Drawing;
using System.Drawing.Drawing2D;

namespace Integral.App
{
    /// <summary>Значки панели инструментов: рисуются кодом в едином стиле (линия 1,8 px, один акцентный цвет).</summary>
    internal static class IconFactory
    {
        public enum Kind { New, Open, Save, Html, Calc, Graph, Help, About }

        public static Bitmap Create(Kind kind, int size)
        {
            var bmp = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                g.ScaleTransform(size / 24f, size / 24f);
                using (var pen = new Pen(Ui.Ink, 1.8f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                using (var accent = new Pen(Ui.Accent, 1.8f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                using (var soft = new SolidBrush(Ui.AccentSoft))
                using (var fill = new SolidBrush(Ui.Accent))
                    Draw(g, kind, pen, accent, soft, fill);
            }
            return bmp;
        }

        private static Icon _appIcon;

        /// <summary>Значок приложения: многоразмерный app.ico из ресурсов (tools/make_icon.py).</summary>
        public static Icon AppIcon()
        {
            if (_appIcon == null)
                using (System.IO.Stream st = typeof(IconFactory).Assembly.GetManifestResourceStream("app.ico"))
                    _appIcon = new Icon(st);
            return _appIcon;
        }

        /// <summary>Значок приложения нужного размера как изображение (для окна «О программе»).</summary>
        public static Bitmap AppImage(int size)
        {
            using (var icon = new Icon(AppIcon(), size, size)) return icon.ToBitmap();
        }

        private static void Page(Graphics g, Pen pen, Brush soft)
        {
            Point[] p = { new Point(6, 3), new Point(14, 3), new Point(19, 8), new Point(19, 21), new Point(6, 21) };
            g.FillPolygon(soft, p);
            g.DrawPolygon(pen, p);
            g.DrawLines(pen, new[] { new Point(14, 3), new Point(14, 8), new Point(19, 8) });
        }

        private static void Draw(Graphics g, Kind kind, Pen pen, Pen accent, Brush soft, Brush fill)
        {
            switch (kind)
            {
                case Kind.New:
                    Page(g, pen, soft);
                    g.DrawLine(accent, 12.5f, 12, 12.5f, 18); g.DrawLine(accent, 9.5f, 15, 15.5f, 15);
                    break;
                case Kind.Open:
                    g.FillRectangle(soft, 3, 8, 18, 12);
                    g.DrawLines(pen, new[] { new Point(3, 20), new Point(3, 6), new Point(9, 6), new Point(11, 8), new Point(19, 8), new Point(19, 11) });
                    g.DrawLines(accent, new[] { new Point(3, 20), new Point(6, 11), new Point(22, 11), new Point(19, 20), new Point(3, 20) });
                    break;
                case Kind.Save:
                    g.FillRectangle(soft, 4, 4, 16, 16);
                    g.DrawRectangle(pen, 4, 4, 16, 16);
                    g.DrawRectangle(accent, 8, 4, 8, 5); g.DrawRectangle(pen, 7, 13, 10, 7);
                    break;
                case Kind.Html:
                    Page(g, pen, soft);
                    g.DrawLines(accent, new[] { new Point(11, 12), new Point(8, 15), new Point(11, 18) });
                    g.DrawLines(accent, new[] { new Point(14, 12), new Point(17, 15), new Point(14, 18) });
                    break;
                case Kind.Calc:
                    g.FillPolygon(fill, new[] { new Point(7, 4), new Point(20, 12), new Point(7, 20) });
                    g.DrawPolygon(accent, new[] { new Point(7, 4), new Point(20, 12), new Point(7, 20) });
                    break;
                case Kind.Graph:
                    g.DrawLines(pen, new[] { new Point(4, 3), new Point(4, 20), new Point(21, 20) });
                    g.FillClosedCurve(soft, new[] { new PointF(6, 19), new PointF(10, 8), new PointF(14, 14), new PointF(19, 6), new PointF(19, 19) }, FillMode.Alternate, 0.4f);
                    g.DrawCurve(accent, new[] { new PointF(6, 19), new PointF(10, 8), new PointF(14, 14), new PointF(19, 6) }, 0.5f);
                    break;
                case Kind.Help:
                    g.DrawEllipse(pen, 3, 3, 18, 18);
                    g.DrawArc(accent, 8.5f, 7, 7, 6, 200, 250); g.DrawLine(accent, 12, 12.5f, 12, 14.5f);
                    g.FillEllipse(fill, 11f, 16.2f, 2f, 2f);
                    break;
                case Kind.About:
                    g.DrawEllipse(pen, 3, 3, 18, 18);
                    g.FillEllipse(fill, 11f, 6.8f, 2.2f, 2.2f); g.DrawLine(accent, 12, 11, 12, 17);
                    break;
            }
        }
    }
}
