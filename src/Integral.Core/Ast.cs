namespace Integral.Core
{
    /// <summary>Узел дерева выражения.</summary>
    public abstract class Node
    {
    }

    /// <summary>Число (константы pi и e сворачиваются в число при разборе).</summary>
    public sealed class NumberNode : Node
    {
        public double Value { get; }
        public NumberNode(double value) { Value = value; }
    }

    /// <summary>Переменная x.</summary>
    public sealed class VariableNode : Node
    {
    }

    /// <summary>Унарный минус.</summary>
    public sealed class NegateNode : Node
    {
        public Node Operand { get; }
        public NegateNode(Node operand) { Operand = operand; }
    }

    /// <summary>Бинарная операция: '+', '-', '*', '/', '^'.</summary>
    public sealed class BinaryNode : Node
    {
        public char Op { get; }
        public Node Left { get; }
        public Node Right { get; }
        public BinaryNode(char op, Node left, Node right) { Op = op; Left = left; Right = right; }
    }

    /// <summary>Вызов функции с одним аргументом.</summary>
    public sealed class FunctionNode : Node
    {
        public string Name { get; }
        public Node Argument { get; }
        public FunctionNode(string name, Node argument) { Name = name; Argument = argument; }
    }
}
