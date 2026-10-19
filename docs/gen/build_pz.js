// Сборка пояснительной записки (.docx): node build_pz.js out.docx headings.json [pages.json]
const fs = require('fs');
const { Document, Packer, Paragraph, TextRun, Header, Footer, AlignmentType } = require('docx');
const cfg = require('./config');
const { smallStamp, bigStamp } = require('./lib/stamp');
const B = require('./lib/blocks');
const front = require('./pz/front');
const part1 = require('./pz/part1');

const [out, headingsOut, pagesIn] = process.argv.slice(2);
const pages = pagesIn && fs.existsSync(pagesIn) ? JSON.parse(fs.readFileSync(pagesIn, 'utf8')) : { total: 0 };

// --- содержание частей (их заголовки попадают в реестр в порядке следования)
const body = [].concat(part1.introduction(), part1.section11(), part1.section12());

/** Реферат: числа берутся из документа; пока записка не закончена, они подсвечиваются как неокончательные. */
function abstract() {
  const num = (v) => (cfg.draft ? B.hl(String(v), { size: 28 }) : B.t(String(v)));
  return [
    B.headingPlain('Реферат', { newPage: false }),
    B.para([B.t('Пояснительная записка – '), num(pages.total || '…'), B.t(' с., количество источников – '), num(cfg.sources.length),
      B.t(', таблиц – '), num(B.counts.tables), B.t(', рисунков – '), num(B.counts.figures), B.t('.')]),
    B.para('Объект разработки – прикладное программное обеспечение, разработанное с использованием языка ' +
      'программирования высокого уровня C# (платформа .NET Framework 4.8, библиотека Windows Forms).'),
    B.para('Цель работы – разработка программы для вычисления определённых интегралов функций, заданных формулой, ' +
      'методами трапеций и Симпсона.'),
    B.para('Основными характеристиками программы «Интегрирование функций» являются: ввод формулы подынтегральной ' +
      'функции в виде строки с контролем синтаксиса и области определения; ввод исходных данных из текстового файла; ' +
      'выбор метода – трапеций, Симпсона или оба метода для сравнения; расчёт при заданном числе разбиений или с ' +
      'автоматическим подбором числа разбиений под заданную точность на основе правила Рунге; предупреждение при ' +
      'негладкой функции, для которой оценка погрешности занижена; вывод результатов в таблицы окна, на график и в ' +
      'файл формата HTML. Интерфейс программы многооконный (MDI) с главным меню, панелью инструментов, строкой ' +
      'состояния и справкой. Вычислительное ядро программы проверено автоматическими модульными тестами.'),
    B.para('ЧИСЛЕННОЕ ИНТЕГРИРОВАНИЕ, МЕТОД ТРАПЕЦИЙ, МЕТОД СИМПСОНА, ПРАВИЛО РУНГЕ, ЛЕКСИЧЕСКИЙ АНАЛИЗ, ' +
      'СИНТАКСИЧЕСКИЙ АНАЛИЗ, MDI-ИНТЕРФЕЙС', { noIndent: true, align: AlignmentType.JUSTIFIED }),
  ];
}

const contents = [B.headingPlain('Содержание'), new Paragraph({ children: [new TextRun({ text: '@@TOC@@', size: 2, font: cfg.font })] })];
const mainChildren = [].concat(abstract(), contents, body);

// порядок заголовков в содержании: сначала задание и реферат
const toc = [
  { level: 1, text: 'Задание и календарный план', find: 'ЗАДАНИЕ НА КУРСОВУЮ РАБОТУ' },
  { level: 1, text: 'Реферат', find: 'РЕФЕРАТ' },
].concat(B.registry);
fs.writeFileSync(headingsOut, JSON.stringify(toc, null, 1));

const mkHeader = () => new Header({ children: [new Paragraph({
  spacing: { line: 20, lineRule: 'exact', before: 0, after: 0 }, children: [new TextRun({ text: '@@FRAME@@', size: 2, font: cfg.font })] })] });
const tail = () => new Paragraph({ spacing: { line: 20, lineRule: 'exact', before: 0, after: 0 }, children: [new TextRun({ text: '', size: 2, font: cfg.font })] });
const A4 = { width: 11906, height: 16838 };

const doc = new Document({
  creator: cfg.studentFull, title: 'Пояснительная записка: интегрирование функций, заданных формулой',
  styles: {
    default: { document: { run: { font: cfg.font, size: 28 } } },
    paragraphStyles: [{ id: 'Normal', name: 'Normal', quickFormat: true, run: { font: cfg.font, size: 28 } }],
  },
  sections: [
    { properties: { page: { size: A4, margin: { top: 1134, bottom: 1134, left: 1701, right: 567 } } }, children: front.titlePage() },
    { properties: { page: { size: A4, margin: { top: 1134, bottom: 1134, left: 1701, right: 567 } } }, children: front.assignmentPage() },
    {
      properties: { titlePage: true, page: { size: A4, margin: { top: 567, bottom: 1417, left: 1417, right: 567, header: 0, footer: 283 } } },
      headers: { default: mkHeader(), first: mkHeader() },
      footers: {
        default: new Footer({ children: [smallStamp(), tail()] }),
        first: new Footer({ children: [bigStamp(), tail()] }),
      },
      children: mainChildren,
    },
  ],
});

Packer.toBuffer(doc).then((buf) => { fs.writeFileSync(out, buf); console.log('docx written:', out, buf.length, 'bytes'); });
