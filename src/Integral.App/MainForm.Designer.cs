using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Integral.App
{
    partial class MainForm
    {
        private IContainer components = null;

        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusHint, statusState, statusMethod;

        private ToolStripMenuItem miFile, miCalc, miWindow, miHelp, miMethod;
        private ToolStripMenuItem miNew, miOpen, miSave, miExport, miExit;
        private ToolStripMenuItem miRun, miMethodTrapezoid, miMethodSimpson, miMethodBoth, miGraph;
        private ToolStripMenuItem miCascade, miTileHorizontal, miTileVertical, miArrangeIcons, miCloseAll;
        private ToolStripMenuItem miContents, miFileFormat, miAbout;
        private ToolStripButton tbNew, tbOpen, tbSave, tbExport, tbRun, tbGraph, tbHelp, tbAbout;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private static ToolStripMenuItem Item(string text, string hint, Keys keys = Keys.None)
        {
            var m = new ToolStripMenuItem(text) { ToolTipText = hint, ShortcutKeys = keys };
            return m;
        }

        private static ToolStripButton Button(string hint, IconFactory.Kind kind)
        {
            return new ToolStripButton { ToolTipText = hint, Image = IconFactory.Create(kind, 24), DisplayStyle = ToolStripItemDisplayStyle.Image, AutoSize = false, Size = new Size(34, 34) };
        }

        private void InitializeComponent()
        {
            components = new Container();
            SuspendLayout();

            // --- главное меню
            menuStrip = new MenuStrip { Font = Ui.BaseFont() };
            miFile = new ToolStripMenuItem("&Файл"); miCalc = new ToolStripMenuItem("&Расчёт");
            miWindow = new ToolStripMenuItem("&Окно"); miHelp = new ToolStripMenuItem("&Справка");

            miNew = Item("&Новый расчёт", "Открыть новое окно расчёта", Keys.Control | Keys.N);
            miOpen = Item("&Открыть данные...", "Загрузить исходные данные из файла *.txt в новое окно", Keys.Control | Keys.O);
            miSave = Item("&Сохранить данные...", "Сохранить исходные данные текущего расчёта в файл *.txt", Keys.Control | Keys.S);
            miExport = Item("&Экспорт отчёта в HTML...", "Сохранить результаты расчёта в файл *.html", Keys.Control | Keys.E);
            miExit = Item("&Выход", "Закрыть программу", Keys.Alt | Keys.F4);
            miFile.DropDownItems.AddRange(new ToolStripItem[] { miNew, miOpen, miSave, miExport, new ToolStripSeparator(), miExit });

            miRun = Item("&Вычислить", "Выполнить расчёт интеграла", Keys.F5);
            miMethod = new ToolStripMenuItem("&Метод") { ToolTipText = "Метод численного интегрирования" };
            miMethodTrapezoid = Item("&Трапеций", "Метод трапеций (порядок 2)");
            miMethodSimpson = Item("&Симпсона", "Метод Симпсона (порядок 4)");
            miMethodBoth = Item("&Оба метода", "Рассчитать обоими методами и сравнить");
            miMethod.DropDownItems.AddRange(new ToolStripItem[] { miMethodTrapezoid, miMethodSimpson, miMethodBoth });
            miGraph = Item("Показать &график", "Открыть окно с графиком функции", Keys.F6);
            miCalc.DropDownItems.AddRange(new ToolStripItem[] { miRun, miMethod, new ToolStripSeparator(), miGraph });

            miCascade = Item("&Каскадом", "Расположить окна каскадом");
            miTileHorizontal = Item("Мозаикой по &горизонтали", "Расположить окна мозаикой по горизонтали");
            miTileVertical = Item("Мозаикой по &вертикали", "Расположить окна мозаикой по вертикали");
            miArrangeIcons = Item("&Упорядочить значки", "Упорядочить свёрнутые окна");
            miCloseAll = Item("&Закрыть все", "Закрыть все дочерние окна");
            miWindow.DropDownItems.AddRange(new ToolStripItem[] { miCascade, miTileHorizontal, miTileVertical, miArrangeIcons, new ToolStripSeparator(), miCloseAll, new ToolStripSeparator() });

            miContents = Item("&Содержание", "Справка по работе с программой", Keys.F1);
            miFileFormat = Item("&Формат входного файла", "Справка: формат файла исходных данных");
            miAbout = Item("&О программе", "Сведения о программе и авторе");
            miHelp.DropDownItems.AddRange(new ToolStripItem[] { miContents, miFileFormat, new ToolStripSeparator(), miAbout });
            menuStrip.Items.AddRange(new ToolStripItem[] { miFile, miCalc, miWindow, miHelp });
            menuStrip.MdiWindowListItem = miWindow;

            // --- панель инструментов
            toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, ImageScalingSize = new Size(24, 24), Padding = new Padding(6, 3, 6, 3), Font = Ui.BaseFont() };
            tbNew = Button("Новый расчёт (Ctrl+N)", IconFactory.Kind.New);
            tbOpen = Button("Открыть данные (Ctrl+O)", IconFactory.Kind.Open);
            tbSave = Button("Сохранить данные (Ctrl+S)", IconFactory.Kind.Save);
            tbExport = Button("Экспорт отчёта в HTML (Ctrl+E)", IconFactory.Kind.Html);
            tbRun = Button("Вычислить (F5)", IconFactory.Kind.Calc);
            tbGraph = Button("Показать график (F6)", IconFactory.Kind.Graph);
            tbHelp = Button("Справка (F1)", IconFactory.Kind.Help);
            tbAbout = Button("О программе", IconFactory.Kind.About);
            toolStrip.Items.AddRange(new ToolStripItem[] { tbNew, tbOpen, tbSave, tbExport, new ToolStripSeparator(), tbRun, tbGraph, new ToolStripSeparator(), tbHelp, tbAbout });

            // --- строка состояния
            statusStrip = new StatusStrip { Font = Ui.BaseFont(), SizingGrip = true };
            statusHint = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft, Text = "Готово" };
            statusState = new ToolStripStatusLabel { AutoSize = false, Width = 330, TextAlign = ContentAlignment.MiddleLeft, BorderSides = ToolStripStatusLabelBorderSides.Left, Text = "Нет открытых расчётов: Ctrl+N или Ctrl+O" };
            statusMethod = new ToolStripStatusLabel { AutoSize = false, Width = 170, TextAlign = ContentAlignment.MiddleLeft, BorderSides = ToolStripStatusLabelBorderSides.Left, Text = "" };
            statusStrip.Items.AddRange(new ToolStripItem[] { statusHint, statusState, statusMethod });

            // --- форма
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = Ui.BaseFont();
            ClientSize = new Size(1040, 700);
            MinimumSize = new Size(760, 520);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip;
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            Text = AppInfo.Title;
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
