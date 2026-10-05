# Ф1. Ядро вычислений (C#) — детальный план

> Часть общего плана [`PLAN.md`](../../PLAN.md). Язык — **C#** (решение 05.10, Р-01).
> Сроки: старт по команде Артёма «Начинай, Джарвис», 5 рабочих дней (ориентир — до 12.10).
> Закрывает этап 4 «Код основного алгоритма» (КТ, сдаём 19.10) и даёт код для таблиц этапа 2.

## 1. Цель

Библиотека `Integral.Core` без единой формы. Она должна:
1. разбирать формулу `f(x)` в дерево выражения с ошибками по позиции;
2. вычислять дерево для любого `x` с контролем области определения;
3. интегрировать методами **трапеций** и **Симпсона** — при фиксированном `n` или до точности `ε` по правилу Рунге;
4. читать и записывать файл исходных данных `*.txt`.

Плюс консольная обвязка `Integral.Cli` для проверки и демонстрации, и автотесты.

**Не входит:** формы (Ф3–Ф4), HTML-отчёт (Ф4), справка (Ф5).

## 2. Решения уровня кода

| ID | Решение | Обоснование |
|---|---|---|
| Р1-01 | Ядро — `netstandard2.0`, подключается и к WinForms (`net48`), и к тестам (`net8.0`) | Одна библиотека, тестируется в облаке на Linux |
| Р1-02 | Ошибки — исключения `FormulaException` (код, позиция), `EvalException` (код, `x`), `TaskFileException` (код, строка). Базовый класс `IntegralException` | Т-22; форма ловит одно базовое исключение |
| Р1-03 | Все тексты сообщений — в `Messages.cs` (перечисление `ErrorCode` → русская фраза) | Т-23 |
| Р1-04 | Числа — `double.Parse(..., CultureInfo.InvariantCulture)` после замены `,` → `.` | Не зависим от региональных настроек Windows |
| Р1-05 | Имена — по соглашениям C#: `PascalCase` для типов и методов, `_camelCase` для полей. Для элементов форм в Ф3 — венгерские префиксы (`txtFormula`, `btnCalc`, `cmbMethod`) | Т-20: методичка требует венгерскую нотацию — применяем её там, где она уместна в C# (элементы управления), это обоснуем в ПЗ |
| Р1-06 | Файл ≤ 250 строк, XML-комментарии `///` у каждого публичного члена | Т-20, Т-21 |
| Р1-07 | Симпсон при нечётном `n` берёт `n + 1` и ставит флаг `NAdjusted` | Удобно и честно отражено в результате |
| Р1-08 | Предел удвоения `MaxN = 2²⁰`; если точность не достигнута — `Converged = false` | Нет зависаний |
| Р1-09 | Тесты — xUnit | Стандарт для .NET, понятен на защите |

## 3. Структура и классы

```text
Integral.sln
src/Integral.Core/
├── Messages.cs        enum ErrorCode, Messages.Text(code, ...)           ~110 строк
├── Exceptions.cs      IntegralException, FormulaException, EvalException, TaskFileException
├── Token.cs           enum TokenType, struct Token (тип, позиция, длина, значение)
├── Lexer.cs           Lexer.Next(): лексемы формулы                      ~180
├── Ast.cs             абстрактный Node; NumberNode, VarNode, UnaryNode, BinaryNode, FuncNode
├── Parser.cs          рекурсивный спуск по БНФ → Node                    ~200
├── Functions.cs       таблица функций и констант + проверки области определения
├── Evaluator.cs       Evaluate(Node, x)                                   ~120
├── Integrator.cs      Trapezoid, Simpson, Runge-адаптация → IntegrationResult  ~220
├── IntegrationResult.cs  результат + таблица сходимости
└── TaskFile.cs        IntegrationTask, Parse(string), ToText()           ~200
src/Integral.Cli/Program.cs    консольный запуск
tests/Integral.Tests/          LexerTests, ParserTests, EvaluatorTests, IntegratorTests, TaskFileTests
data/example.txt
```

Ключевые сигнатуры:
```csharp
public static Node Parser.Parse(string formula);                    // FormulaException
public static double Evaluator.Evaluate(Node root, double x);       // EvalException
public enum Method { Trapezoid, Simpson }
public sealed class IntegrationParams { double A, B; int N; double Eps; }   // Eps = 0 → фиксированное n
public static IntegrationResult Integrator.Integrate(Node f, IntegrationParams p, Method m);
public sealed class IntegrationResult {
    Method Method; double Value; int N; double H; double ErrorEstimate;   // NaN, если не считалась
    long Evaluations; double ElapsedMs; bool Converged; bool NAdjusted;
    IReadOnlyList<ConvergenceStep> Steps;   // n, I(n), оценка погрешности
}
public sealed class IntegrationTask { string Formula; double A, B; MethodChoice Method; int N; double Eps; }
public static IntegrationTask TaskFile.Parse(string text);           // TaskFileException
public static string TaskFile.ToText(IntegrationTask task);
```
`MethodChoice` = `Trapezoid | Simpson | Both`; режим «Оба» выполняет форма двумя вызовами.

Методы разбора соответствуют БНФ из `PLAN.md` §5.2 один к одному: `ParseExpression → ParseTerm → ParseFactor → ParsePower → ParseBase`. Это прямо станет блок-схемами раздела 2.3 ПЗ.

