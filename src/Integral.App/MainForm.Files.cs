using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    // Работа с файлами и графиком: открытие/сохранение исходных данных, экспорт, показ графика.
    public partial class MainForm
    {
        private const string TxtFilter = "Файлы исходных данных (*.txt)|*.txt|Все файлы (*.*)|*.*";

        /// <summary>Читает текст файла: UTF-8, а если кодировка не UTF-8 - Windows-1251.</summary>
        private static string ReadTextSmart(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            try { return new UTF8Encoding(false, true).GetString(bytes).TrimStart('﻿'); }
            catch (DecoderFallbackException) { return Encoding.GetEncoding(1251).GetString(bytes); }
        }

        private void OpenData()
        {
            using (var dlg = new OpenFileDialog { Filter = TxtFilter, Title = "Открыть исходные данные" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    IntegrationTask task = TaskFile.Parse(ReadTextSmart(dlg.FileName));
                    NewCalc(task, Path.GetFileName(dlg.FileName));
                    statusState.Text = "Загружен файл " + Path.GetFileName(dlg.FileName);
                }
                catch (TaskFileException ex)
                {
                    MessageBox.Show(this, ex.Message, "Файл " + Path.GetFileName(dlg.FileName), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    statusState.Text = "Ошибка в файле данных";
                }
                catch (IOException ex) { ShowFileError(ex); }
                catch (UnauthorizedAccessException ex) { ShowFileError(ex); }
            }
        }

        private void ShowFileError(Exception ex)
        {
            MessageBox.Show(this, "Не удалось прочитать или записать файл: " + ex.Message, AppInfo.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SaveData()
        {
            CalcForm calc = ActiveCalc;
            IntegrationTask task; Node node;
            if (calc == null || !calc.TryReadTask(out task, out node)) return;
            using (var dlg = new SaveFileDialog { Filter = TxtFilter, Title = "Сохранить исходные данные", FileName = "данные.txt", DefaultExt = "txt", OverwritePrompt = true })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllText(dlg.FileName, TaskFile.ToText(task).Replace("\n", "\r\n"), new UTF8Encoding(true));
                    statusState.Text = "Данные сохранены: " + Path.GetFileName(dlg.FileName);
                }
                catch (IOException ex) { ShowFileError(ex); }
                catch (UnauthorizedAccessException ex) { ShowFileError(ex); }
            }
        }

        private void ExportHtml()
        {
            MessageBox.Show(this, "Экспорт отчёта в HTML будет добавлен на следующем этапе разработки.", AppInfo.Title,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowGraph()
        {
            CalcForm calc = ActiveCalc;
            if (calc == null) return;
            if (!calc.HasResult) calc.Calculate();
            if (!calc.HasResult) return;
            IntegrationResult r = calc.Results[calc.Results.Count - 1];
            var g = new GraphForm(calc, calc.Text, calc.Formula, calc.Function, calc.Params.A, calc.Params.B, r.N, r.Method);
            PlaceChild(g, 760, 520);
        }
    }
}
