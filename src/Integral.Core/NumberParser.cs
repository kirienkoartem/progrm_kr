using System.Globalization;

namespace Integral.Core
{
    /// <summary>Разбор чисел из полей ввода и файла: разделитель «.» или «,», без учёта региональных настроек.</summary>
    public static class NumberParser
    {
        /// <summary>Пытается прочитать конечное число; пробелы по краям игнорируются.</summary>
        public static bool TryParse(string? text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            string s = text!.Trim().Replace(',', '.');
            if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return false;
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>Пытается прочитать целое число.</summary>
        public static bool TryParseInt(string? text, out int value)
        {
            return int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
