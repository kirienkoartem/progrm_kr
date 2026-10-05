using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Integral.App
{
    partial class CalcForm
    {
        private IContainer components = null;

        private GroupBox grpInput;
        private Label lblFormula, lblLowerLimit, lblUpperLimit, lblFormulaHint;
        private TextBox txtFormula, txtLowerLimit, txtUpperLimit;
        private GroupBox grpMethod;
        private Label lblMethod, lblN, lblEps, lblMethodHint;
        private ComboBox cmbMethod;
        private TextBox txtN, txtEps;
        private Button btnCalculate;
        private GroupBox grpResult;
        private DataGridView dgvSummary, dgvConvergence;
        private Label lblWarning, lblConvergence;
        private ToolTip toolTip;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private static Label MakeLabel(string text, int x, int y, int w)
        {
            return new Label { Text = text, Location = new Point(x, y), Size = new Size(w, Ui.ControlHeight), TextAlign = ContentAlignment.MiddleLeft };
        }

        private static TextBox MakeText(int x, int y, int w)
        {
            return new TextBox { Location = new Point(x, y), Size = new Size(w, Ui.ControlHeight), BackColor = Color.White };
        }

        private static DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                EnableHeadersVisualStyles = true, ColumnHeadersHeight = 24, RowTemplate = { Height = 22 },
            };
            return g;
        }

        private void InitializeComponent()
        {
            components = new Container();
            toolTip = new ToolTip(components);
            SuspendLayout();

            // --- «Функция и пределы»
            grpInput = new GroupBox { Text = "Функция и пределы", Location = new Point(8, 8), Size = new Size(784, 108), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            lblFormula = MakeLabel("f(x) =", 8, 26, 52);
            txtFormula = MakeText(64, 26, 712); txtFormula.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblLowerLimit = MakeLabel("a =", 8, 51, 52);
            txtLowerLimit = MakeText(64, 51, 110);
            lblUpperLimit = MakeLabel("b =", 190, 51, 32);
            txtUpperLimit = MakeText(226, 51, 110);
            lblFormulaHint = MakeLabel("Функции: sin, cos, tg, ctg, arcsin, arccos, arctg, exp, ln, lg, sqrt, abs. Константы: pi, e. Знаки: + - * / ^", 8, 78, 768);
            lblFormulaHint.ForeColor = SystemColors.GrayText; lblFormulaHint.Font = new Font("Tahoma", 8.25f);
            lblFormulaHint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpInput.Controls.AddRange(new Control[] { lblFormula, txtFormula, lblLowerLimit, txtLowerLimit, lblUpperLimit, txtUpperLimit, lblFormulaHint });

            // --- «Метод и точность»
            grpMethod = new GroupBox { Text = "Метод и точность", Location = new Point(8, 124), Size = new Size(784, 84), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            lblMethod = MakeLabel("Метод:", 8, 26, 52);
            cmbMethod = new ComboBox { Location = new Point(64, 26), Size = new Size(170, 22), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White };
            cmbMethod.Items.AddRange(new object[] { "Трапеций", "Симпсона", "Оба метода" });
            lblN = MakeLabel("n =", 250, 26, 32);
            txtN = MakeText(286, 26, 80);
            lblEps = MakeLabel("ε =", 382, 26, 28);
            txtEps = MakeText(414, 26, 110);
            btnCalculate = new Button { Text = "Вычислить", Location = new Point(640, 24), Size = new Size(136, Ui.ButtonHeight), Anchor = AnchorStyles.Top | AnchorStyles.Right, FlatStyle = FlatStyle.Standard, ForeColor = SystemColors.ControlText, UseVisualStyleBackColor = true };
            lblMethodHint = MakeLabel("ε не задана - расчёт при указанном n; ε задана - n подбирается автоматически (правило Рунге).", 8, 53, 768);
            lblMethodHint.ForeColor = SystemColors.GrayText; lblMethodHint.Font = new Font("Tahoma", 8.25f);
            lblMethodHint.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpMethod.Controls.AddRange(new Control[] { lblMethod, cmbMethod, lblN, txtN, lblEps, txtEps, btnCalculate, lblMethodHint });

            // --- «Результат»
            grpResult = new GroupBox { Text = "Результат", Location = new Point(8, 216), Size = new Size(784, 376), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            dgvSummary = MakeGrid(); dgvSummary.Location = new Point(8, 26); dgvSummary.Size = new Size(768, 84);
            dgvSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblWarning = new Label { Location = new Point(8, 116), Size = new Size(768, 36), ForeColor = Ui.Warning, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            lblConvergence = MakeLabel("Таблица сходимости выбранного метода:", 8, 156, 500);
            dgvConvergence = MakeGrid(); dgvConvergence.Location = new Point(8, 178); dgvConvergence.Size = new Size(768, 190);
            dgvConvergence.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpResult.Controls.AddRange(new Control[] { dgvSummary, lblWarning, lblConvergence, dgvConvergence });

            // --- форма
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Ui.BaseFont();
            ClientSize = new Size(800, 600);
            MinimumSize = new Size(560, 520);
            Controls.AddRange(new Control[] { grpInput, grpMethod, grpResult });
            Text = "Расчёт";
            ResumeLayout(false);
        }
    }
}