**Формулы:**
- трапеции `I = h·[(f₀+fₙ)/2 + Σfᵢ]`; при удвоении `n` переиспользуется старая сумма, считаются только новые середины;
- Симпсон `I = h/3·[f₀ + 4Σf_нечёт + 2Σf_чёт + fₙ]`;
- Рунге `R = |I₂ₙ − Iₙ| / (2ᵖ − 1)`, `p = 2 | 4`; удваиваем, пока `R > ε` и `n < MaxN`;
- `a > b` допустимо (знак), `a = b` → 0, `n < 1`, `ε < 0` → ошибка.

**Область определения:** деление на 0; `ln`/`lg` ≤ 0; `sqrt` < 0; `arcsin`/`arccos` вне [−1; 1]; полюса `tg`/`ctg`; `a^b` при `a < 0` и нецелом `b`; любой `NaN`/`∞` → ошибка со значением `x`.

## 4. Тесты (критерий готовности)

| Набор | Примеры |
|---|---|
| `LexerTests` | `"2,5e-3*X"` → Number(0.0025) Mul X; регистр `SIN`; `"2e"` → ошибка; `"@"` → недопустимый символ, поз. 1 |
| `ParserTests` | `2+3*4 = 14`; `2^3^2 = 512`; `-2^2 = -4`; `sin(x` → «ожидается ")"», поз. 6; `sn(x)` → неизвестное имя, поз. 1; `sqrt(x)sin(x)` → неожиданная лексема, поз. 8; пустая строка |
| `EvaluatorTests` | `sin(pi/2)=1`, `ln(e)=1`, `lg(100)=2`; `ln(x)` при −0,5; `1/x` при 0; `(-8)^(1/3)` |
| `IntegratorTests` | эталоны ниже + порядки точности + крайние случаи |
| `TaskFileTests` | корректный файл, умолчания, каждая ошибка с номером строки, BOM, CRLF, `ToText → Parse` даёт то же |

| f(x) | [a; b] | Эталон |
|---|---|---|
| `x^2` | [0; 1] | 1/3 (Симпсон точен) |
| `x^3 - 2*x + 1` | [−1; 2] | 3,75 (Симпсон точен) |
| `sin(x)` | [0; pi] | 2 |
| `ln(x)` | [1; e] | 1 |
| `4/(1+x^2)` | [0; 1] | π |
| `exp(-x^2)` | [0; 2] | 0,882081390762422 |
| `20*sin(sqrt(x)*3)` | [0; 10] | сверяем с независимым расчётом (Python) |
| `x` | [5; 5] и [1; 0] | 0 и −0,5 |
| `1/x` | [−1; 1] | ошибка «деление на ноль, x = 0» |

**Порядки:** `err(n)/err(2n)` ≈ 4 для трапеций и ≈ 16 для Симпсона (±10 %) — экспериментальное подтверждение теории для раздела 1.1 ПЗ.

## 5. Инфраструктура Ф1

| Файл | Назначение |
|---|---|
| `Integral.sln`, `*.csproj` | решение для Visual Studio и `dotnet` |
| `Directory.Build.props` | общие настройки: `LangVersion`, `Nullable`, `TreatWarningsAsErrors` |
| `.gitignore` | `bin/`, `obj/`, `.vs/` |
| `tools/check_lines.sh` | лимит 250 строк на файл |
| `README.md` | сборка, запуск, тесты |

## 6. Порядок работ

| День | Шаги |
|---|---|
| 1 | установка .NET SDK в облаке, решение и проекты, `Messages`, `Exceptions`, `Lexer` + тесты |
| 2 | `Ast`, `Parser` + тесты (приоритеты, позиции ошибок) |
| 3 | `Functions`, `Evaluator` + тесты области определения |
| 4 | `Integrator` + тесты: эталоны, порядки, Рунге, крайние случаи |
| 5 | `TaskFile` + тесты, `Integral.Cli`, `data/example.txt`, лимит строк, README, отчёт с разбором кода |

Коммит после каждого шага; тесты зелёные перед каждым коммитом. По завершении — уведомление в Telegram.

## 7. Готово, когда

- [ ] Все тесты проходят, `dotnet build` без предупреждений (в т.ч. сборка ядра под `net48`).
- [ ] Эталоны совпадают, порядки 2 и 4 подтверждены.
- [ ] Каждый `ErrorCode` покрыт тестом и даёт понятную русскую фразу.
- [ ] `Integral.Cli` считает `data/example.txt`.
- [ ] Ни один `.cs` не длиннее 250 строк, у публичных членов есть `///`-комментарии.
- [ ] Отчёт фазы с коротким разбором: лексер → парсер → дерево → интегрирование.

## 8. От Артёма

- Установить **Visual Studio Community 2022** с нагрузкой «Разработка классических приложений .NET» — понадобится с Ф3.
- Для обучения: после дня 4 самостоятельно написать метод трапеций (~20 строк) и прогнать через готовые тесты.

## 9. Риски

| Риск | Мера |
|---|---|
| .NET SDK не ставится в облаке (сеть) | Проверка в первый же час; запасной вариант — сборка у Артёма в VS |
| `net48` не собирается на Linux | `Microsoft.NETFramework.ReferenceAssemblies` из NuGet — штатный способ |
| `Parser.cs` превысит 250 строк | Таблица имён вынесена в `Functions.cs` |
