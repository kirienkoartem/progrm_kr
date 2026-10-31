// Титульный лист и бланк задания (без рамки и штампа).
const { Paragraph, TextRun, AlignmentType, Table, TableRow, TableCell, WidthType, BorderStyle, TableLayoutType, VerticalAlign } = require('docx');
const cfg = require('../config');
const B = require('../lib/blocks');

const run = (text, o = {}) => (text.startsWith('[') ? B.hl(text, o) : new TextRun({ text, font: cfg.font, size: o.size || 28, bold: o.bold }));
const line = (text, o = {}) => new Paragraph({
  alignment: o.align || AlignmentType.CENTER, spacing: { before: o.before || 0, after: o.after || 0, line: o.line || 360 },
  indent: { left: o.left || 0 }, keepNext: o.keepNext, children: Array.isArray(text) ? text : [run(text, o)],
});
/** «Подпись значение»: значение-заглушка в квадратных скобках подсвечивается. */
const labeled = (label, value, o = {}) => line([run(label + ' ', o), run(value, o)], o);

/** Титульный лист по образцу методички (прил. Б), шапка — российская. */
function titlePage() {
  return [
    line('МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ', { bold: true }),
    line('РОССИЙСКОЙ ФЕДЕРАЦИИ', { bold: true, after: 120 }),
    line('ФГБОУ ВО «ДОНБАССКИЙ ГОСУДАРСТВЕННЫЙ ТЕХНИЧЕСКИЙ УНИВЕРСИТЕТ»', { after: 240 }),
    labeled('Факультет', cfg.faculty),
    labeled('Кафедра', cfg.department, { after: 1900 }),
    line('ПОЯСНИТЕЛЬНАЯ ЗАПИСКА', { bold: true, after: 120 }),
    line('к курсовой работе', { after: 120 }),
    line('на тему «Интегрирование функций, заданных формулой»', { after: 120 }),
    line('по дисциплине «Основы программирования»', { after: 1500 }),
    line('Разработал: студент гр. ' + cfg.group + ' ' + cfg.studentFull, { align: AlignmentType.LEFT, left: 3900, after: 240, line: 300 }),
    line([run('Руководитель: ', { size: 28 }), run(cfg.supervisorTitle, { size: 28 }), run(' ' + cfg.supervisor, { size: 28 })], { align: AlignmentType.LEFT, left: 3900, line: 300 }),
    line(cfg.city + ' ' + cfg.year, { before: 1900 }),
  ];
}

const THIN = { style: BorderStyle.SINGLE, size: 4, color: '000000' };
const cell = (w, text, o = {}) => new TableCell({
  width: { size: w, type: WidthType.DXA }, verticalAlign: VerticalAlign.CENTER,
  margins: { top: 40, bottom: 40, left: 80, right: 80 }, borders: { top: THIN, bottom: THIN, left: THIN, right: THIN },
  children: [new Paragraph({ alignment: o.align || AlignmentType.LEFT, spacing: { line: 252 }, children: [new TextRun({ text, font: cfg.font, size: 24, bold: o.bold })] })],
});

const PLAN = [
  ['1', 'Описание задачи, разработка содержательной блок-схемы алгоритма программы', '1 неделя', ''],
  ['2', 'Определение и описание переменных, констант и подпрограмм (функций)', '3 неделя', 'КТ'],
  ['3', 'Разработка программы интерфейса', '5 неделя', ''],
  ['4', 'Разработка программного кода основного алгоритма', '7 неделя', 'КТ'],
  ['5', 'Разработка программного кода обработчиков событий', '9, 10 неделя', 'КТ'],
  ['6', 'Выполнение тестового примера', '11, 12 неделя', 'КТ'],
  ['7', 'Оформление записки объяснительной', '13 неделя', ''],
  ['8', 'Сдача работы на проверку', '14 неделя', 'КТ'],
  ['9', 'Защита курсовой работы', '15, 16 недели', 'КТ'],
];

/** Задание на курсовую работу и календарный план (по бланку задания кафедры). */
function assignmentPage() {
  const W = [600, 5922, 1900, 1500];
  const rows = [new TableRow({ tableHeader: true, children: ['№ п/п', 'Наименование этапов курсового проекта (работы)', 'Срок выполнения этапов', 'Примечание'].map((h, i) => cell(W[i], h, { bold: true, align: AlignmentType.CENTER })) })]
    .concat(PLAN.map((r) => new TableRow({ children: r.map((c, i) => cell(W[i], c, { align: i === 1 ? AlignmentType.LEFT : AlignmentType.CENTER })) })));
  return [
    line('ЗАДАНИЕ НА КУРСОВУЮ РАБОТУ', { bold: true, after: 120 }),
    line('по дисциплине «Основы программирования»', { after: 120 }),
    line('Студент: ' + cfg.studentName + ', группа ' + cfg.group, { align: AlignmentType.LEFT, line: 300 }),
    line('Вариант 7. Тема: «Интегрирование функций, заданных формулой».', { align: AlignmentType.LEFT, line: 300, after: 120 }),
    line('Разработать программу на языке высокого уровня (C#.NET), которая выполняет:', { align: AlignmentType.LEFT, line: 300 }),
    line('1. ввод данных из файла *.txt;', { align: AlignmentType.LEFT, line: 300, left: 400 }),
    line('2. выбор метода интегрирования из двух;', { align: AlignmentType.LEFT, line: 300, left: 400 }),
    line('3. вывод результатов на форму и в файл *.html.', { align: AlignmentType.LEFT, line: 300, left: 400, after: 120 }),
    line('Интерфейс: главная форма и подчинённые формы (MDI), главное меню, панель инструментов, строка состояния, справка о работе с программой.', { align: AlignmentType.LEFT, line: 300, after: 200 }),
    line('Календарный план', { bold: true, after: 100 }),
    new Table({ width: { size: W.reduce((a, b) => a + b, 0), type: WidthType.DXA }, columnWidths: W, layout: TableLayoutType.FIXED, rows }),
    line('Студент ______________  «____» ____________ 2026 г.', { align: AlignmentType.LEFT, before: 400, line: 300 }),
    line('Руководитель ______________ ' + cfg.supervisor + '  «____» ____________ 2026 г.', { align: AlignmentType.LEFT, before: 160, line: 300 }),
  ];
}

module.exports = { titlePage, assignmentPage };
