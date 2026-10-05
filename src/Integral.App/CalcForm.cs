using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    /// <summary>Дочернее окно «Расчёт»: ввод формулы, пределов, метода и точности; результаты и таблица сходимости.</summary>
    public partial class CalcForm : Form
    {
        private readonly List<IntegrationResult> _results = new List<IntegrationResult>();
        private Node _node;
        private IntegrationParams _params;

        /// <summary>Изменился статус расчёта (для строки состояния главного окна).</summary>
        public event EventHandler<string> StatusChanged;
        /// <summary>Выбран другой метод (для меню «Расчёт — Метод»).</summary>
        public event EventHandler MethodChanged;

        public CalcForm(string title)
        {
            InitializeComponent();
            Text = title;
            Icon = IconFactory.AppIcon();
            cmbMethod.SelectedIndex = 1;
            LoadTask(new IntegrationTask { Formula = "x^2", A = 0, B = 1, N = 100 });
            btnCalculate.Click += (s, e) => Calculate();
            AcceptButton = btnCalculate;
            cmbMethod.SelectedIndexChanged += (s, e) => { if (MethodChanged != null) MethodChanged(this, EventArgs.Empty); };
            dgvSummary.SelectionChanged += (s, e) => ShowSteps();
            SetupColumns();
            Hint(txtFormula, "Формула подынтегральной функции f(x), например 20*sin(sqrt(x)*3)");
            Hint(txtLowerLimit, "Нижний предел интегрирования a");
            Hint(txtUpperLimit, "Верхний предел интегрирования b");
            Hint(cmbMethod, "Метод численного интегрирования");
            Hint(txtN, "Число разбиений отрезка [a; b] (начальное, если задана точность)");
            Hint(txtEps, "Требуемая точность; пусто - расчёт при заданном n");
            Hint(btnCalculate, "Выполнить расчёт (F5)");
        }

        /// <summary>Подсказка элемента: показывается в строке состояния главного окна.</summary>
        public event EventHandler<string> HintChanged;

        private void Hint(Control c, string text)
        {
            toolTip.SetToolTip(c, text);
            c.MouseEnter += (s, e) => { if (HintChanged != null) HintChanged(this, text); };
            c.MouseLeave += (s, e) => { if (HintChanged != null) HintChanged(this, ""); };
        }

        /// <summary>Выбранный метод: 0 - трапеций, 1 - Симпсона, 2 - оба.</summary>
        public int MethodIndex
        {
            get { return cmbMethod.SelectedIndex; }
            set { cmbMethod.SelectedIndex = value; }
        }

        public bool HasResult { get { return _results.Count > 0 && _node != null; } }
        public Node Function { get { return _node; } }
        public IntegrationParams Params { get { return _params; } }
        public IList<IntegrationResult> Results { get { return _results; } }
        public string Formula { get { return txtFormula.Text.Trim(); } }

        /// <summary>Заполняет поля ввода из задачи (файл исходных данных).</summary>
        public void LoadTask(IntegrationTask t)
        {
            txtFormula.Text = t.Formula;
            txtLowerLimit.Text = t.A.ToString("R", CultureInfo.CurrentCulture);
            txtUpperLimit.Text = t.B.ToString("R", CultureInfo.CurrentCulture);
            cmbMethod.SelectedIndex = (int)t.Method;
            txtN.Text = t.N.ToString(CultureInfo.InvariantCulture);
            txtEps.Text = t.Eps > 0 ? t.Eps.ToString("R", CultureInfo.CurrentCulture) : "";
            ClearResults();
        }

        /// <summary>Читает и проверяет поля. При ошибке показывает сообщение и ставит фокус в поле.</summary>
        public bool TryReadTask(out IntegrationTask task, out Node node)
        {
            task = new IntegrationTask(); node = null;
            string formula = txtFormula.Text.Trim();
            try { node = Parser.Parse(formula); }
            catch (FormulaException ex) { FailFormula(ex); return false; }
            double a, b, eps = 0; int n;
            if (!NumberParser.TryParse(txtLowerLimit.Text, out a)) return Fail(txtLowerLimit, "Поле «a»: введите число, например 0 или -1,5.");
            if (!NumberParser.TryParse(txtUpperLimit.Text, out b)) return Fail(txtUpperLimit, "Поле «b»: введите число, например 10.");
            if (!NumberParser.TryParseInt(txtN.Text, out n) || n < 1 || n > IntegrationParams.MaxN)
                return Fail(txtN, "Поле «n»: введите целое число от 1 до " + IntegrationParams.MaxN.ToString("N0", CultureInfo.CurrentCulture) + ".");
            if (txtEps.Text.Trim().Length > 0 && (!NumberParser.TryParse(txtEps.Text, out eps) || eps < 0))
                return Fail(txtEps, "Поле «ε»: введите неотрицательное число, например 1e-6, или оставьте поле пустым.");
            task.Formula = formula; task.A = a; task.B = b; task.N = n; task.Eps = eps;
            task.Method = (MethodChoice)Math.Max(0, cmbMethod.SelectedIndex);
            return true;
        }

        private bool Fail(Control c, string message)
        {
            MessageBox.Show(this, message, AppInfo.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            c.Focus();
            TextBox t = c as TextBox;
            if (t != null) t.SelectAll();
            Report("Ошибка ввода");
            return false;
        }

        private void FailFormula(FormulaException ex)
        {
            MessageBox.Show(this, ex.Message, AppInfo.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtFormula.Focus();
            txtFormula.Select(Math.Max(0, Math.Min(ex.Position - 1, txtFormula.TextLength)), 0);
            Report("Ошибка в формуле");
        }

        private void Report(string text) { if (StatusChanged != null) StatusChanged(this, text); }

        private void ClearResults()
        {
            _results.Clear(); _node = null;
            dgvSummary.Rows.Clear(); dgvConvergence.Rows.Clear();
            lblWarning.Text = "";
        }

        /// <summary>Выполняет расчёт выбранным методом (или обоими) и выводит результаты.</summary>
        public void Calculate()
        {
            IntegrationTask task; Node node;
            if (!TryReadTask(out task, out node)) return;
            var p = new IntegrationParams { A = task.A, B = task.B, N = task.N, Eps = task.Eps };
            var list = new List<IntegrationResult>();
            try
            {
                Cursor = Cursors.WaitCursor;
                if (task.Method != MethodChoice.Simpson) list.Add(Integrator.Integrate(node, p, Method.Trapezoid));
                if (task.Method != MethodChoice.Trapezoid) list.Add(Integrator.Integrate(node, p, Method.Simpson));
            }
            catch (IntegralException ex)
            {
                ClearResults();
                MessageBox.Show(this, ex.Message, AppInfo.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Report("Ошибка вычисления");
                return;
            }
            finally { Cursor = Cursors.Default; }
            _node = node; _params = p; _results.Clear(); _results.AddRange(list);
            ShowResults();
            IntegrationResult last = list[list.Count - 1];
            Report("Готово: " + MethodName(last.Method) + ", I = " + last.Value.ToString("G10", CultureInfo.CurrentCulture));
        }
    }
}
