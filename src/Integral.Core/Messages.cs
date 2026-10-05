using System.Collections.Generic;
using System.Globalization;

namespace Integral.Core
{
    /// <summary>Тексты сообщений об ошибках (все в одном месте).</summary>
    public static class Messages
    {
        private static readonly Dictionary<ErrorCode, string> Templates = new Dictionary<ErrorCode, string>
        {
            { ErrorCode.None, "Ошибок нет." },
            { ErrorCode.BadChar, "Недопустимый символ \"{0}\" в позиции {1}." },
            { ErrorCode.BadNumber, "Неверная запись числа в позиции {0}." },
            { ErrorCode.UnknownIdent, "Неизвестное имя \"{0}\" в позиции {1}." },
            { ErrorCode.ExpectedOperand, "Ожидается число, переменная x, функция или \"(\" в позиции {0}." },
            { ErrorCode.ExpectedRParen, "Ожидается \")\" в позиции {0}." },
            { ErrorCode.ExpectedLParen, "Ожидается \"(\" после имени функции \"{0}\" в позиции {1}." },
            { ErrorCode.UnexpectedToken, "Недопустимая лексема в позиции {0}: ожидается знак операции или конец выражения." },
            { ErrorCode.EmptyExpression, "Формула не задана." },
            { ErrorCode.TooDeep, "Формула слишком сложна: вложенность скобок и операций больше {0}." },

            { ErrorCode.DivByZero, "Деление на ноль при x = {0}." },
            { ErrorCode.LnDomain, "Логарифм определён только для аргумента > 0 (x = {0})." },
            { ErrorCode.SqrtDomain, "Корень определён только для аргумента >= 0 (x = {0})." },
            { ErrorCode.AsinDomain, "Арксинус и арккосинус определены только на отрезке [-1; 1] (x = {0})." },
            { ErrorCode.TanPole, "Тангенс или котангенс не определён в этой точке (x = {0})." },
            { ErrorCode.PowDomain, "Степень отрицательного числа с нецелым показателем не определена (x = {0})." },
            { ErrorCode.NotFinite, "Результат не является конечным числом (x = {0})." },

            { ErrorCode.BadN, "Число разбиений n должно быть целым от 1 до {0}." },
            { ErrorCode.BadEps, "Точность должна быть неотрицательным числом." },
            { ErrorCode.BadLimits, "Пределы интегрирования должны быть конечными числами." },

            { ErrorCode.FileEmpty, "Файл не содержит данных." },
            { ErrorCode.FileSyntax, "Строка {0}: ожидается запись вида \"ключ = значение\"." },
            { ErrorCode.FileUnknownKey, "Строка {0}: неизвестный ключ \"{1}\"." },
            { ErrorCode.FileDuplicateKey, "Строка {0}: ключ \"{1}\" указан повторно." },
            { ErrorCode.FileMissingKey, "Не задан обязательный ключ \"{0}\"." },
            { ErrorCode.FileBadValue, "Строка {0}: неверное значение ключа \"{1}\"." },
        };

        /// <summary>Возвращает готовую фразу по коду и аргументам шаблона.</summary>
        public static string Text(ErrorCode code, params object[] args)
        {
            string template = Templates.TryGetValue(code, out string? t) ? t : code.ToString();
            return string.Format(CultureInfo.InvariantCulture, template, args);
        }

        /// <summary>Число для сообщений: без лишних знаков, с точкой.</summary>
        public static string Num(double value)
        {
            return value.ToString("G6", CultureInfo.InvariantCulture);
        }
    }
}
