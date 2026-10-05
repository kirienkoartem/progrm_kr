# Курсовая работа: интегрирование функций, заданных формулой (вариант 7)

Язык — C# (.NET Framework 4.8 / WinForms). Дисциплина «Основы программирования», ДонГТУ, группа БИ-25.
План работ: [`PLAN.md`](PLAN.md), детальный план фазы 1: [`docs/phases/F1_core.md`](docs/phases/F1_core.md).

## Состояние

| Фаза | Статус |
|---|---|
| Ф1. Ядро вычислений (`Integral.Core`) | ✅ готово |
| Ф2. Материалы контрольной точки (`docs/kt/`) | ✅ готово |
| Ф3–Ф8 | см. `PLAN.md` |

## Структура

```text
Integral.sln
src/Integral.Core/   ядро: Lexer, Parser, Evaluator, Integrator, TaskFile, Messages
src/Integral.Cli/    консольная обвязка (демонстрация ядра)
tests/Integral.Tests/ автотесты xUnit
data/example.txt     пример файла исходных данных
tools/check_lines.sh проверка лимита 250 строк на файл
docs/kt/             материалы контрольной точки (.docx + .pdf-предпросмотр)
docs/gen/            генератор документов Word: рамки, штампы, блок-схемы (`sh docs/gen/build.sh`)
```

## Сборка и проверка

Нужен .NET SDK 8 (для Visual Studio 2022 — нагрузка «Разработка классических приложений .NET»).

```sh
dotnet build
dotnet test                       # автотесты ядра
sh tools/check_lines.sh           # лимит длины файла
dotnet run --project src/Integral.Cli -- data/example.txt
dotnet run --project src/Integral.Cli -- "x^2" 0 1 --method both --eps 1e-8
```

## Формат файла исходных данных

```text
# комментарий
function = 20*sin(sqrt(x)*3)
a        = 0
b        = 10
method   = both        # trapezoid | simpson | both
n        = 100
eps      = 1e-6        # необязательно
```

Формула: `+ - * / ^`, скобки, переменная `x`, константы `pi`, `e`, функции
`sin cos tg ctg arcsin arccos arctg exp ln lg sqrt abs`. Десятичный разделитель — `.` или `,`.
Неявное умножение (`2x`) не допускается.
