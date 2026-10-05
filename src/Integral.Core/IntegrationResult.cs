using System.Collections.Generic;

namespace Integral.Core
{
    /// <summary>Метод численного интегрирования.</summary>
    public enum Method
    {
        /// <summary>Составная формула трапеций (порядок точности 2).</summary>
        Trapezoid,
        /// <summary>Составная формула Симпсона (порядок точности 4).</summary>
        Simpson
    }

    /// <summary>Параметры интегрирования.</summary>
    public sealed class IntegrationParams
    {
        /// <summary>Максимальное число разбиений (2^20).</summary>
        public const int MaxN = 1 << 20;

        /// <summary>Нижний предел.</summary>
        public double A { get; set; }
        /// <summary>Верхний предел.</summary>
        public double B { get; set; }
        /// <summary>Число разбиений (начальное, если задана точность).</summary>
        public int N { get; set; } = 100;
        /// <summary>Требуемая точность; 0 — расчёт с фиксированным n.</summary>
        public double Eps { get; set; }
    }

    /// <summary>Одна строка таблицы сходимости.</summary>
    public readonly struct ConvergenceStep
    {
        public int N { get; }
        public double Value { get; }
        /// <summary>Оценка погрешности по правилу Рунге (NaN для первой строки).</summary>
        public double ErrorEstimate { get; }

        public ConvergenceStep(int n, double value, double errorEstimate)
        {
            N = n; Value = value; ErrorEstimate = errorEstimate;
        }
    }

    /// <summary>Результат интегрирования одним методом.</summary>
    public sealed class IntegrationResult
    {
        public Method Method { get; internal set; }
        /// <summary>Значение интеграла.</summary>
        public double Value { get; internal set; }
        /// <summary>Итоговое число разбиений.</summary>
        public int N { get; internal set; }
        /// <summary>Итоговый шаг (со знаком, если A &gt; B).</summary>
        public double H { get; internal set; }
        /// <summary>Оценка погрешности по правилу Рунге (NaN, если не вычислялась).</summary>
        public double ErrorEstimate { get; internal set; } = double.NaN;
        /// <summary>Фактический порядок сходимости по трём последним удвоениям (NaN, если шагов меньше трёх).</summary>
        public double ObservedOrder { get; internal set; } = double.NaN;
        /// <summary>
        /// Фактический порядок заметно ниже теоретического (негладкая функция):
        /// оценка погрешности по Рунге в этом случае занижена.
        /// </summary>
        public bool OrderDegraded { get; internal set; }
        /// <summary>Число вычислений подынтегральной функции.</summary>
        public long Evaluations { get; internal set; }
        /// <summary>Время расчёта, мс.</summary>
        public double ElapsedMs { get; internal set; }
        /// <summary>Достигнута ли заданная точность (при фиксированном n всегда true).</summary>
        public bool Converged { get; internal set; } = true;
        /// <summary>Заданное n было увеличено до чётного (метод Симпсона).</summary>
        public bool NAdjusted { get; internal set; }
        /// <summary>Таблица сходимости при подборе n по точности.</summary>
        public IReadOnlyList<ConvergenceStep> Steps { get; internal set; } = new List<ConvergenceStep>();
    }
}
