using System;

namespace Integral.Core
{
    /// <summary>Вычисление дерева выражения в точке x с контролем области определения.</summary>
    public static class Evaluator
    {
        /// <summary>Значение выражения при заданном x. Бросает <see cref="EvalException"/>.</summary>
        public static double Evaluate(Node node, double x)
        {
            switch (node)
            {
                case NumberNode n: return n.Value;
                case VariableNode _: return x;
                case NegateNode g: return -Evaluate(g.Operand, x);
                case BinaryNode b: return Binary(b, x);
                case FunctionNode f: return Function(f, x);
                default: throw new ArgumentException("Неизвестный тип узла.", nameof(node));
            }
        }

        private static double Binary(BinaryNode b, double x)
        {
            double l = Evaluate(b.Left, x);
            double r = Evaluate(b.Right, x);
            double res;
            switch (b.Op)
            {
                case '+': res = l + r; break;
                case '-': res = l - r; break;
                case '*': res = l * r; break;
                case '/':
                    if (r == 0) throw new EvalException(ErrorCode.DivByZero, x);
                    res = l / r;
                    break;
                case '^':
                    if (l == 0 && r < 0) throw new EvalException(ErrorCode.DivByZero, x);
                    if (l < 0 && r != Math.Floor(r)) throw new EvalException(ErrorCode.PowDomain, x);
                    res = Math.Pow(l, r);
                    break;
                default: throw new ArgumentException("Неизвестная операция.", nameof(b));
            }
            return Finite(res, x);
        }

        private static double Function(FunctionNode f, double x)
        {
            double arg = Evaluate(f.Argument, x);
            ErrorCode code = Functions.TryApply(f.Name, arg, out double res);
            if (code != ErrorCode.None) throw new EvalException(code, x);
            return Finite(res, x);
        }

        private static double Finite(double v, double x)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) throw new EvalException(ErrorCode.NotFinite, x);
            return v;
        }
    }
}
