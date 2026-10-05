using System;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    // Снимок данных расчёта и отслеживание устаревших результатов.
    public partial class CalcForm
    {
        private IntegrationTask _task;
        private bool _stale;

        /// <summary>Исходные данные, по которым выполнен последний расчёт.</summary>
        public IntegrationTask LastTask { get { return _task; } }

        /// <summary>Поля изменены после расчёта: результаты относятся к прежним данным.</summary>
        public bool IsStale { get { return _stale; } }

        private const string StaleText = "Исходные данные изменены — результаты относятся к предыдущему расчёту. Нажмите «Вычислить» (F5).";

        /// <summary>Подписывает поля ввода на отслеживание изменений.</summary>
        private void WatchInputs()
        {
            foreach (TextBox t in new[] { txtFormula, txtLowerLimit, txtUpperLimit, txtN, txtEps })
                t.TextChanged += (s, e) => MarkStale();
            cmbMethod.SelectedIndexChanged += (s, e) => MarkStale();
        }

        private void MarkStale()
        {
            if (!HasResult || _stale) return;
            _stale = true;
            lblWarning.Text = StaleText + (lblWarning.Text.Length > 0 ? " " + lblWarning.Text : "");
            Report("Данные изменены");
        }

        /// <summary>Запоминает данные успешного расчёта.</summary>
        private void Remember(IntegrationTask task)
        {
            _task = task;
            _stale = false;
        }

        /// <summary>Данные для HTML-отчёта по последнему расчёту.</summary>
        public ReportInput ToReportInput()
        {
            return new ReportInput
            {
                Formula = _task.Formula, A = _task.A, B = _task.B, N = _task.N, Eps = _task.Eps, Method = _task.Method,
                Function = _node, Results = _results, Created = DateTime.Now,
            };
        }
    }
}
