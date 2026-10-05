using System;
using System.Collections.Generic;

namespace Integral.Core
{
    /// <summary>Таблицы встроенных функций и констант формулы.</summary>
    public static class Functions
    {
        private const double PoleEps = 1e-12;

        /// <summary>Одноаргументные функции: имя в формуле → вычисление.</summary>
        private static readonly Dictionary<string, Func<double, double>> Table =
            new Dictionary<string, Func<double, double>>
            {
                { "sin", Math.Sin },
                { "cos", Math.Cos },
                { "tg", Math.Tan },
                { "ctg", a => 1.0 / Math.Tan(a) },
                { "arcsin", Math.Asin },
                { "arccos", Math.Acos },
                { "arctg", Math.Atan },
                { "exp", Math.Exp },
                { "ln", Math.Log },
                { "lg", Math.Log10 },
                { "sqrt", Math.Sqrt },
                { "abs", Math.Abs },
            };

        /// <summary>Константы формулы.</summary>
        private static readonly Dictionary<string, double> Constants =
            new Dictionary<string, double>
            {
                { "pi", Math.PI },
                { "e", Math.E },
            };

        /// <summary>Является ли имя встроенной функцией.</summary>
        public static bool IsFunction(string name) => Table.ContainsKey(name);

        /// <summary>Пытается получить значение константы по имени.</summary>
        public static bool TryGetConstant(string name, out double value) => Constants.TryGetValue(name, out value);

        /// <summary>
        /// Вычисляет функцию с проверкой области определения.
        /// Возвращает <see cref="ErrorCode.None"/> при успехе, иначе код ошибки.
        /// </summary>
        public static ErrorCode TryApply(string name, double arg, out double result)
        {
            result = 0;
            switch (name)
            {
                case "ln":
                case "lg":
                    if (!(arg > 0)) return ErrorCode.LnDomain;
                    break;
                case "sqrt":
                    if (arg < 0) return ErrorCode.SqrtDomain;
                    break;
                case "arcsin":
                case "arccos":
                    if (arg < -1 || arg > 1) return ErrorCode.AsinDomain;
                    break;
                case "tg":
                    if (Math.Abs(Math.Cos(arg)) < PoleEps) return ErrorCode.TanPole;
                    break;
                case "ctg":
                    if (Math.Abs(Math.Sin(arg)) < PoleEps) return ErrorCode.TanPole;
                    break;
            }
            result = Table[name](arg);
            return ErrorCode.None;
        }
    }
}
