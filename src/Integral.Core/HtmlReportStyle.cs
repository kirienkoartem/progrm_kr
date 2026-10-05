namespace Integral.Core
{
    /// <summary>Таблица стилей HTML-отчёта (встраивается в файл, внешних ресурсов нет).</summary>
    internal static class HtmlReportStyle
    {
        public const string Css = @"
:root{--ink:#1f2937;--muted:#6b7280;--accent:#2563eb;--soft:#eff6ff;--line:#e5e7eb;--warn:#9a3412;--warn-bg:#fff7ed;}
*{box-sizing:border-box}
body{margin:0;background:#f3f4f6;color:var(--ink);font:15px/1.5 'Segoe UI',Tahoma,Arial,sans-serif}
.page{max-width:1000px;margin:32px auto;background:#fff;border-radius:12px;box-shadow:0 1px 3px rgba(0,0,0,.08);padding:40px 48px}
.eyebrow{margin:0;color:var(--accent);font-size:13px;font-weight:600;letter-spacing:.04em;text-transform:uppercase}
h1{margin:6px 0 4px;font-size:28px;line-height:1.2}
h2{margin:36px 0 12px;font-size:19px;padding-bottom:6px;border-bottom:1px solid var(--line)}
.meta{margin:0;color:var(--muted);font-size:13px}
.formula{margin:20px 0 0;padding:16px 20px;background:var(--soft);border-radius:10px;font-size:18px}
.formula .int{font-family:'Times New Roman',serif;font-size:34px;font-style:italic;vertical-align:middle}
.formula .lim{display:inline-flex;flex-direction:column;font-size:13px;line-height:1.1;vertical-align:middle;margin:0 6px 0 2px}
.formula code{font:16px Consolas,'Courier New',monospace;background:#fff;border:1px solid var(--line);border-radius:6px;padding:2px 8px}
.cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:16px;margin-top:24px}
.card{border:1px solid var(--line);border-radius:10px;padding:16px 20px}
.card .label{color:var(--muted);font-size:13px}
.card .value{font-size:28px;font-weight:600;font-variant-numeric:tabular-nums;margin:2px 0}
.card .sub{color:var(--muted);font-size:13px}
.warn{margin-top:20px;padding:12px 16px;background:var(--warn-bg);color:var(--warn);border-left:4px solid var(--warn);border-radius:6px}
.warn p{margin:4px 0}
table{width:100%;border-collapse:collapse;font-size:14px}
th,td{padding:7px 10px;border-bottom:1px solid var(--line);text-align:left;vertical-align:top}
th{background:#f9fafb;font-weight:600;color:#374151}
td.n,th.n{text-align:right;font-variant-numeric:tabular-nums;white-space:nowrap}
table.wide{font-size:13px}table.wide th,table.wide td{padding:7px 6px}
td.sep{border-left:1px solid var(--line)}
.scroll{overflow-x:auto}
table.kv th{width:42%;background:none;font-weight:400;color:var(--muted)}
figure{margin:0}
figure svg{width:100%;height:auto;border:1px solid var(--line);border-radius:8px}
figcaption{margin-top:8px;color:var(--muted);font-size:13px;text-align:center}
.cols{display:grid;grid-template-columns:1fr 1fr;gap:24px}
footer{margin-top:40px;padding-top:12px;border-top:1px solid var(--line);color:var(--muted);font-size:12px}
@media (max-width:700px){.page{margin:0;border-radius:0;padding:24px 16px}.cols{grid-template-columns:1fr}}
@media print{body{background:#fff}.page{box-shadow:none;margin:0;padding:0;max-width:none}section,figure,table{break-inside:avoid}}
";
    }
}
