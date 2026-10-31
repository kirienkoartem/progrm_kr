"""Блок-схемы раздела 2.4 и схема модулей (координаты в мм внутри рисунка). Строятся по реальному коду."""
from ooxml import shape, line, polyline, label, figure_paragraph


class Chart:
    """Вертикальная цепочка блоков с автоматическими стрелками между соседними."""

    def __init__(self, cx):
        self.cx = cx
        self.c = []
        self.y = 0
        self.last = None

    def box(self, kind, text, h, w=60, gap=5, cx=None, y=None, link=True, size=10):
        cx = self.cx if cx is None else cx
        top = self.y if y is None else y
        if self.last is not None and y is None:
            top += gap
            if link:
                self.c.append(line(self.last[0], self.last[1] + self.last[2], cx, top, arrow=True))
        self.c.append(shape(kind, text, cx - w / 2, top, w, h, size))
        info = (cx, top, h, w)
        if y is None:
            self.y = top + h
            self.last = info
        return info

    def right_of(self, b, kind, text, w=62, h=None, dx=14, size=10):
        """Блок справа от b на одном уровне, со стрелкой от правой вершины b."""
        cx, top, bh, bw = b
        h = h or bh
        x = cx + bw / 2 + dx
        self.c.append(polyline([(cx + bw / 2, top + bh / 2), (x, top + bh / 2)]))
        self.c.append(shape(kind, text, x, top + (bh - h) / 2, w, h, size))
        return (x + w / 2, top + (bh - h) / 2, h, w)


def flow_lexer():
    """Алгоритм Lexer.Next."""
    ch = Chart(48)
    b0 = ch.box('terminator', 'Начало', 7, 34)
    ch.box('process', 'Пропустить пробелы', 8, 56)
    d1 = ch.box('decision', 'Конец\nстроки?', 15, 50, gap=5)
    ch.box('process', 'c := текущий символ', 8, 56)
    d2 = ch.box('decision', 'c – знак\n+ − * / ^ ( ) ?', 17, 62)
    d3 = ch.box('decision', 'c – цифра?', 12, 46)
    d4 = ch.box('decision', 'c – «.» или «,»?', 12, 56)
    d5 = ch.box('decision', 'c – буква?', 12, 46)
    e = ch.box('process', 'Ошибка BadChar\n(недопустимый символ)', 11, 56)
    end = ch.box('terminator', 'Конец', 7, 34, gap=14)
    # правая колонка
    r = [ch.right_of(d1, 'terminator', 'Лексема End', 40, 8, 18),
         ch.right_of(d2, 'process', 'Вернуть лексему знака;\nсдвиг на 1 символ', 62, 11, 18),
         ch.right_of(d3, 'subprocess', 'ReadNumber:\nчисло', 40, 11, 18),
         ch.right_of(d4, 'process', 'Ошибка BadNumber', 40, 8, 18),
         ch.right_of(d5, 'subprocess', 'ReadWord:\nx, константа, функция', 52, 11, 18)]
    bus = 168
    ys = []
    for (cx, top, h, w) in r:
        ys.append(top + h / 2)
        ch.c.append(line(cx + w / 2, top + h / 2, bus, top + h / 2))
    ymid = end[1] + end[2] / 2
    ch.c.append(line(bus, ys[0], bus, ymid))
    ch.c.append(polyline([(bus, ymid), (end[0] + end[3] / 2, ymid)]))
    ch.c.append(polyline([(e[0] + e[3] / 2, e[1] + e[2] / 2), (bus, e[1] + e[2] / 2)], arrow=False))
    c = ch.c
    c += [label('Да', d1[0] + d1[3] / 2 + 0.5, d1[1] + 3, 7), label('Нет', d1[0] + 1, d1[1] + d1[2] + 0.2, 8),
          label('Да', d2[0] + d2[3] / 2 + 0.5, d2[1] + 4, 7), label('Нет', d2[0] + 1, d2[1] + d2[2] + 0.2, 8),
          label('Да', d3[0] + d3[3] / 2 + 0.5, d3[1] + 2, 7), label('Нет', d3[0] + 1, d3[1] + d3[2] + 0.2, 8),
          label('Да', d4[0] + d4[3] / 2 + 0.5, d4[1] + 2, 7), label('Нет', d4[0] + 1, d4[1] + d4[2] + 0.2, 8),
          label('Да', d5[0] + d5[3] / 2 + 0.5, d5[1] + 2, 7), label('Нет', d5[0] + 1, d5[1] + d5[2] + 0.2, 8)]
    return figure_paragraph(172, ch.y + 2, c, 'Рисунок 2.6')


