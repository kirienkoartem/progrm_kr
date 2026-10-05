// Общие блоки оформления по ГОСТ 7.32-2017: абзацы, заголовки, таблицы, подписи, листинги.
const {
  Paragraph, TextRun, AlignmentType, Table, TableRow, TableCell, WidthType, BorderStyle,
  TabStopType, VerticalAlign, TableLayoutType,
} = require('docx');
const cfg = require('../config');

const TEXT_W = 9922; // ширина текста: 175 мм
const THIN = { style: BorderStyle.SINGLE, size: 4, color: '000000' };

const t = (text, o = {}) => new TextRun({ text, font: cfg.font, size: o.size || 28, bold: o.bold, italics: o.italics });

/** Обычный абзац основного текста: 14 пт, интервал 1,5, отступ 1,25 см, по ширине. */
function para(text, o = {}) {
  const runs = Array.isArray(text) ? text : [t(text, o)];
  return new Paragraph({
    alignment: o.align || AlignmentType.JUSTIFIED,
    spacing: { line: o.line || 360, before: 0, after: o.after || 0 },
    indent: { firstLine: o.noIndent ? 0 : 709, left: o.left || 0 },
    keepNext: o.keepNext,
    children: runs,
  });
}

function heading1(text) {
  return new Paragraph({
    spacing: { line: 360, before: 240, after: 120 }, indent: { firstLine: 709 }, keepNext: true,
    outlineLevel: 0, children: [t(text, { bold: true })],
  });
}
function heading2(text) {
  return new Paragraph({
    spacing: { line: 360, before: 120, after: 120 }, indent: { firstLine: 709 }, keepNext: true,
    outlineLevel: 1, children: [t(text, { bold: true })],
  });
}

/** Маркированный список через дефис (по ГОСТ 7.32 — перечисления с тире). */
function dash(text) {
  return new Paragraph({
    alignment: AlignmentType.JUSTIFIED, spacing: { line: 360 }, indent: { left: 709, firstLine: 0 },
    children: [t('– ' + text)],
  });
}

/** Формула по центру с номером справа. */
function formula(text, num) {
  return new Paragraph({
    spacing: { line: 360, before: 60, after: 60 }, indent: { firstLine: 0 },
    tabStops: [{ type: TabStopType.CENTER, position: TEXT_W / 2 }, { type: TabStopType.RIGHT, position: TEXT_W }],
    children: [t('\t' + text + '\t(' + num + ')')],
  });
}

function tableCaption(text) {
  return new Paragraph({ spacing: { line: 360, before: 120, after: 60 }, indent: { firstLine: 0 }, keepNext: true, children: [t(text)] });
}
function figureCaption(text) {
  return new Paragraph({ alignment: AlignmentType.CENTER, spacing: { line: 360, after: 120 }, indent: { firstLine: 0 }, children: [t(text)] });
}

function tcell(w, text, o = {}) {
  return new TableCell({
    width: { size: w, type: WidthType.DXA },
    verticalAlign: VerticalAlign.CENTER,
    margins: { top: 20, bottom: 20, left: 80, right: 80 },
    borders: { top: THIN, bottom: THIN, left: THIN, right: THIN },
    children: [new Paragraph({
      alignment: o.align || AlignmentType.LEFT, spacing: { line: 240 }, indent: { firstLine: 0 },
      children: [new TextRun({ text: String(text), font: o.mono ? 'Courier New' : cfg.font, size: o.size || (o.mono ? 17 : 22), bold: o.bold })],
    })],
  });
}

/** Таблица с повторяющейся шапкой. widths — в twips, сумма = TEXT_W. */
function table(widths, head, rows, mono0 = false) {
  const hr = new TableRow({
    tableHeader: true, cantSplit: true,
    children: head.map((h, i) => tcell(widths[i], h, { bold: true, align: AlignmentType.CENTER })),
  });
  const body = rows.map((r) => new TableRow({
    cantSplit: true,
    children: r.map((c, i) => tcell(widths[i], c, { mono: mono0 && i === 0 })),
  }));
  return new Table({
    width: { size: widths.reduce((a, b) => a + b, 0), type: WidthType.DXA },
    columnWidths: widths, layout: TableLayoutType.FIXED, rows: [hr, ...body],
  });
}

function spacer() { return new Paragraph({ spacing: { line: 240, after: 0 }, children: [] }); }

/** Строка листинга: Courier New 8 пт. */
function codeLine(text) {
  return new Paragraph({
    spacing: { line: 200, before: 0, after: 0 }, indent: { firstLine: 0 },
    children: [new TextRun({ text: text.replace(/\t/g, '    ') || ' ', font: 'Courier New', size: 16 })],
  });
}

module.exports = { t, para, heading1, heading2, dash, formula, tableCaption, figureCaption, table, spacer, codeLine, TEXT_W };
