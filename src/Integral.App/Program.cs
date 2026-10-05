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
            Application.Run(new MainForm());
        }
    }
}
