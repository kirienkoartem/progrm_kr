using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Integral.App
{
    /// <summary>Единые размеры, шрифт и цвета интерфейса (методические указания, табл. 2.1).</summary>
    internal static class Ui
    {
        public const int ControlHeight = 20;   // высота поля ввода
        public const int ButtonHeight = 24;    // высота кнопки
        public const int GapRelated = 5;       // интервал между связанными элементами
        public const int GapUnrelated = 8;     // интервал между несвязанными элементами
        public const int Frame = 8;            // ширина рамки

        public static readonly Color Ink = Color.FromArgb(31, 41, 55);
        public static readonly Color Accent = Color.FromArgb(37, 99, 235);
        public static readonly Color AccentSoft = Color.FromArgb(219, 234, 254);
        public static readonly Color Warning = Color.FromArgb(154, 52, 18);

        /// <summary>Основной шрифт Tahoma 10 пт (по методическим указаниям); если Tahoma не установлен - шрифт системы.</summary>
        public static Font BaseFont()
        {
            foreach (FontFamily f in FontFamily.Families)
                if (string.Equals(f.Name, "Tahoma", System.StringComparison.OrdinalIgnoreCase)) return new Font(f, 10f);
            return new Font(SystemFonts.MessageBoxFont.FontFamily, 10f);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        /// <summary>Серая подсказка в пустом поле ввода (Windows). В других средах просто не показывается.</summary>
        public static void SetCue(TextBox box, string text)
        {
            EventHandler apply = (s, e) =>
            {
                try { SendMessage(box.Handle, EM_SETCUEBANNER, (IntPtr)1, text); }
                catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException) { }
            };
            if (box.IsHandleCreated) apply(box, EventArgs.Empty);
            else box.HandleCreated += apply;
        }
    }

    /// <summary>Сведения для окна «О программе» и заголовков.</summary>
    internal static class AppInfo
    {
        public const string Title = "Интегрирование функций";
        public const string University = "ФГБОУ ВО «Донбасский государственный технический университет»";
        // TODO: уточнить у студента и заменить (см. PLAN.md, раздел 9)
        public const string Faculty = "(факультет уточняется)";
        public const string Department = "(кафедра уточняется)";
        public const string Group = "БИ-25";
        public const string Student = "Кириенко Артём Максимович";
        public const string Topic = "Интегрирование функций, заданных формулой (вариант 7)";
        public const string Supervisor = "Самойлов Д.В. (должность уточняется)";
        public const string Year = "2026";
        public const string Description =
            "Программа вычисляет определённый интеграл функции, заданной формулой, " +
            "методом трапеций и методом Симпсона. Исходные данные читаются из текстового файла или вводятся " +
            "в окне. Число разбиений задаётся вручную либо подбирается автоматически под заданную точность " +
            "(оценка по правилу Рунге). Результаты выводятся в окно, показываются на графике и сохраняются " +
            "в файл HTML.";
    }
}
