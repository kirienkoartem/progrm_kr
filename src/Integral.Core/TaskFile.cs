using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Integral.Core
{
    /// <summary>Выбор метода в файле исходных данных.</summary>
    public enum MethodChoice { Trapezoid, Simpson, Both }

    /// <summary>Исходные данные задачи интегрирования.</summary>
    public sealed class IntegrationTask
    {
        public string Formula { get; set; } = "";
        public double A { get; set; }
        public double B { get; set; }
        public MethodChoice Method { get; set; } = MethodChoice.Simpson;
        public int N { get; set; } = 100;
        /// <summary>0 — точность не задана, расчёт с фиксированным n.</summary>
        public double Eps { get; set; }
    }

    /// <summary>Чтение и запись файла исходных данных (строки «ключ = значение», «#» — комментарий).</summary>
    public static class TaskFile
    {
        private static readonly string[] Keys = { "function", "a", "b", "method", "n", "eps" };

        /// <summary>Разбирает текст файла. Бросает <see cref="TaskFileException"/>.</summary>
        public static IntegrationTask Parse(string text)
        {
            var task = new IntegrationTask();
            var seen = new Dictionary<string, int>();
            string[] lines = (text ?? "").TrimStart('﻿').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int lineNo = i + 1;
                string line = lines[i];
                int hash = line.IndexOf('#');
                if (hash >= 0) line = line.Substring(0, hash);
                line = line.Trim();
                if (line.Length == 0) continue;

                int eq = line.IndexOf('=');
                if (eq <= 0) throw new TaskFileException(ErrorCode.FileSyntax, lineNo, lineNo);
                string key = line.Substring(0, eq).Trim().ToLowerInvariant();
                string value = line.Substring(eq + 1).Trim();
                if (Array.IndexOf(Keys, key) < 0)
                    throw new TaskFileException(ErrorCode.FileUnknownKey, lineNo, lineNo, key);
                if (seen.ContainsKey(key))
                    throw new TaskFileException(ErrorCode.FileDuplicateKey, lineNo, lineNo, key);
                seen[key] = lineNo;
                Assign(task, key, value, lineNo);
            }
            if (seen.Count == 0) throw new TaskFileException(ErrorCode.FileEmpty, 0);
            foreach (string required in new[] { "function", "a", "b" })
                if (!seen.ContainsKey(required)) throw new TaskFileException(ErrorCode.FileMissingKey, 0, required);
            return task;
        }

        /// <summary>Формирует текст файла исходных данных (для команды «Сохранить данные»).</summary>
        public static string ToText(IntegrationTask t)
        {
            var sb = new StringBuilder();
            sb.Append("# Исходные данные: интегрирование функции, заданной формулой\n");
            sb.Append("function = ").Append(t.Formula).Append('\n');
            sb.Append("a        = ").Append(Num(t.A)).Append('\n');
            sb.Append("b        = ").Append(Num(t.B)).Append('\n');
            sb.Append("method   = ").Append(t.Method.ToString().ToLowerInvariant()).Append('\n');
            sb.Append("n        = ").Append(t.N.ToString(CultureInfo.InvariantCulture)).Append('\n');
            if (t.Eps > 0) sb.Append("eps      = ").Append(Num(t.Eps)).Append('\n');
            return sb.ToString();
        }

        private static string Num(double v) => v.ToString("R", CultureInfo.InvariantCulture);

        private static void Assign(IntegrationTask task, string key, string value, int line)
        {
            switch (key)
            {
                case "function":
                    try { Parser.Parse(value); }
                    catch (FormulaException ex) { throw new TaskFileException(ErrorCode.FileBadFormula, line, line, ex.Message); }
                    task.Formula = value;
                    break;
                case "a": task.A = ReadNumber(value, key, line); break;
                case "b": task.B = ReadNumber(value, key, line); break;
                case "method": task.Method = ReadMethod(value, key, line); break;
                case "n":
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
                        || n < 1 || n > IntegrationParams.MaxN)
                        throw new TaskFileException(ErrorCode.FileBadValue, line, line, key);
                    task.N = n;
                    break;
                case "eps":
                    task.Eps = ReadNumber(value, key, line);
                    if (task.Eps < 0) throw new TaskFileException(ErrorCode.FileBadValue, line, line, key);
                    break;
            }
        }

        private static double ReadNumber(string value, string key, int line)
        {
            if (!double.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double d)
                || double.IsNaN(d) || double.IsInfinity(d))
                throw new TaskFileException(ErrorCode.FileBadValue, line, line, key);
            return d;
        }

        private static MethodChoice ReadMethod(string value, string key, int line)
        {
            switch (value.ToLowerInvariant())
            {
                case "trapezoid": case "трапеции": return MethodChoice.Trapezoid;
                case "simpson": case "симпсон": return MethodChoice.Simpson;
                case "both": case "оба": return MethodChoice.Both;
                default: throw new TaskFileException(ErrorCode.FileBadValue, line, line, key);
            }
        }
    }
}
