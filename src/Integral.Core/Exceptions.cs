using System;

namespace Integral.Core
{
    /// <summary>Базовое исключение ядра: несёт код ошибки и готовый русский текст.</summary>
    public class IntegralException : Exception
    {
        /// <summary>Код ошибки.</summary>
        public ErrorCode Code { get; }

        public IntegralException(ErrorCode code, string message) : base(message)
        {
            Code = code;
        }
    }

    /// <summary>Ошибка в записи формулы (лексика или синтаксис).</summary>
    public sealed class FormulaException : IntegralException
    {
        /// <summary>Позиция ошибки в формуле, начиная с 1.</summary>
        public int Position { get; }

        public FormulaException(ErrorCode code, int position, params object[] args)
            : base(code, Messages.Text(code, args))
        {
            Position = position;
        }
    }

    /// <summary>Ошибка вычисления функции в конкретной точке.</summary>
    public sealed class EvalException : IntegralException
    {
        /// <summary>Значение x, при котором возникла ошибка.</summary>
        public double X { get; }

        public EvalException(ErrorCode code, double x)
            : base(code, Messages.Text(code, Messages.Num(x)))
        {
            X = x;
        }
    }

    /// <summary>Ошибка параметров интегрирования.</summary>
    public sealed class ParamsException : IntegralException
    {
        public ParamsException(ErrorCode code, params object[] args)
            : base(code, Messages.Text(code, args))
        {
        }
    }

    /// <summary>Ошибка в файле исходных данных.</summary>
    public sealed class TaskFileException : IntegralException
    {
        /// <summary>Номер строки файла, начиная с 1; 0 — ошибка относится ко всему файлу.</summary>
        public int Line { get; }

        public TaskFileException(ErrorCode code, int line, params object[] args)
            : base(code, Messages.Text(code, args))
        {
            Line = line;
        }
    }
}
