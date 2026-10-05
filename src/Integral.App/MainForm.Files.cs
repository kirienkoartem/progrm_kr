using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Integral.Core;

namespace Integral.App
{
    // Работа с файлами и графиком: исходные данные, HTML-отчёт, перетаскивание файлов, окно графика.
    public partial class MainForm
    {
        private const string TxtFilter = "Файлы исходных данных (*.txt)|*.txt|Все файлы (*.*)|*.*";
        private const string HtmlFilter = "Веб-страница (*.html)|*.html";

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
                if (dlg.ShowDialog(this) == DialogResult.OK) OpenFile(dlg.FileName);
        }

        /// <summary>Загружает файл исходных данных в новое окно расчёта.</summary>
        private void OpenFile(string path)
        {
            try
            {
                IntegrationTask task = TaskFile.Parse(ReadTextSmart(path));
                NewCalc(task, Path.GetFileName(path));
                statusState.Text = "Загружен файл " + Path.GetFileName(path);
            }
            catch (TaskFileException ex)
            {
                MessageBox.Show(this, ex.Message, "Файл " + Path.GetFileName(path), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                statusState.Text = "Ошибка в файле данных";
            }
            catch (IOException ex) { ShowFileError(ex); }
            catch (UnauthorizedAccessException ex) { ShowFileError(ex); }
        }

        private void ShowFileError(Exception ex)
        {
            MessageBox.Show(this, "Не удалось прочитать или записать файл: " + ex.Message, AppInfo.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Имя файла по заголовку окна: недопустимые символы заменяются на «_».</summary>
        private static string SafeName(string text)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) text = text.Replace(c, '_');
            return text.Trim();
        }

        private void SaveData()
        {
            CalcForm calc = ActiveCalc;
            IntegrationTask task; Node node;
            if (calc == null || !calc.TryReadTask(out task, out node)) return;
            string name = SafeName(Path.GetFileNameWithoutExtension(calc.Text));
            using (var dlg = new SaveFileDialog { Filter = TxtFilter, Title = "Сохранить исходные данные", FileName = name + ".txt", DefaultExt = "txt", OverwritePrompt = true })
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

        /// <summary>
        /// Проверяет, что результаты расчёта есть и соответствуют полям окна; при необходимости предлагает пересчитать.
        /// Возвращает false, если действие нужно отменить.
        /// </summary>
        private bool EnsureResults(CalcForm calc, string action)
        {
            if (!calc.HasResult) { calc.Calculate(); return calc.HasResult; }
            if (!calc.IsStale) return true;
            DialogResult ans = MessageBox.Show(this, "Исходные данные изменены после расчёта. Пересчитать перед " + action + "?\n\n" +
                "Да — пересчитать; Нет — использовать результаты предыдущего расчёта.", AppInfo.Title,
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (ans == DialogResult.Cancel) return false;
            if (ans == DialogResult.Yes) calc.Calculate();
            return calc.HasResult;
        }

        /// <summary>Сохраняет отчёт о расчёте в HTML и открывает его в браузере.</summary>
        private void ExportHtml()
        {
            CalcForm calc = ActiveCalc;
            if (calc == null || !EnsureResults(calc, "экспортом")) return;
            string name = "Отчёт - " + SafeName(Path.GetFileNameWithoutExtension(calc.Text));
            using (var dlg = new SaveFileDialog { Filter = HtmlFilter, Title = "Экспорт отчёта в HTML", FileName = name + ".html", DefaultExt = "html", OverwritePrompt = true })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllText(dlg.FileName, HtmlReport.Build(calc.ToReportInput()), new UTF8Encoding(true));
                    statusState.Text = "Отчёт сохранён: " + Path.GetFileName(dlg.FileName);
                }
                catch (IOException ex) { ShowFileError(ex); return; }
                catch (UnauthorizedAccessException ex) { ShowFileError(ex); return; }
                try { Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true }); }
                catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException)
                {
                    MessageBox.Show(this, "Отчёт сохранён, но открыть его в браузере не удалось:\n" + dlg.FileName, AppInfo.Title,
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ShowGraph()
        {
            CalcForm calc = ActiveCalc;
            if (calc == null || !EnsureResults(calc, "построением графика")) return;
            IntegrationResult r = calc.Results[calc.Results.Count - 1];
            var g = new GraphForm(calc, calc.Text, calc.Formula, calc.Function, calc.Params.A, calc.Params.B, r.N, r.Method);
            PlaceChild(g, 760, 520);
        }

        /// <summary>Перетаскивание файлов *.txt в главное окно открывает их.</summary>
        private void EnableFileDrop()
        {
            AllowDrop = true;
            DragEnter += OnFileDragEnter;
            DragDrop += OnFileDragDrop;
            foreach (Control c in Controls)
                if (c is MdiClient)
                {
                    c.AllowDrop = true;
                    c.DragEnter += OnFileDragEnter;
                    c.DragDrop += OnFileDragDrop;
                }
        }

        private static void OnFileDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void OnFileDragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null) return;
            foreach (string f in files)
                if (string.Equals(Path.GetExtension(f), ".txt", StringComparison.OrdinalIgnoreCase)) OpenFile(f);
                else statusState.Text = "Пропущен файл (нужен *.txt): " + Path.GetFileName(f);
        }
    }
}
