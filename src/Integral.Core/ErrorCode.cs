namespace Integral.Core
{
    /// <summary>Коды ошибок ядра. Тексты сообщений — в <see cref="Messages"/>.</summary>
    public enum ErrorCode
    {
        None = 0,

        // Лексический и синтаксический анализ формулы
        BadChar,
        BadNumber,
        UnknownIdent,
        ExpectedOperand,
        ExpectedRParen,
        ExpectedLParen,
        UnexpectedToken,
        EmptyExpression,
        TooDeep,

        // Вычисление значения функции
        DivByZero,
        LnDomain,
        SqrtDomain,
        AsinDomain,
        TanPole,
        PowDomain,
        NotFinite,

        // Параметры интегрирования
        BadN,
        BadEps,
        BadLimits,

        // Файл исходных данных
        FileEmpty,
        FileSyntax,
        FileUnknownKey,
        FileDuplicateKey,
        FileMissingKey,
        FileBadValue,
        FileBadFormula,
    }
}