def flow_parse_expr():
    """Алгоритм ParseExpression (ParseTerm строится так же: операции * и /, подчинённый разбор ParseFactor)."""
    ch = Chart(55)
    ch.box('terminator', 'Начало', 7, 36)
    ch.box('process', 'Enter: вложенность + 1;\nесли больше 200 – ошибка TooDeep', 12, 80)
    ch.box('subprocess', 'left := ParseTerm()', 8, 56)
    d = ch.box('decision', 'Текущая лексема\n«+» или «−»?', 17, 62, gap=10)
    ch.box('process', 'Leave: вложенность − 1', 8, 56, gap=8)
    ch.box('io', 'Вернуть left', 8, 46)
    ch.box('terminator', 'Конец', 7, 36)
    c = ch.c
    # тело цикла справа
    b1 = ch.right_of(d, 'process', 'op := знак;  Advance()', 56, 8, 16)
    b2 = (b1[0], b1[1] + 14, 8, 56)
    c.append(shape('subprocess', 'right := ParseTerm()', b2[0] - 28, b2[1], 56, 8, 10))
    b3 = (b1[0], b2[1] + 14, 8, 56)
    c.append(shape('process', 'left := BinaryNode(op, left, right)', b3[0] - 28, b3[1], 56, 8, 10))
    c.append(line(b1[0], b1[1] + 8, b2[0], b2[1], arrow=True))
    c.append(line(b2[0], b2[1] + 8, b3[0], b3[1], arrow=True))
    # возврат к проверке цикла
    ytop = d[1] - 5
    xr = b3[0] + 28 + 5
    c.append(polyline([(b3[0] + 28, b3[1] + 4), (xr, b3[1] + 4), (xr, ytop), (d[0], ytop)]))
    c += [label('Да', d[0] + d[3] / 2 + 0.5, d[1] + 5, 7), label('Нет', d[0] + 1, d[1] + d[2] + 0.2, 8)]
    return figure_paragraph(172, ch.y + 2, c, 'Рисунок 2.7')


