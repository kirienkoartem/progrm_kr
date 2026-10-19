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

/** Реестр заголовков (для содержания): заполняется при создании заголовков. */
const registry = [];
/** Счётчики подписей — для реферата (число таблиц и рисунков). */
const counts = { tables: 0, figures: 0 };

function heading1(text, o = {}) {
  registry.push({ level: 1, text, find: text });
  return new Paragraph({
    spacing: { line: 360, before: o.newPage ? 0 : 240, after: 120 }, indent: { firstLine: 709 }, keepNext: true,
    pageBreakBefore: !!o.newPage, outlineLevel: 0, children: [t(text, { bold: true })],
  });
}
function heading2(text) {
  registry.push({ level: 2, text, find: text });
  return new Paragraph({
    spacing: { line: 360, before: 120, after: 120 }, indent: { firstLine: 709 }, keepNext: true,
    outlineLevel: 1, children: [t(text, { bold: true })],
  });
}
function heading3(text) {
  return new Paragraph({
    spacing: { line: 360, before: 60, after: 60 }, indent: { firstLine: 709 }, keepNext: true,
    outlineLevel: 2, children: [t(text, { bold: true, italics: true })],
  });
}
/** Заголовок без номера (РЕФЕРАТ, СОДЕРЖАНИЕ, ВВЕДЕНИЕ…): по центру, прописными, с новой страницы. */
function headingPlain(text, o = {}) {
  if (o.toc) registry.push({ level: 1, text: o.toc, find: text.toUpperCase() });
  return new Paragraph({
    alignment: AlignmentType.CENTER, spacing: { line: 360, before: 0, after: 240 }, keepNext: true,
    pageBreakBefore: o.newPage !== false, outlineLevel: o.toc ? 0 : undefined, children: [t(text.toUpperCase(), { bold: true })],
  });
}
/** Заглушка с жёлтой подсветкой: данные, которые ещё нужно уточнить. */
function hl(text, o = {}) { return new TextRun({ text, font: cfg.font, size: o.size || 28, bold: o.bold, highlight: 'yellow' }); }

/** Рамка с текстом моноширинным шрифтом (грамматика, пример файла). */
function codeBlock(lines) {
  const kids = lines.map((l) => new Paragraph({
    spacing: { line: 252, before: 0, after: 0 }, indent: { firstLine: 0 },
    children: [new TextRun({ text: l || ' ', font: 'Courier New', size: 19 })],
  }));
  return new Table({
    width: { size: TEXT_W, type: WidthType.DXA }, columnWidths: [TEXT_W], layout: TableLayoutType.FIXED,
    rows: [new TableRow({ cantSplit: true, children: [new TableCell({
      width: { size: TEXT_W, type: WidthType.DXA },
      margins: { top: 100, bottom: 100, left: 160, right: 160 },
      borders: { top: THIN, bottom: THIN, left: THIN, right: THIN },
      children: kids,
    })] })],
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

/** Длинная формула в несколько строк: номер ставится у последней строки. */
function formulaLines(lines, num) {
  return lines.map((text, i) => new Paragraph({
    spacing: { line: 360, before: i === 0 ? 60 : 0, after: i === lines.length - 1 ? 60 : 0 }, indent: { firstLine: 0 },
    tabStops: [{ type: TabStopType.CENTER, position: TEXT_W / 2 }, { type: TabStopType.RIGHT, position: TEXT_W }],
    keepNext: i < lines.length - 1,
    children: [t('\t' + text + (i === lines.length - 1 ? '\t(' + num + ')' : ''))],
  }));
}

function tableCaption(text) {
  counts.tables++;
  return new Paragraph({ spacing: { line: 360, before: 120, after: 60 }, indent: { firstLine: 0 }, keepNext: true, children: [t(text)] });
}
function figureCaption(text) {
  counts.figures++;
  return new Paragraph({ alignment: AlignmentType.CENTER, spacing: { line: 360, after: 120 }, indent: { firstLine: 0 }, children: [t(text)] });
}

function tcell(w, text, o = {}) {
  return new TableCell({
    width: { size: w, type: WidthType.DXA },
    verticalAlign: VerticalAlign.CENTER,
    margins: { top: 20, bottom: 20, left: 80, right: 80 },
    borders: { top: THIN, bottom: THIN, left: THIN, right: THIN },
    children: [new Paragraph({
      alignment: o.align || AlignmentType.LEFT, spacing: { line: 240 }, indent: { firstLine: 0 }, keepNext: o.keep,
      children: [new TextRun({ text: String(text), font: o.mono ? 'Courier New' : cfg.font, size: o.size || (o.mono ? 17 : 22), bold: o.bold })],
    })],
  });
}

/** Таблица с повторяющейся шапкой. widths — в twips, сумма = TEXT_W. */
function table(widths, head, rows, mono0 = false) {
  const keepAll = rows.length <= 12; // небольшая таблица не разрывается между листами
  const hr = new TableRow({
    tableHeader: true, cantSplit: true,
    children: head.map((h, i) => tcell(widths[i], h, { bold: true, align: AlignmentType.CENTER, keep: keepAll })),
  });
  const body = rows.map((r, ri) => new TableRow({
    cantSplit: true,
    children: r.map((c, i) => tcell(widths[i], c, { mono: mono0 && i === 0, keep: keepAll && ri < rows.length - 1 })),
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

module.exports = { t, para, heading1, heading2, heading3, headingPlain, hl, codeBlock, registry, counts, dash, formula, formulaLines, tableCaption, figureCaption, table, spacer, codeLine, TEXT_W };
