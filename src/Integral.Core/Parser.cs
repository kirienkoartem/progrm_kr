namespace Integral.Core
{
    /// <summary>
    /// Синтаксический анализатор формулы (рекурсивный спуск по грамматике БНФ).
    /// Выражение  ::= Слагаемое { ('+'|'-') Слагаемое }
    /// Слагаемое  ::= Множитель { ('*'|'/') Множитель }
    /// Множитель  ::= ['+'|'-'] Степень
    /// Степень    ::= Основа [ '^' Множитель ]
    /// Основа     ::= Число | 'x' | Константа | Функция '(' Выражение ')' | '(' Выражение ')'
    /// </summary>
    public sealed class Parser
    {
        /// <summary>Максимальная вложенность разбора (защита от переполнения стека).</summary>
        public const int MaxDepth = 200;

        private readonly Lexer _lexer;
        private Token _cur;
        private int _depth;

        private Parser(string source)
        {
            _lexer = new Lexer(source);
            _cur = _lexer.Next();
        }

        /// <summary>Разбирает формулу в дерево выражения. Бросает <see cref="FormulaException"/>.</summary>
        public static Node Parse(string formula)
        {
            var parser = new Parser(formula);
            if (parser._cur.Type == TokenType.End)
                throw new FormulaException(ErrorCode.EmptyExpression, 1);
            Node root = parser.ParseExpression();
            if (parser._cur.Type != TokenType.End)
                throw new FormulaException(ErrorCode.UnexpectedToken, parser._cur.Position, parser._cur.Position);
            return root;
        }

        /// <summary>Есть ли в дереве переменная x.</summary>
        public static bool UsesX(Node node)
        {
            switch (node)
            {
                case VariableNode _: return true;
                case NegateNode n: return UsesX(n.Operand);
                case BinaryNode b: return UsesX(b.Left) || UsesX(b.Right);
                case FunctionNode f: return UsesX(f.Argument);
                default: return false;
            }
        }

        private void Advance() { _cur = _lexer.Next(); }

        private void Enter()
        {
            if (++_depth > MaxDepth)
                throw new FormulaException(ErrorCode.TooDeep, _cur.Position, MaxDepth);
        }

        private void Leave() { _depth--; }

        /// <summary>Выражение: сложение и вычитание слагаемых.</summary>
        private Node ParseExpression()
        {
            Enter();
            Node left = ParseTerm();
            while (_cur.Type == TokenType.Plus || _cur.Type == TokenType.Minus)
            {
                char op = _cur.Type == TokenType.Plus ? '+' : '-';
                Advance();
                left = new BinaryNode(op, left, ParseTerm());
            }
            Leave();
            return left;
        }

        /// <summary>Слагаемое: умножение и деление множителей.</summary>
        private Node ParseTerm()
        {
            Node left = ParseFactor();
            while (_cur.Type == TokenType.Multiply || _cur.Type == TokenType.Divide)
            {
                char op = _cur.Type == TokenType.Multiply ? '*' : '/';
                Advance();
                left = new BinaryNode(op, left, ParseFactor());
            }
            return left;
        }

        /// <summary>Множитель: необязательный унарный знак перед степенью.</summary>
        private Node ParseFactor()
        {
            Enter();
            Node result;
            if (_cur.Type == TokenType.Minus) { Advance(); result = new NegateNode(ParseFactor()); }
            else if (_cur.Type == TokenType.Plus) { Advance(); result = ParseFactor(); }
            else result = ParsePower();
            Leave();
            return result;
        }

        /// <summary>Степень: основа и необязательный показатель (правая ассоциативность).</summary>
        private Node ParsePower()
        {
            Node b = ParseBase();
            if (_cur.Type == TokenType.Power)
            {
                Advance();
                return new BinaryNode('^', b, ParseFactor());
            }
            return b;
        }

        /// <summary>Основа: число, x, константа, функция или выражение в скобках.</summary>
        private Node ParseBase()
        {
            Token t = _cur;
            switch (t.Type)
            {
                case TokenType.Number:
                case TokenType.Constant:
                    Advance();
                    return new NumberNode(t.Value);
                case TokenType.Variable:
                    Advance();
                    return new VariableNode();
                case TokenType.Function:
                    Advance();
                    if (_cur.Type != TokenType.LeftParen)
                        throw new FormulaException(ErrorCode.ExpectedLParen, _cur.Position, t.Name, _cur.Position);
                    Advance();
                    Node arg = ParseExpression();
                    Expect(TokenType.RightParen);
                    return new FunctionNode(t.Name, arg);
                case TokenType.LeftParen:
                    Advance();
                    Node inner = ParseExpression();
                    Expect(TokenType.RightParen);
                    return inner;
                default:
                    throw new FormulaException(ErrorCode.ExpectedOperand, t.Position, t.Position);
            }
        }

        private void Expect(TokenType type)
        {
            if (_cur.Type != type)
                throw new FormulaException(ErrorCode.ExpectedRParen, _cur.Position, _cur.Position);
            Advance();
        }
    }
}
