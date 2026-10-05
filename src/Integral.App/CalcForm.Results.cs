using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    // Вывод результатов расчёта в таблицы окна «Расчёт».
    public partial class CalcForm
    {
        private static string MethodName(Method m) { return m == Method.Simpson ? "Симпсона" : "Трапеций"; }

        private static string Fmt(double v, string format)
        {
            return double.IsNaN(v) ? "-" : v.ToString(format, CultureInfo.CurrentCulture);
        }

        private void SetupColumns()
        {
            AddColumn(dgvSummary, "Метод", 11); AddColumn(dgvSummary, "Интеграл", 19); AddColumn(dgvSummary, "n", 10);
            AddColumn(dgvSummary, "h", 11); AddColumn(dgvSummary, "Погрешность", 14);
            AddColumn(dgvSummary, "Порядок", 10); AddColumn(dgvSummary, "Вычислений f(x)", 14); AddColumn(dgvSummary, "Время, мс", 11);
            AddColumn(dgvConvergence, "n", 20); AddColumn(dgvConvergence, "Значение интеграла", 40); AddColumn(dgvConvergence, "Оценка погрешности", 40);
            foreach (DataGridViewColumn c in dgvSummary.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
            foreach (DataGridViewColumn c in dgvConvergence.Columns) c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private static void AddColumn(DataGridView g, string title, float weight)
        {
            var col = new DataGridViewTextBoxColumn { HeaderText = title, FillWeight = weight };
            if (g.Columns.Count > 0) col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            g.Columns.Add(col);
        }

        private void ShowResults()
        {
            dgvSummary.Rows.Clear();
            var adjusted = new List<string>(); var notConverged = new List<string>(); var degraded = new List<string>();
            foreach (IntegrationResult r in _results)
            {
                dgvSummary.Rows.Add(MethodName(r.Method), r.Value.ToString("G12", CultureInfo.CurrentCulture),
                    r.N.ToString("N0", CultureInfo.CurrentCulture), r.H.ToString("G6", CultureInfo.CurrentCulture),
                    Fmt(r.ErrorEstimate, "E2"), Fmt(r.ObservedOrder, "F2"),
                    r.Evaluations.ToString("N0", CultureInfo.CurrentCulture), r.ElapsedMs.ToString("F1", CultureInfo.CurrentCulture));
                if (r.NAdjusted) adjusted.Add(MethodName(r.Method));
                if (!r.Converged) notConverged.Add(MethodName(r.Method));
                if (r.OrderDegraded) degraded.Add(MethodName(r.Method));
            }
            var warn = new StringBuilder();
            if (adjusted.Count > 0) warn.Append("Метод Симпсона: n увеличено до чётного. ");
            if (notConverged.Count > 0) warn.Append("Заданная точность не достигнута (" + string.Join(", ", notConverged) + "). ");
            if (degraded.Count > 0)
                warn.Append("Функция негладкая (" + string.Join(", ", degraded) + "): порядок сходимости ниже теоретического, реальная погрешность больше оценки.");
            lblWarning.Text = warn.ToString().Trim();
            dgvSummary.ClearSelection();
            if (dgvSummary.Rows.Count > 0) dgvSummary.Rows[dgvSummary.Rows.Count - 1].Selected = true;
            ShowSteps();
        }

        private void ShowSteps()
        {
            dgvConvergence.Rows.Clear();
            if (dgvSummary.SelectedRows.Count == 0) return;
            int i = dgvSummary.SelectedRows[0].Index;
            if (i < 0 || i >= _results.Count) return;
            foreach (ConvergenceStep s in _results[i].Steps)
                dgvConvergence.Rows.Add(s.N.ToString("N0", CultureInfo.CurrentCulture), s.Value.ToString("G12", CultureInfo.CurrentCulture), Fmt(s.ErrorEstimate, "E2"));
            lblConvergence.Text = "Таблица сходимости: метод " + MethodName(_results[i].Method).ToLowerInvariant() +
                (_results[i].Steps.Count < 2 ? " (при заданном n не строится)" : "");
        }
    }
}
