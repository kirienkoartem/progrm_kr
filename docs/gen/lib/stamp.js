// Угловые штампы по ГОСТ 2.104-2006: форма 2 (большой, 185x40 мм) и форма 2а (малый, 185x15 мм).
const {
  Table, TableRow, TableCell, Paragraph, TextRun, AlignmentType, WidthType, BorderStyle,
  VerticalAlign, HeightRule, PageNumber, TableLayoutType,
} = require('docx');
const cfg = require('./../config');

const MM = 56.7; // twips в миллиметре
const mm = (v) => Math.round(v * MM);
const THIN = { style: BorderStyle.SINGLE, size: 4, color: '000000' };
const THICK = { style: BorderStyle.SINGLE, size: 12, color: '000000' };
const ROW_H = mm(5);

function run(text, size = 16, italics = true) {
  return new TextRun({ text, size, italics, font: cfg.font });
}

function cell(widthMm, text, o = {}) {
  const children = [];
  const content = o.children || (text !== undefined && text !== '' ? [run(text, o.size || 16, o.italics !== false)] : []);
  children.push(new Paragraph({
    alignment: o.align || AlignmentType.CENTER,
    spacing: { before: 0, after: 0, line: 240 },
    indent: { left: 0, right: 0, firstLine: 0 },
    children: content,
  }));
  return new TableCell({
    width: { size: mm(widthMm), type: WidthType.DXA },
    columnSpan: o.span,
    rowSpan: o.rowSpan,
    verticalAlign: VerticalAlign.CENTER,
    margins: { top: 0, bottom: 0, left: 28, right: 28 },
    borders: { top: THIN, bottom: THIN, left: THIN, right: THIN },
    children,
  });
}

function row(cells, heightMm = 5) {
  return new TableRow({ height: { value: mm(heightMm), rule: HeightRule.EXACT }, cantSplit: true, children: cells });
}

function sheetField(size = 20) {
  return [new TextRun({ children: [PageNumber.CURRENT], size, italics: true, font: cfg.font })];
}
function totalField(size = 16) {
  return [new TextRun({ children: [PageNumber.TOTAL_PAGES], size, italics: true, font: cfg.font })];
}

function wrap(grid, rows) {
  return new Table({
    width: { size: grid.reduce((a, b) => a + mm(b), 0), type: WidthType.DXA },
    columnWidths: grid.map(mm),
    layout: TableLayoutType.FIXED,
    indent: { size: -mm(5), type: WidthType.DXA }, // штамп начинается на линии рамки (20 мм), текст — с 25 мм
    borders: { top: THICK, bottom: THICK, left: THICK, right: THICK, insideHorizontal: THIN, insideVertical: THIN },
    rows,
  });
}

/** Малый штамп (форма 2а): 3 строки по 5 мм. */
function smallStamp() {
  const g = [7, 10, 23, 15, 10, 110, 10];
  const blank = (i) => cell(g[i], '');
  return wrap(g, [
    row([blank(0), blank(1), blank(2), blank(3), blank(4),
      cell(110, cfg.designation, { rowSpan: 3, size: 28 }), cell(10, 'Лист')]),
    row([blank(0), blank(1), blank(2), blank(3), blank(4),
      cell(10, '', { rowSpan: 2, children: sheetField(22) })]),
    row([cell(7, 'Изм.'), cell(10, 'Лист'), cell(23, '№ докум.'), cell(15, 'Подпись'), cell(10, 'Дата')]),
  ]);
}

/** Большой штамп (форма 2): 8 строк по 5 мм. */
function bigStamp() {
  const g = [7, 10, 23, 15, 10, 70, 15, 15, 20];
  const b = (i) => cell(g[i], '');
  const left = (label, name) => [
    cell(17, label, { span: 2, align: AlignmentType.LEFT }),
    cell(23, name || '', { size: 14 }), b(3), b(4),
  ];
  return wrap(g, [
    row([b(0), b(1), b(2), b(3), b(4), cell(120, cfg.designation, { span: 4, rowSpan: 3, size: 28 })]),
    row([b(0), b(1), b(2), b(3), b(4)]),
    row([cell(7, 'Изм.'), cell(10, 'Лист'), cell(23, '№ докум.'), cell(15, 'Подпись'), cell(10, 'Дата')]),
    row([...left('Разраб.', cfg.developer),
      cell(70, cfg.title, { rowSpan: 5, size: 24 }), cell(15, 'Лит.'), cell(15, 'Лист'), cell(20, 'Листов')]),
    row([...left('Провер.', cfg.supervisor), b(6),
      cell(15, '', { children: sheetField(18) }), cell(20, '', { children: totalField(16) })]),
    row([...left('Реценз.'), cell(50, cfg.org, { span: 3, rowSpan: 3, size: 24 })]),
    row(left('Н. контр.')),
    row(left('Утверд.')),
  ]);
}

module.exports = { smallStamp, bigStamp, mm };
