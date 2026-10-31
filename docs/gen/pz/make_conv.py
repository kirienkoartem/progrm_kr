"""Данные таблицы сходимости для записки: считает Integral.Cli (exp(-x^2) на [0; 2]) и пишет conv_data.js.
Запуск: python3 docs/gen/pz/make_conv.py   (из корня репозитория, после dotnet build src/Integral.Cli -c Release)"""
import json, math, subprocess, os
CLI = 'src/Integral.Cli/bin/Release/net8.0/Integral.Cli.dll'
EXACT = 0.8820813907624215          # (sqrt(pi)/2)*erf(2)
ru = lambda v, d: ('%.*f' % (d, v)).replace('.', ',').replace('-', '−')
def sci(v):
    e = int(math.floor(math.log10(abs(v)))); m = v / 10 ** e
    return ('%.2f' % m).replace('.', ',') + '·10' + ''.join('⁰¹²³⁴⁵⁶⁷⁸⁹'[int(c)] if c.isdigit() else '⁻' for c in str(e))
rows, prev = [], {}
for n in (4, 8, 16, 32, 64):
    r = [str(n)]
    for m in ('trapezoid', 'simpson'):
        out = subprocess.run(['dotnet', CLI, 'exp(-x^2)', '0', '2', '--method', m, '--n', str(n)], capture_output=True, text=True, env=dict(os.environ, DOTNET_CLI_TELEMETRY_OPTOUT='1')).stdout
        v = float([l for l in out.split('\n') if 'Интеграл' in l][0].split('= ')[1])
        err = abs(EXACT - v)
        r += [ru(v, 9), sci(err), (ru(prev[m] / err, 2) if m in prev else '—')]
        prev[m] = err
    rows.append(r)
open('docs/gen/pz/conv_data.js', 'w', encoding='utf-8').write('// Создано make_conv.py — не править вручную.\nmodule.exports = ' + json.dumps(rows, ensure_ascii=False, indent=1) + ';\n')
for r in rows: print(r)
