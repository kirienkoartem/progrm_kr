using System;
using System.Globalization;

namespace Integral.Core
{
    /// <summary>Лексический анализатор формулы: выделяет лексемы по одной.</summary>
    public sealed class Lexer
    {
        private readonly string _src;
        private int _pos; // индекс следующего символа, с 0

        public Lexer(string source)
        {
            _src = source ?? string.Empty;
        }

        /// <summary>Возвращает следующую лексему; в конце строки — <see cref="TokenType.End"/>.</summary>
        public Token Next()
        {
            while (_pos < _src.Length && char.IsWhiteSpace(_src[_pos])) _pos++;
            if (_pos >= _src.Length) return new Token(TokenType.End, _pos + 1, 0);

            char c = _src[_pos];
            int pos1 = _pos + 1;
            switch (c)
            {
                case '+': _pos++; return new Token(TokenType.Plus, pos1, 1);
                case '-': _pos++; return new Token(TokenType.Minus, pos1, 1);
                case '*': _pos++; return new Token(TokenType.Multiply, pos1, 1);
                case '/': _pos++; return new Token(TokenType.Divide, pos1, 1);
                case '^': _pos++; return new Token(TokenType.Power, pos1, 1);
                case '(': _pos++; return new Token(TokenType.LeftParen, pos1, 1);
                case ')': _pos++; return new Token(TokenType.RightParen, pos1, 1);
            }
            if (IsDigit(c)) return ReadNumber();
            if (c == '.' || c == ',') throw new FormulaException(ErrorCode.BadNumber, pos1, pos1);
            if (IsLetter(c)) return ReadWord();
            throw new FormulaException(ErrorCode.BadChar, pos1, c, pos1);
        }

        /// <summary>Число: цифры [.|, цифры] [e|E [+|-] цифры].</summary>
        private Token ReadNumber()
        {
            int start = _pos;
            while (_pos < _src.Length && IsDigit(_src[_pos])) _pos++;
            if (_pos < _src.Length && (_src[_pos] == '.' || _src[_pos] == ','))
            {
                _pos++;
                if (_pos >= _src.Length || !IsDigit(_src[_pos]))
                    throw new FormulaException(ErrorCode.BadNumber, start + 1, start + 1);
                while (_pos < _src.Length && IsDigit(_src[_pos])) _pos++;
            }
            // Экспонента принимается, только если за ней идут цифры; иначе "e" — отдельная константа.
            if (_pos < _src.Length && (_src[_pos] == 'e' || _src[_pos] == 'E'))
            {
                int p = _pos + 1;
                if (p < _src.Length && (_src[p] == '+' || _src[p] == '-')) p++;
                if (p < _src.Length && IsDigit(_src[p]))
                {
                    while (p < _src.Length && IsDigit(_src[p])) p++;
                    _pos = p;
                }
            }
            string text = _src.Substring(start, _pos - start).Replace(',', '.');
            double value = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
            if (double.IsInfinity(value)) throw new FormulaException(ErrorCode.BadNumber, start + 1, start + 1);
            return new Token(TokenType.Number, start + 1, _pos - start, value);
        }

        /// <summary>Слово: x, константа, имя функции; регистр не учитывается.</summary>
        private Token ReadWord()
        {
            int start = _pos;
            while (_pos < _src.Length && (IsLetter(_src[_pos]) || IsDigit(_src[_pos]))) _pos++;
            string raw = _src.Substring(start, _pos - start);
            string word = raw.ToLowerInvariant();
            int len = _pos - start;
            if (word == "x") return new Token(TokenType.Variable, start + 1, len);
            if (Functions.TryGetConstant(word, out double c)) return new Token(TokenType.Constant, start + 1, len, c, word);
            if (Functions.IsFunction(word)) return new Token(TokenType.Function, start + 1, len, 0, word);
            throw new FormulaException(ErrorCode.UnknownIdent, start + 1, raw, start + 1);
        }

        private static bool IsDigit(char c) => c >= '0' && c <= '9';
        private static bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}