def flow_parse_base():
    """Алгоритм ParseBase (основа: число, x, константа, функция, скобки)."""
    ch = Chart(48)
    ch.box('terminator', 'Начало', 7, 34)
    d1 = ch.box('decision', 'Число или\nконстанта?', 15, 52)
    d2 = ch.box('decision', 'Переменная x?', 12, 52)
    d3 = ch.box('decision', 'Имя функции?', 12, 52, gap=22)
    d4 = ch.box('decision', '«(» ?', 12, 40, gap=44)
    e = ch.box('process', 'Ошибка ExpectedOperand', 9, 56, gap=22)
    end = ch.box('terminator', 'Конец', 7, 34, gap=14)
    r1 = ch.right_of(d1, 'process', 'NumberNode(значение);\nAdvance()', 52, 11, 18)
    r2 = ch.right_of(d2, 'process', 'VariableNode;  Advance()', 52, 9, 18)
    # ветка функции: после имени требуется «(»
    r3 = ch.right_of(d3, 'process', 'Advance(); если нет «(» –\nошибка ExpectedLParen', 62, 12, 18)
    c = ch.c
    r3b = (r3[0], r3[1] + 17, 10, 62)
    c.append(shape('subprocess', 'arg := ParseExpression()', r3b[0] - 31, r3b[1], 62, 8, 10))
    c.append(line(r3[0], r3[1] + r3[2], r3b[0], r3b[1], arrow=True))
    r3c = (r3[0], r3b[1] + 13, 10, 62)
    c.append(shape('process', 'Expect(«)»);  FunctionNode(имя, arg)', r3c[0] - 31, r3c[1], 62, 9, 10))
    c.append(line(r3b[0], r3b[1] + 8, r3c[0], r3c[1], arrow=True))
    r4 = ch.right_of(d4, 'subprocess', 'Advance();  inner := ParseExpression()', 62, 11, 18)
    r4b = (r4[0], r4[1] + 16, 9, 62)
    c.append(shape('process', 'Expect(«)»);  вернуть inner', r4b[0] - 31, r4b[1], 62, 8, 10))
    c.append(line(r4[0], r4[1] + r4[2], r4b[0], r4b[1], arrow=True))
    bus = 168
    ends = [r1, r2, (r3c[0], r3c[1], 9, 62), (r4b[0], r4b[1], 8, 62)]
    ymid = end[1] + end[2] / 2
    for (cx, top, h, w) in ends:
        c.append(line(cx + w / 2, top + h / 2, bus, top + h / 2))
    c.append(line(bus, ends[0][1] + ends[0][2] / 2, bus, ymid))
    c.append(polyline([(bus, ymid), (end[0] + end[3] / 2, ymid)]))
    c.append(polyline([(e[0] + e[3] / 2, e[1] + e[2] / 2), (bus, e[1] + e[2] / 2)], arrow=False))
    for d, dy in [(d1, 2), (d2, 2), (d3, 2), (d4, 2)]:
        c += [label('Да', d[0] + d[3] / 2 + 0.5, d[1] + dy, 7), label('Нет', d[0] + 1, d[1] + d[2] + 0.2, 8)]
    return figure_paragraph(172, max(ch.y, r4b[1] + 9) + 2, c, 'Рисунок 2.8')


def flow_eval():
    """Алгоритм Evaluator.Evaluate."""
    ch = Chart(48)
    ch.box('terminator', 'Начало', 7, 34)
    ch.box('io', 'Ввод: узел node, значение x', 9, 66)
    d1 = ch.box('decision', 'Число?', 11, 36)
    d2 = ch.box('decision', 'Переменная x?', 12, 50)
    d3 = ch.box('decision', 'Унарный\nминус?', 14, 46)
    d4 = ch.box('decision', 'Бинарная\nоперация?', 15, 50)
    d5 = ch.box('decision', 'Функция?', 11, 40)
    ch.box('process', 'Ошибка: неизвестный тип узла', 9, 66)
    end = ch.box('terminator', 'Конец', 7, 34, gap=14)
    r1 = ch.right_of(d1, 'process', 'Вернуть значение числа', 50, 8, 18)
    r2 = ch.right_of(d2, 'process', 'Вернуть x', 50, 8, 18)
    r3 = ch.right_of(d3, 'process', 'Вернуть −Evaluate(операнд, x)', 56, 8, 18)
    r4 = ch.right_of(d4, 'subprocess', 'Binary: левая и правая части,\nпроверки операции, Finite', 68, 12, 18)
    r5 = ch.right_of(d5, 'subprocess', 'Function: аргумент, TryApply,\nпроверка области, Finite', 68, 12, 18)
    bus = 172
    ymid = end[1] + end[2] / 2
    c = ch.c
    rs = [r1, r2, r3, r4, r5]
    for (cx, top, h, w) in rs:
        c.append(line(cx + w / 2, top + h / 2, bus, top + h / 2))
    c.append(line(bus, rs[0][1] + rs[0][2] / 2, bus, ymid))
    c.append(polyline([(bus, ymid), (end[0] + end[3] / 2, ymid)]))
    for d, dy in [(d1, 2), (d2, 2), (d3, 2), (d4, 3), (d5, 2)]:
        c += [label('Да', d[0] + d[3] / 2 + 0.5, d[1] + dy, 7), label('Нет', d[0] + 1, d[1] + d[2] + 0.2, 8)]
    return figure_paragraph(176, ch.y + 2, c, 'Рисунок 2.9')


