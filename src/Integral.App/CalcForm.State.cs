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

        /// <summary>Подсказка в пустой области результатов.</summary>
        private void ShowEmptyHint()
        {
            lblWarning.ForeColor = System.Drawing.SystemColors.GrayText;
            lblWarning.Text = "Введите формулу и пределы, затем нажмите «Вычислить» (F5 или Enter).";
        }

        private void MarkStale()
        {
            if (!HasResult || _stale) return;
            _stale = true;
            lblWarning.ForeColor = Ui.Warning;
            lblWarning.Text = StaleText + (lblWarning.Text.Length > 0 ? " " + lblWarning.Text : "");
            Report("Данные изменены");
        }

        /// <summary>Запоминает данные успешного расчёта.</summary>
        private void Remember(IntegrationTask task)
        {
            _task = task;
            _stale = false;
        }

        /// <summary>Раздел справки для поля, в котором стоит курсор (контекстная справка по F1).</summary>
        public string HelpTopic
        {
            get
            {
                Control c = ActiveControl;
                while (c is ContainerControl && ((ContainerControl)c).ActiveControl != null) c = ((ContainerControl)c).ActiveControl;
                if (c == txtFormula || c == txtLowerLimit || c == txtUpperLimit) return HelpContent.Formula;
                if (c == cmbMethod || c == txtN || c == txtEps || c == btnCalculate) return HelpContent.Methods;
                return HelpContent.Windows;
            }
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
