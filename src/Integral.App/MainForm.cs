using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    /// <summary>Главное MDI-окно: меню, панель инструментов, строка состояния, управление дочерними окнами.</summary>
    public partial class MainForm : Form
    {
        /// <summary>Команда: пункт меню и (необязательно) кнопка панели инструментов с общим состоянием доступности.</summary>
        private sealed class Command
        {
            public ToolStripMenuItem Menu; public ToolStripButton Tool;
            public bool Enabled { set { Menu.Enabled = value; if (Tool != null) Tool.Enabled = value; } }
        }

        private int _calcCounter;
        private HelpForm _help;
        private Command _cmdSave, _cmdExport, _cmdRun, _cmdGraph;

        public MainForm()
        {
            InitializeComponent();
            Icon = IconFactory.AppIcon();
            Wire();
            EnableFileDrop();
            UpdateCommands();
        }

        /// <summary>Окно расчёта, к которому относятся команды: активное «Расчёт» либо исходное для активного «Графика».</summary>
        private CalcForm ActiveCalc
        {
            get
            {
                var g = ActiveMdiChild as GraphForm;
                if (g != null) return g.Source.IsDisposed ? null : g.Source;
                return ActiveMdiChild as CalcForm;
            }
        }

        private void Wire()
        {
            _cmdSave = new Command { Menu = miSave, Tool = tbSave }; _cmdExport = new Command { Menu = miExport, Tool = tbExport };
            _cmdRun = new Command { Menu = miRun, Tool = tbRun }; _cmdGraph = new Command { Menu = miGraph, Tool = tbGraph };

            Bind(miNew, tbNew, () => NewCalc(null, null)); Bind(miOpen, tbOpen, OpenData); Bind(miSave, tbSave, SaveData);
            Bind(miExport, tbExport, ExportHtml); Bind(miRun, tbRun, delegate { if (ActiveCalc != null) ActiveCalc.Calculate(); });
            Bind(miGraph, tbGraph, ShowGraph); Bind(miContents, tbHelp, () => ShowHelp(ContextTopic()));
            Bind(miFileFormat, null, () => ShowHelp(HelpContent.File)); Bind(miAbout, tbAbout, ShowAbout); Bind(miExit, null, Close);
            miCascade.Click += (s, e) => LayoutMdi(MdiLayout.Cascade);
            miTileHorizontal.Click += (s, e) => LayoutMdi(MdiLayout.TileHorizontal);
            miTileVertical.Click += (s, e) => LayoutMdi(MdiLayout.TileVertical);
            miArrangeIcons.Click += (s, e) => LayoutMdi(MdiLayout.ArrangeIcons);
            miCloseAll.Click += (s, e) => CloseAllChildren();
            miMethodTrapezoid.Click += (s, e) => SetMethod(0); miMethodSimpson.Click += (s, e) => SetMethod(1); miMethodBoth.Click += (s, e) => SetMethod(2);
            MdiChildActivate += (s, e) => UpdateCommands();
            FormClosing += (s, e) => { if (_help != null && !_help.IsDisposed) _help.Close(); };
            foreach (ToolStripItem it in AllItems()) HookHint(it);
        }

        private static void Bind(ToolStripMenuItem menu, ToolStripButton tool, Action action)
        {
            menu.Click += (s, e) => action();
            if (tool != null) tool.Click += (s, e) => action();
        }

        private IEnumerable<ToolStripItem> AllItems()
        {
            foreach (ToolStripItem top in menuStrip.Items)
            {
                yield return top;
                var m = top as ToolStripMenuItem;
                if (m == null) continue;
                foreach (ToolStripItem sub in m.DropDownItems)
                {
                    yield return sub;
                    var mm = sub as ToolStripMenuItem;
                    if (mm != null) foreach (ToolStripItem sub2 in mm.DropDownItems) yield return sub2;
                }
            }
            foreach (ToolStripItem t in toolStrip.Items) yield return t;
        }

        private void HookHint(ToolStripItem it)
        {
            if (string.IsNullOrEmpty(it.ToolTipText)) return;
            it.MouseEnter += (s, e) => statusHint.Text = it.ToolTipText;
            it.MouseLeave += (s, e) => statusHint.Text = "Готово";
        }

        private void SetHint(string text) { statusHint.Text = string.IsNullOrEmpty(text) ? "Готово" : text; }

        private void UpdateCommands()
        {
            CalcForm calc = ActiveCalc;
            bool has = calc != null;
            _cmdSave.Enabled = has; _cmdExport.Enabled = has; _cmdRun.Enabled = has; _cmdGraph.Enabled = has; miMethod.Enabled = has;
            if (has)
            {
                miMethodTrapezoid.Checked = calc.MethodIndex == 0; miMethodSimpson.Checked = calc.MethodIndex == 1; miMethodBoth.Checked = calc.MethodIndex == 2;
                statusMethod.Text = "Метод: " + new[] { "трапеций", "Симпсона", "оба метода" }[Math.Max(0, calc.MethodIndex)];
            }
            else
            {
                statusMethod.Text = "";
                statusState.Text = MdiChildren.Length == 0 ? "Нет открытых расчётов: Ctrl+N или Ctrl+O" : statusState.Text;
            }
        }

        private void SetMethod(int index)
        {
            if (ActiveCalc != null) ActiveCalc.MethodIndex = index;
            UpdateCommands();
        }

        private Size MdiAvailable()
        {
            foreach (Control c in Controls) if (c is MdiClient) return c.ClientSize;
            return new Size(1000, 560);
        }

        private void PlaceChild(Form f, int maxW, int maxH)
        {
            int off = (MdiChildren.Length % 8) * 24;
            Size avail = MdiAvailable();
            f.StartPosition = FormStartPosition.Manual;
            f.Size = new Size(Math.Max(400, Math.Min(maxW, avail.Width - off - 4)), Math.Max(300, Math.Min(maxH, avail.Height - off - 4)));
            f.Location = new Point(off, off);
            f.MdiParent = this;
            f.Show();
        }

        /// <summary>Открывает новое окно «Расчёт».</summary>
        private CalcForm NewCalc(IntegrationTask task, string title)
        {
            _calcCounter++;
            var f = new CalcForm(title ?? ("Расчёт " + _calcCounter));
            f.StatusChanged += (s, text) => statusState.Text = text;
            f.HintChanged += (s, text) => SetHint(text);
            f.MethodChanged += (s, e) => UpdateCommands();
            if (task != null) f.LoadTask(task);
            PlaceChild(f, 830, 640);
            UpdateCommands();
            return f;
        }

        private void CloseAllChildren()
        {
            foreach (Form f in MdiChildren) f.Close();
            UpdateCommands();
        }

        /// <summary>Раздел справки для текущей ситуации: зависит от активного окна и поля ввода.</summary>
        private string ContextTopic()
        {
            if (ActiveMdiChild is GraphForm) return HelpContent.Graph;
            var calc = ActiveMdiChild as CalcForm;
            return calc != null ? calc.HelpTopic : HelpContent.Start;
        }

        private void ShowHelp(string topic)
        {
            if (_help == null || _help.IsDisposed) { _help = new HelpForm(); _help.Show(this); }
            _help.ShowTopic(topic);
            _help.Activate();
        }

        private void ShowAbout()
        {
            using (var dlg = new AboutForm()) dlg.ShowDialog(this);
        }
    }
}