def _sum_chart(title, init, body, finish, num):
    ch = Chart(52)
    ch.box('terminator', 'Начало', 7, 34)
    ch.box('io', 'Ввод: f, a, b, n', 8, 50)
    ch.box('process', 'h := (b − a) / n', 8, 50)
    ch.box('process', init, 8, 62)
    ch.box('process', 'i := 1', 8, 34)
    d = ch.box('decision', 'i < n ?', 12, 40)
    # тело цикла под условием
    b1 = ch.box('process', body, 11 if '\n' in body else 8, 62)
    b2 = ch.box('process', 'i := i + 1', 8, 34)
    fin = ch.box('process', finish, 8, 56, gap=10, link=False)
    out = ch.box('io', 'Вывод: I', 8, 34)
    end = ch.box('terminator', 'Конец', 7, 34)
    c = ch.c
    # возврат из цикла на проверку условия (слева)
    xl = d[0] - 40
    c.append(polyline([(b2[0] - b2[3] / 2, b2[1] + 4), (xl, b2[1] + 4), (xl, d[1] - 4), (d[0], d[1] - 4)]))
    # выход «Нет» вправо и вниз к блоку вычисления I
    xr = d[0] + 40
    c.append(polyline([(d[0] + d[3] / 2, d[1] + d[2] / 2), (xr, d[1] + d[2] / 2), (xr, fin[1] + 4), (fin[0] + fin[3] / 2, fin[1] + 4)]))
    c += [label('Да', d[0] + 0.5, d[1] + d[2] + 0.2, 7), label('Нет', d[0] + d[3] / 2 + 0.5, d[1] + 3, 8)]
    return figure_paragraph(104, ch.y + 2, c, num)


def flow_trapezoid():
    return _sum_chart('', 'sum := (f(a) + f(b)) / 2', 'sum := sum + f(a + i·h)', 'I := h · sum', 'Рисунок 2.11')


def flow_simpson():
    return _sum_chart('', 'sum := f(a) + f(b)', 'i нечётно: sum += 4·f(a + i·h)\nиначе: sum += 2·f(a + i·h)',
                      'I := h / 3 · sum', 'Рисунок 2.12')


def flow_modules():
    """Рисунок 2.1 – структура программы: интерфейс (Integral.App) и вычислительное ядро (Integral.Core)."""
    c = []
    c.append(shape('process', 'Integral.App – интерфейс (Windows Forms, .NET Framework 4.8)', 0, 0, 170, 8, 11))
    app = [('MainForm\nглавное окно, меню,\nпанель, состояние', 0), ('CalcForm\nокно расчёта:\nввод и результаты', 1),
           ('GraphForm,\nPlotPanel\nграфик функции', 2), ('HelpForm,\nAboutForm\nсправка, «О программе»', 3)]
    for text, i in app:
        c.append(shape('process', text, 2 + i * 42.5, 12, 40, 16, 10))
    c.append(shape('process', 'Integral.Core – вычислительное ядро (netstandard2.0, без форм)', 0, 62, 170, 8, 11))
    core = [('Lexer, Token,\nParser, Ast\nразбор формулы', 0), ('Evaluator,\nFunctions\nвычисление f(x)', 1),
            ('Integrator,\nIntegrationResult\nметоды, Рунге', 2), ('TaskFile,\nNumberParser\nфайл данных', 3)]
    for text, i in core:
        c.append(shape('subprocess', text, 2 + i * 42.5, 74, 40, 16, 10))
    core2 = [('PlotModel, AxisScale,\nSvgPlot\nданные и рисунок графика', 0), ('HtmlReport,\nHtmlFormat\nотчёт в HTML', 1),
             ('Messages, ErrorCode,\nExceptions\nсообщения и ошибки', 2)]
    for text, i in core2:
        c.append(shape('subprocess', text, 2 + i * 56.5, 96, 54, 16, 10))
    # связи: интерфейс использует ядро
    for i in range(4):
        c.append(line(22 + i * 42.5, 28, 22 + i * 42.5, 62, arrow=True))
    c.append(shape('process', 'Файлы *.txt, *.html', 52, 118, 66, 8, 10))
    c.append(line(85, 112, 85, 118, arrow=True))
    return figure_paragraph(172, 128, c, 'Рисунок 2.1')
