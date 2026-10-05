// Сверка имён из таблиц КТ с исходным кодом: каждое имя должно встречаться в src/ как отдельное слово.
const fs = require('fs'), path = require('path');
const { variables, constants, routines } = require('./kt_data');
const files = [];
(function walk(d) { for (const f of fs.readdirSync(d)) { const p = path.join(d, f); if (fs.statSync(p).isDirectory()) { if (!/(bin|obj)$/.test(p)) walk(p); } else if (p.endsWith('.cs')) files.push(p); } })(path.join(__dirname, '../../src'));
const src = files.map((f) => fs.readFileSync(f, 'utf8')).join('\n');
let bad = 0;
for (const [name] of [...variables, ...constants, ...routines]) {
  const last = name.split('.').pop();
  if (!new RegExp('\\b' + last + '\\b').test(src)) { console.log('НЕ НАЙДЕНО в коде:', name); bad++; }
}
const enumBody = /enum ErrorCode\s*\{([^}]*)\}/s.exec(fs.readFileSync(path.join(__dirname, '../../src/Integral.Core/ErrorCode.cs'), 'utf8'))[1];
const cnt = enumBody.replace(/\/\/.*$/gm, '').split(',').map((s) => s.trim()).filter(Boolean).length;
console.log('ErrorCode значений:', cnt);
console.log(bad ? 'ОШИБКИ: ' + bad : 'OK: все имена таблиц есть в исходном коде (' + (variables.length + constants.length + routines.length) + ' шт.)');
process.exit(bad ? 1 : 0);
