using System;
using System.Globalization;
using System.Net;

namespace Integral.Core
{
    /// <summary>Форматирование чисел и текста для HTML-отчёта: десятичная запятая, степени десяти, экранирование.</summary>
    public static class HtmlFormat
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Экранирует текст для вставки в HTML.</summary>
        public static string Esc(string text) => WebUtility.HtmlEncode(text ?? "");

        /// <summary>Число с заданным числом значащих цифр и десятичной запятой; очень большие и малые — как m·10ⁿ.</summary>
        public static string Num(double v, int digits = 10)
        {
            if (double.IsNaN(v)) return "—";
            if (v == 0) return "0";
            double abs = Math.Abs(v);
            if (abs >= 1e-4 && abs < 1e9)
                return v.ToString("G" + digits, Inv).Replace('.', ',').Replace("-", "−");
            return Sci(v, Math.Min(digits, 6));
        }

        /// <summary>Научная запись: «1,23·10<sup>−6</sup>».</summary>
        public static string Sci(double v, int digits = 3)
        {
            if (double.IsNaN(v)) return "—";
            if (v == 0) return "0";
            int exp = (int)Math.Floor(Math.Log10(Math.Abs(v)));
            double mant = v / Math.Pow(10, exp);
            if (Math.Abs(mant) >= 9.9999999) { mant /= 10; exp++; }
            string m = mant.ToString("F" + Math.Max(0, digits - 1), Inv).Replace('.', ',').Replace("-", "−");
            return m + "·10<sup>" + exp.ToString(Inv).Replace("-", "−") + "</sup>";
        }

        /// <summary>Целое число с разделением разрядов неразрывным пробелом: 1 048 576.</summary>
        public static string Int(long v) => v.ToString("N0", Inv).Replace(",", " ");

        /// <summary>Координата SVG: точка как разделитель, одна цифра после запятой.</summary>
        public static string Coord(double v) => v.ToString("F1", Inv);
    }
}
