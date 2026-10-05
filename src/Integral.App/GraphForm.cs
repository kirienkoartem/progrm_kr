using System.Drawing;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    /// <summary>Дочернее окно «График»: кривая, площадь под ней и узлы разбиения.</summary>
    public sealed class GraphForm : Form
    {
        /// <summary>Окно расчёта, из которого построен график.</summary>
        public CalcForm Source { get; private set; }

        public GraphForm(CalcForm source, string title, string formula, Node f, double a, double b, int n, Method method)
        {
            Source = source;
            source.FormClosed += (s, e) => Close();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Ui.BaseFont();
            Text = "График - " + title;
            Icon = IconFactory.AppIcon();
            ClientSize = new Size(720, 480);
            MinimumSize = new Size(360, 260);
            var caption = new Label
            {
                Text = "f(x) = " + formula + "     x от " + System.Math.Min(a, b).ToString("G6") + " до " + System.Math.Max(a, b).ToString("G6") +
                       (method == Method.Trapezoid ? "     трапеции" : "     Симпсон") + ", n = " + n,
                Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(Ui.GapUnrelated, 0, 0, 0),
            };
            Controls.Add(new PlotPanel(f, a, b, n, method));
            Controls.Add(caption);
        }
    }
}
