namespace Integral.Core
{
    /// <summary>Типы лексем формулы.</summary>
    public enum TokenType
    {
        Number, Variable, Constant, Function,
        Plus, Minus, Multiply, Divide, Power,
        LeftParen, RightParen, End
    }

    /// <summary>Лексема: тип, позиция в формуле (с 1), длина, значение числа/константы или имя функции.</summary>
    public readonly struct Token
    {
        public TokenType Type { get; }
        public int Position { get; }
        public int Length { get; }
        public double Value { get; }
        public string Name { get; }

        public Token(TokenType type, int position, int length, double value = 0, string name = "")
        {
            Type = type;
            Position = position;
            Length = length;
            Value = value;
            Name = name;
        }
    }
}
