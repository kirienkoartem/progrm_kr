using System;
using System.Windows.Forms;

namespace Integral.App
{
    /// <summary>Точка входа приложения.</summary>
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportCrash(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportCrash(e.ExceptionObject as Exception);
            Application.Run(new MainForm());
        }

        /// <summary>Непредвиденная ошибка: сообщение пользователю вместо аварийного закрытия.</summary>
        private static void ReportCrash(Exception ex)
        {
            MessageBox.Show("Произошла непредвиденная ошибка. Программа продолжит работу, но последнее действие могло не выполниться.\n\n" +
                (ex != null ? ex.Message : "(нет сведений)"), AppInfo.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
